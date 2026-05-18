export type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;

export type NormalizedEvent =
  | { kind: "system"; subtype: string; model?: string }
  | { kind: "assistant-text"; text: string }
  | { kind: "thinking"; text: string }
  | {
      kind: "tool-use";
      id: string;
      name: string;
      input: Record<string, unknown>;
      result?: string;
      isError?: boolean;
    }
  | {
      kind: "result";
      success: boolean;
      text?: string;
      durationMs?: number;
      costUsd?: number;
      tokensIn?: number;
      tokensOut?: number;
      numTurns?: number;
    };

export interface JsonStreamRenderer {
  name: string;
  parseEvents(jsonStream: string): NormalizedEvent[];
}
