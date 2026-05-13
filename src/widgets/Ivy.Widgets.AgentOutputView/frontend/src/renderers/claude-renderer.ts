import type { JsonStreamRenderer, NormalizedEvent } from "../types";
import { contentToString, splitLines, tryParse } from "./parse-util";

interface ClaudeContentBlock {
  type: string;
  text?: string;
  thinking?: string;
  id?: string;
  name?: string;
  input?: Record<string, unknown>;
  tool_use_id?: string;
  content?: unknown;
}

interface ClaudeMessage {
  content?: ClaudeContentBlock[];
}

interface ClaudeEvent {
  type?: string;
  subtype?: string;
  model?: string;
  message?: ClaudeMessage;
  tool_use_result?: { stdout?: unknown; content?: unknown };
  is_error?: boolean;
  result?: string;
  duration_ms?: number;
  cost_usd?: number;
  total_cost_usd?: number;
  num_turns?: number;
  usage?: { input_tokens?: number; output_tokens?: number };
}

export const claudeRenderer: JsonStreamRenderer = {
  name: "claude",
  parseEvents(jsonStream: string): NormalizedEvent[] {
    const raw = splitLines(jsonStream)
      .map((line) => tryParse(line) as ClaudeEvent | null)
      .filter((e): e is ClaudeEvent => e !== null);

    // First pass: collect tool results keyed by tool_use_id
    const toolResults = new Map<string, string>();
    for (const e of raw) {
      if (e.type !== "user") continue;
      const blocks = e.message?.content ?? [];
      for (const b of blocks) {
        if (b.type === "tool_result" && b.tool_use_id) {
          const text =
            contentToString(b.content) ||
            contentToString(e.tool_use_result?.stdout) ||
            contentToString(e.tool_use_result?.content) ||
            "";
          toolResults.set(b.tool_use_id, text);
        }
      }
    }

    // Second pass: emit normalized events in order; skip user events (their results are folded in)
    const out: NormalizedEvent[] = [];
    for (const e of raw) {
      if (e.type === "system") {
        out.push({ kind: "system", subtype: e.subtype ?? "", model: e.model });
        continue;
      }
      if (e.type === "assistant") {
        const blocks = e.message?.content;
        if (!Array.isArray(blocks)) continue;
        for (const b of blocks) {
          if (b.type === "text" && b.text) {
            out.push({ kind: "assistant-text", text: b.text });
          } else if (b.type === "thinking" && b.thinking) {
            out.push({ kind: "thinking", text: b.thinking });
          } else if (b.type === "tool_use" && b.name) {
            const id = b.id ?? "";
            out.push({
              kind: "tool-use",
              id,
              name: b.name,
              input: b.input ?? {},
              result: id ? toolResults.get(id) : undefined,
            });
          }
        }
        continue;
      }
      if (e.type === "result") {
        const isError = !!e.is_error || e.subtype === "error";
        out.push({
          kind: "result",
          success: !isError,
          text: e.result,
          durationMs: e.duration_ms,
          costUsd: e.cost_usd ?? e.total_cost_usd,
          tokensIn: e.usage?.input_tokens,
          tokensOut: e.usage?.output_tokens,
          numTurns: e.num_turns,
        });
      }
    }
    return out;
  },
};
