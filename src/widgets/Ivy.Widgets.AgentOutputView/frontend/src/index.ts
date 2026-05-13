import { AgentOutputView } from "./AgentOutputView";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_AgentOutputView = {
    AgentOutputView,
  };
}

export { AgentOutputView };
