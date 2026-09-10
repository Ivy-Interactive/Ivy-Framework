import { logger } from "./logger";

/**
 * Document-global bridge between Ivy's server-driven tool declarations and the experimental WebMCP
 * browser API (`document.modelContext`).
 *
 * Apps are not iframed — AppHostWidget mounts a second `useBackend`, so the AppShell and the hosted
 * app share one document and one `document.modelContext`. Registrations are therefore grouped by
 * scope, one per `useBackend` instance, and tool names are claimed document-wide.
 *
 * A scope is deliberately not a SignalR connection id: that changes on automatic reconnect, which
 * would strand the previous registrations here with no one left to release them.
 */

/** A tool as pushed from the server in a `WebMcpTools` message. */
export interface WebMcpToolMessage {
  toolId: string;
  name: string;
  title?: string | null;
  description: string;
  /** JSON Schema, serialized as a JSON string. */
  inputSchema?: string | null;
  readOnly: boolean;
  untrustedContent: boolean;
}

/** The server's answer to a tool call, in a `WebMcpToolResult` message. */
export interface WebMcpToolResultMessage {
  callId: string;
  isError: boolean;
  content: { type: string; text: string }[];
}

/** Sends a tool call to the server. Implemented by `useBackend` over `connection.invoke`. */
export type WebMcpCallSender = (callId: string, toolId: string, argumentsJson: string) => void;

/** Tells the server whether this browser can run WebMCP tools. */
export type WebMcpAvailabilityReporter = (available: boolean) => void;

interface PendingCall {
  resolve: (result: WebMcpExecuteResult) => void;
  timer: ReturnType<typeof setTimeout>;
}

interface RegisteredTool {
  /** Aborting this unregisters the tool. */
  controller: AbortController;
  /** Descriptor fingerprint, so an unchanged tool is never needlessly re-registered. */
  signature: string;
}

interface ScopeEntry {
  registered: Map<string, RegisteredTool>;
  pending: Map<string, PendingCall>;
  /** Last tool set the server asked for, replayed if the browser API shows up late. */
  desired?: { tools: WebMcpToolMessage[]; send: WebMcpCallSender };
  /**
   * Serializes syncs. `registerTool` is async, and the server sends one message per tool as views
   * mount, so overlapping syncs would collide on duplicate names and lose tools.
   */
  syncChain: Promise<void>;
}

/** A tool handler that never returns would leave the agent hanging; bound it instead. */
const CALL_TIMEOUT_MS = 60_000;

/**
 * A polyfill or extension can define `document.modelContext` after our first tools message lands.
 * Re-check on this bounded schedule rather than dropping the tools for the life of the page.
 */
const RETRY_DELAYS_MS = [250, 1000, 3000];

const scopes = new Map<string, ScopeEntry>();
/** Tool name to the scope that currently owns it. Names must be unique per document. */
const nameOwners = new Map<string, string>();
const availabilityReporters = new Map<string, WebMcpAvailabilityReporter>();

let callCounter = 0;
let retryAttempt = 0;
let retryTimer: ReturnType<typeof setTimeout> | null = null;
let unavailableReported = false;
let lastAvailability: boolean | null = null;

function getModelContext(): WebMcpModelContext | undefined {
  // `navigator.modelContext` was the original spelling and is deprecated as of Chrome 150.
  return document.modelContext ?? navigator.modelContext;
}

/** Why WebMCP is not usable right now, or null when it is. */
function getUnavailableReason(): string | null {
  const meta = document.querySelector('meta[name="ivy-webmcp"]');
  if (meta?.getAttribute("content") !== "true") {
    return "WebMCP is not enabled on the server. Call server.UseWebMcp() in Program.cs.";
  }
  if (typeof getModelContext()?.registerTool !== "function") {
    return (
      "This browser does not expose document.modelContext. WebMCP is behind a Chrome origin " +
      "trial. For local development enable chrome://flags/#enable-webmcp-testing and relaunch " +
      "Chrome (no token needed), or launch Chrome with --enable-features=WebMCPTesting. For a " +
      'deployed origin, pass an origin trial token to server.UseWebMcp(o => o.OriginTrialToken = "...").'
    );
  }
  return null;
}

/** True when the host enabled WebMCP and the browser actually implements it. */
export function isWebMcpAvailable(): boolean {
  return getUnavailableReason() === null;
}

/**
 * Registers a scope's channel for telling the server about WebMCP support, and reports the current
 * state immediately. The report is repeated if support appears later during the retry window.
 */
