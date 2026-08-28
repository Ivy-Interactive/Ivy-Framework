/**
 * Ambient declarations for the experimental WebMCP browser API.
 *
 * Spec: https://webmachinelearning.github.io/webmcp/ (W3C Web Machine Learning CG, not on the
 * standards track). Shipping in a Chrome origin trial. `navigator.modelContext` was the original
 * spelling and is deprecated as of Chrome 150 in favour of `document.modelContext`; both are
 * declared here so feature detection can cover either.
 */

interface WebMcpToolAnnotations {
  readOnlyHint?: boolean;
  untrustedContentHint?: boolean;
}

interface WebMcpContentBlock {
  type: "text";
  text: string;
}

interface WebMcpExecuteResult {
  content: WebMcpContentBlock[];
  isError?: boolean;
}

interface WebMcpToolDescriptor {
  name: string;
  description: string;
  title?: string;
  inputSchema?: object;
  annotations?: WebMcpToolAnnotations;
  execute: (input: Record<string, unknown>) => Promise<WebMcpExecuteResult>;
}

interface WebMcpRegisterToolOptions {
  signal?: AbortSignal;
  exposedTo?: string[];
}

interface WebMcpModelContext extends EventTarget {
  registerTool(tool: WebMcpToolDescriptor, options?: WebMcpRegisterToolOptions): Promise<void>;
  getTools(options?: { fromOrigins?: string[] }): Promise<unknown[]>;
}

interface Document {
  modelContext?: WebMcpModelContext;
}

interface Navigator {
  modelContext?: WebMcpModelContext;
}
