import type { JsonStreamRenderer, NormalizedEvent } from "../types";
import { splitLines, tryParse } from "./parse-util";

const SKIP_TOOLS = new Set(["update_topic"]);

interface GeminiNode {
  type?: string;
  model?: string;
  session_id?: string;
  role?: string;
  content?: string;
  delta?: boolean;
  tool_name?: string;
  tool_id?: string;
  parameters?: Record<string, unknown>;
  output?: string;
  status?: string;
  stats?: {
    duration_ms?: number;
    tool_calls?: number;
    input_tokens?: number;
    output_tokens?: number;
  };
}

export const geminiRenderer: JsonStreamRenderer = {
  name: "gemini",
  parseEvents(jsonStream: string): NormalizedEvent[] {
    const out: NormalizedEvent[] = [];
    const skippedToolIds = new Set<string>();
    const toolResults = new Map<string, string>();
    let accumulated = "";

    const flush = () => {
      if (accumulated.length > 0) {
        out.push({ kind: "assistant-text", text: accumulated });
        accumulated = "";
      }
    };

    for (const line of splitLines(jsonStream)) {
      const node = tryParse(line) as GeminiNode | null;
      if (!node) continue;

      switch (node.type) {
        case "init":
          out.push({
            kind: "system",
            subtype: "init",
            model: node.model ?? "gemini",
          });
          break;
        case "message":
          if (node.role !== "assistant") break;
          if (node.delta) {
            accumulated += node.content ?? "";
          } else {
            flush();
            if (node.content) out.push({ kind: "assistant-text", text: node.content });
          }
          break;
        case "tool_use": {
          const name = node.tool_name ?? "unknown";
          const id = node.tool_id ?? "";
          if (SKIP_TOOLS.has(name)) {
            if (id) skippedToolIds.add(id);
            break;
          }
          flush();
          out.push({
            kind: "tool-use",
            id,
            name,
            input: node.parameters ?? {},
            result: toolResults.get(id),
          });
          break;
        }
        case "tool_result": {
          const id = node.tool_id ?? "";
          if (skippedToolIds.delete(id)) break;
          const output = node.output ?? "";
          toolResults.set(id, output);
          for (const e of out) {
            if (e.kind === "tool-use" && e.id === id) e.result = output;
          }
          break;
        }
        case "result":
          flush();
          out.push({
            kind: "result",
            success: (node.status ?? "success") === "success",
            durationMs: node.stats?.duration_ms,
            tokensIn: node.stats?.input_tokens,
            tokensOut: node.stats?.output_tokens,
            numTurns: Math.max(1, node.stats?.tool_calls ?? 0),
          });
          break;
        default:
          break;
      }
    }
    flush();
    return out;
  },
};
