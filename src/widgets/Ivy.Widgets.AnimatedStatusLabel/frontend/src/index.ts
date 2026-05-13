import { AnimatedStatusLabel } from "./AnimatedStatusLabel";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_AnimatedStatusLabel = {
    AnimatedStatusLabel,
  };
}

export { AnimatedStatusLabel };
