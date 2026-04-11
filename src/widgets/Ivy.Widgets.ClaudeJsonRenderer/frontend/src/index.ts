import { ClaudeJsonRenderer } from "./ClaudeJsonRenderer";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_ClaudeJsonRenderer = {
    ClaudeJsonRenderer,
  };
}

export { ClaudeJsonRenderer };
