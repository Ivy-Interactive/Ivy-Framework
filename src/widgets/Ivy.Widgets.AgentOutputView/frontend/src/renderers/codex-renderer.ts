import type { JsonStreamRenderer, NormalizedEvent } from "../types";
import { splitLines, tryParse } from "./parse-util";

interface CodexNode {
  type?: string;
  thread_id?: string;
  item?: {
    type?: string;
    id?: string;
    text?: string;
    command?: string;
    aggregated_output?: string;
  };
  usage?: { input_tokens?: number; output_tokens?: number };
}

export const codexRenderer: JsonStreamRenderer = {
  name: "codex",
  parseEvents(jsonStream: string): NormalizedEvent[] {
    const out: NormalizedEvent[] = [];
    let initEmitted = false;

    for (const line of splitLines(jsonStream)) {
      const node = tryParse(line) as CodexNode | null;
      if (!node) continue;

      switch (node.type) {
        case "thread.started":
          if (!initEmitted) {
            initEmitted = true;
            out.push({ kind: "system", subtype: "init", model: "codex" });
          }
          break;
        case "item.completed": {
          const item = node.item;
          if (!item) break;
          if (item.type === "agent_message" && item.text) {
            out.push({ kind: "assistant-text", text: item.text });
          } else if (item.type === "command_execution") {
            const id = item.id ?? "";
            out.push({
              kind: "tool-use",
              id,
              name: "Bash",
              input: { command: item.command ?? "" },
              result: item.aggregated_output ?? "",
            });
          }
          break;
        }
        case "turn.completed":
          out.push({
            kind: "result",
            success: true,
            tokensIn: node.usage?.input_tokens,
            tokensOut: node.usage?.output_tokens,
            numTurns: 1,
          });
          break;
        default:
          break;
      }
    }
    return out;
  },
};