export function registerAvailabilityReporter(
  scopeId: string,
  report: WebMcpAvailabilityReporter,
): void {
  availabilityReporters.set(scopeId, report);
  const available = isWebMcpAvailable();
  lastAvailability = available;
  report(available);
}

/** Pushes a changed availability verdict to every scope. */
function notifyAvailability(): void {
  const available = isWebMcpAvailable();
  if (available === lastAvailability) return;

  lastAvailability = available;
  for (const report of availabilityReporters.values()) {
    report(available);
  }
}

function getEntry(scopeId: string): ScopeEntry {
  let entry = scopes.get(scopeId);
  if (!entry) {
    entry = { registered: new Map(), pending: new Map(), syncChain: Promise.resolve() };
    scopes.set(scopeId, entry);
  }
  return entry;
}

function unregisterTool(scopeId: string, entry: ScopeEntry, name: string): void {
  entry.registered.get(name)?.controller.abort();
  entry.registered.delete(name);
  if (nameOwners.get(name) === scopeId) {
    nameOwners.delete(name);
  }
}

/** Aborts every registration a scope owns. Leaves the entry itself in place. */
function unregisterAllTools(scopeId: string, entry: ScopeEntry): void {
  for (const [name, tool] of entry.registered) {
    tool.controller.abort();
    if (nameOwners.get(name) === scopeId) {
      nameOwners.delete(name);
    }
  }
  entry.registered.clear();
}

/** Fingerprint of everything that would require re-registering a tool. */
function toolSignature(tool: WebMcpToolMessage): string {
  return JSON.stringify([
    tool.toolId,
    tool.name,
    tool.title ?? null,
    tool.description,
    tool.inputSchema ?? null,
    tool.readOnly,
    tool.untrustedContent,
  ]);
}

/** Resolves every in-flight call as an error so no agent is left waiting. */
function failPendingCalls(entry: ScopeEntry, reason: string): void {
  for (const pending of entry.pending.values()) {
    clearTimeout(pending.timer);
    pending.resolve(errorResult(reason));
  }
  entry.pending.clear();
}

function parseInputSchema(tool: WebMcpToolMessage): object | undefined {
  if (!tool.inputSchema) return undefined;
  try {
    return JSON.parse(tool.inputSchema) as object;
  } catch (error) {
    logger.error("Failed to parse WebMCP input schema", { tool: tool.name, error });
    return undefined;
  }
}

/**
 * Brings a scope's registrations in line with `tools`. The server sends its full current set, and
 * one message per tool as views mount, so this diffs rather than rebuilding: unchanged tools are
 * left alone and only real additions and removals touch the browser.
 */
export function syncTools(
  scopeId: string,
  tools: WebMcpToolMessage[],
  send: WebMcpCallSender,
): void {
  const entry = getEntry(scopeId);
  entry.desired = { tools, send };
  applyDesiredTools(scopeId, entry);
}

/** Schedules a bounded re-check for a WebMCP implementation that appears after page load. */
function scheduleRetry(): void {
  if (retryTimer !== null || retryAttempt >= RETRY_DELAYS_MS.length) return;

  const delay = RETRY_DELAYS_MS[retryAttempt++];
  retryTimer = setTimeout(() => {
    retryTimer = null;
    notifyAvailability();
    for (const [scopeId, entry] of scopes) {
      if (entry.desired) applyDesiredTools(scopeId, entry);
    }
  }, delay);
}

function applyDesiredTools(scopeId: string, entry: ScopeEntry): void {
  // Queue behind any sync already in flight. registerTool is async, so overlapping syncs would
  // race each other onto the same tool names.
  entry.syncChain = entry.syncChain
    .then(() => syncScope(scopeId, entry))
    .catch((error) => logger.error("WebMCP sync failed", { scopeId, error }));
}

