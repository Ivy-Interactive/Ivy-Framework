import { ActivityHeatmap } from "./ActivityHeatmap";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_ActivityHeatmap = {
    ActivityHeatmap,
  };
}

export { ActivityHeatmap };
