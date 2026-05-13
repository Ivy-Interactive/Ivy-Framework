import type { JsonStreamRenderer, NormalizedEvent } from "../types";
import { splitLines, tryParse } from "./parse-util";

interface CopilotNode {
  type?: string;
  data?: {
    model?: string;
    content?: string;
    messageId?: string;
    toolRequests?: Array<{
      toolCallId?: string;
      name?: string;
      arguments?: unknown;
    }>;
    toolCallId?: string;
    result?: { content?: string };
  };
  usage?: { totalApiDurationMs?: number; sessionDurationMs?: number };
}

function parseArguments(args: unknown): Record<string, unknown> {
  if (args && typeof args === "object") return args as Record<string, unknown>;
  if (typeof args === "string") {
    try {
      const parsed = JSON.parse(args);
      return typeof parsed === "object" && parsed !== null
        ? (parsed as Record<string, unknown>)
        : {};
    } catch {
      return {};
    }
  }
  return {};
}

export const copilotRenderer: JsonStreamRenderer = {
  name: "copilot",
  parseEvents(jsonStream: string): NormalizedEvent[] {
    const out: NormalizedEvent[] = [];
    let initEmitted = false;

    for (const line of splitLines(jsonStream)) {
      const node = tryParse(line) as CopilotNode | null;
      if (!node) continue;

      switch (node.type) {
        case "session.tools_updated":
          if (!initEmitted) {
            initEmitted = true;
            out.push({
              kind: "system",
              subtype: "init",
              model: node.data?.model ?? "copilot",
            });
          }
          break;
        case "assistant.message": {
          const d = node.data;
          if (!d) break;
          if (d.content) out.push({ kind: "assistant-text", text: d.content });
          for (const req of d.toolRequests ?? []) {
            const name = req.name ?? "unknown";
            if (name === "report_intent") continue;
            out.push({
              kind: "tool-use",
              id: req.toolCallId ?? "",
              name,
              input: parseArguments(req.arguments),
            });
          }
          break;
        }
        case "tool.execution_complete": {
          const id = node.data?.toolCallId ?? "";
          const content = node.data?.result?.content ?? "";
          for (const e of out) {
            if (e.kind === "tool-use" && e.id === id) e.result = content;
          }
          break;
        }
        case "result": {
          const sessionMs = node.usage?.sessionDurationMs ?? 0;
          const apiMs = node.usage?.totalApiDurationMs ?? 0;
          out.push({
            kind: "result",
            success: true,
            durationMs: sessionMs > 0 ? sessionMs : apiMs,
            numTurns: 1,
          });
          break;
        }
        default:
          break;
      }
    }
    return out;
  },
};