async function syncScope(scopeId: string, entry: ScopeEntry): Promise<void> {
  const desired = entry.desired;
  if (!desired) return;

  notifyAvailability();

  const unavailable = getUnavailableReason();
  if (unavailable !== null) {
    // Silence here is what makes "my tools never showed up" impossible to diagnose.
    if (!unavailableReported) {
      unavailableReported = true;
      logger.warn(`WebMCP tools were not registered. ${unavailable}`, {
        toolCount: desired.tools.length,
      });
    }
    scheduleRetry();
    return;
  }

  const modelContext = getModelContext();
  if (!modelContext) return;

  const { tools, send } = desired;
  const wanted = new Map(tools.map((tool) => [tool.name, tool]));

  // Drop anything gone or changed. Collect first: deleting mid-iteration is a footgun.
  const stale: string[] = [];
  for (const [name, current] of entry.registered) {
    const tool = wanted.get(name);
    if (!tool || toolSignature(tool) !== current.signature) stale.push(name);
  }
  for (const name of stale) {
    unregisterTool(scopeId, entry, name);
  }

  for (const tool of tools) {
    if (entry.registered.has(tool.name)) continue;

    const previousOwner = nameOwners.get(tool.name);
    if (previousOwner !== undefined && previousOwner !== scopeId) {
      // Two scopes in one document claimed the same name. Last registration wins.
      if (process.env.NODE_ENV === "development") {
        logger.warn("Duplicate WebMCP tool name across scopes; last registration wins", {
          tool: tool.name,
          previousOwner,
          scopeId,
        });
      }
      const owner = scopes.get(previousOwner);
      if (owner) unregisterTool(previousOwner, owner, tool.name);
    }

    const controller = new AbortController();
    const inputSchema = parseInputSchema(tool);

    try {
      // Awaited so the map only ever claims tools the browser actually accepted.
      await modelContext.registerTool(
        {
          name: tool.name,
          description: tool.description,
          ...(tool.title ? { title: tool.title } : {}),
          ...(inputSchema ? { inputSchema } : {}),
          annotations: {
            readOnlyHint: tool.readOnly,
            untrustedContentHint: tool.untrustedContent,
          },
          execute: (input) => invokeTool(scopeId, tool.toolId, input, send),
        },
        { signal: controller.signal },
      );

      entry.registered.set(tool.name, { controller, signature: toolSignature(tool) });
      nameOwners.set(tool.name, scopeId);
    } catch (error) {
      // Registration can fail on permissions policy or a name the browser rejects. Leave the name
      // unclaimed rather than unregistering it — another scope may legitimately own it.
      logger.error("Failed to register WebMCP tool", { tool: tool.name, error });
    }
  }
}

function invokeTool(
  scopeId: string,
  toolId: string,
  input: Record<string, unknown>,
  send: WebMcpCallSender,
): Promise<WebMcpExecuteResult> {
  const entry = getEntry(scopeId);
  const callId = `${++callCounter}`;

  return new Promise<WebMcpExecuteResult>((resolve) => {
    const timer = setTimeout(() => {
      entry.pending.delete(callId);
      resolve(errorResult("The tool did not respond in time."));
    }, CALL_TIMEOUT_MS);

    entry.pending.set(callId, { resolve, timer });

    try {
      send(callId, toolId, JSON.stringify(input ?? {}));
    } catch (error) {
      clearTimeout(timer);
      entry.pending.delete(callId);
      logger.error("Failed to send WebMCP tool call", { toolId, error });
      resolve(errorResult("The tool could not be reached."));
    }
  });
}

/** Completes the pending call a `WebMcpToolResult` message refers to. */
export function resolveToolCall(scopeId: string, message: WebMcpToolResultMessage): void {
  const entry = scopes.get(scopeId);
  const pending = entry?.pending.get(message.callId);
  if (!entry || !pending) return;

  clearTimeout(pending.timer);
  entry.pending.delete(message.callId);
  pending.resolve({
    content: (message.content ?? []).map((block) => ({
      type: "text",
      text: block.text,
    })),
    isError: message.isError,
  });
}

/**
 * Drops everything a scope owns. Pending calls resolve as errors rather than hanging, which is what
 * losing the app mid-call must look like to the agent.
 */
export function releaseScope(scopeId: string): void {
  const entry = scopes.get(scopeId);
  if (!entry) return;

  unregisterAllTools(scopeId, entry);
  failPendingCalls(entry, "The connection to the application was lost.");
  scopes.delete(scopeId);
  availabilityReporters.delete(scopeId);
}

function errorResult(text: string): WebMcpExecuteResult {
  return { content: [{ type: "text", text }], isError: true };
}

/** For testing only. Clears all registrations and pending calls. */
export function _resetForTesting(): void {
  for (const [scopeId, entry] of scopes) {
    unregisterAllTools(scopeId, entry);
    failPendingCalls(entry, "Reset.");
  }
  scopes.clear();
  nameOwners.clear();
  availabilityReporters.clear();
  callCounter = 0;
  retryAttempt = 0;
  unavailableReported = false;
  lastAvailability = null;
  if (retryTimer !== null) {
    clearTimeout(retryTimer);
    retryTimer = null;
  }
}

/** For testing only. Number of tools currently registered across all scopes. */
export function _getRegistrySize(): number {
  let total = 0;
  for (const entry of scopes.values()) total += entry.registered.size;
  return total;
}
