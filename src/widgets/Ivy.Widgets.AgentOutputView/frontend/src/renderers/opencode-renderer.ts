import type { JsonStreamRenderer, NormalizedEvent } from "../types";
import { splitLines, tryParse } from "./parse-util";

interface OpenCodeNode {
  type?: string;
  sessionID?: string;
  part?: {
    tool?: string;
    callID?: string;
    state?: {
      input?: Record<string, unknown>;
      status?: string;
      output?: string;
    };
    text?: string;
    reason?: string;
    cost?: number;
    tokens?: { input?: number; output?: number };
  };
}

export const opencodeRenderer: JsonStreamRenderer = {
  name: "opencode",
  parseEvents(jsonStream: string): NormalizedEvent[] {
    const out: NormalizedEvent[] = [];
    let initEmitted = false;
    let accumulatedCost = 0;

    for (const line of splitLines(jsonStream)) {
      const node = tryParse(line) as OpenCodeNode | null;
      if (!node) continue;

      switch (node.type) {
        case "step_start":
          if (!initEmitted) {
            initEmitted = true;
            out.push({ kind: "system", subtype: "init", model: "opencode" });
          }
          break;
        case "tool_use": {
          const part = node.part;
          if (!part) break;
          const name = part.tool ?? "unknown";
          const id = part.callID ?? "";
          const state = part.state;
          const result = state?.status === "completed" ? (state.output ?? "") : undefined;
          out.push({
            kind: "tool-use",
            id,
            name,
            input: state?.input ?? {},
            result,
          });
          break;
        }
        case "text": {
          const text = node.part?.text ?? "";
          if (text) out.push({ kind: "assistant-text", text });
          break;
        }
        case "step_finish": {
          const part = node.part;
          accumulatedCost += part?.cost ?? 0;
          if (part?.reason !== "stop") break;
          out.push({
            kind: "result",
            success: true,
            tokensIn: part.tokens?.input,
            tokensOut: part.tokens?.output,
            costUsd: accumulatedCost,
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
