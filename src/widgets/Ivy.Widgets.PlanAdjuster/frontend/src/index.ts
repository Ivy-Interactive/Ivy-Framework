import { PlanAdjuster } from "./PlanAdjuster";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_PlanAdjuster = {
    PlanAdjuster,
  };
}

export { PlanAdjuster };
