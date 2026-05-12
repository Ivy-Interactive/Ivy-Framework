import type { JsonStreamRenderer } from "../types";
import { claudeRenderer } from "./claude-renderer";
import { codexRenderer } from "./codex-renderer";
import { geminiRenderer } from "./gemini-renderer";
import { opencodeRenderer } from "./opencode-renderer";
import { copilotRenderer } from "./copilot-renderer";

const REGISTRY: Record<string, JsonStreamRenderer> = {
  claude: claudeRenderer,
  codex: codexRenderer,
  gemini: geminiRenderer,
  opencode: opencodeRenderer,
  copilot: copilotRenderer,
};

export function pickRenderer(provider: string | undefined): JsonStreamRenderer {
  const key = (provider ?? "claude").toLowerCase();
  return REGISTRY[key] ?? claudeRenderer;
}
