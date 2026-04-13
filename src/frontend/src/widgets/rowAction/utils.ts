import { MenuItem } from "@/types/widgets";

export type ActionRendererVariant = "default" | "ghost";

/**
 * Gets a unique identifier for an action (tag or label)
 */
export const getActionId = (action: MenuItem): string => {
  return action.tag?.toString() || action.label || "";
};

/**
 * Shared button styles for action buttons
 */
export const ACTION_BUTTON_CLASSES =
  "flex items-center justify-center p-1.5 rounded bg-secondary hover:bg-(--color-muted) transition-colors cursor-pointer border border-[var(--color-border)]";

export const ACTION_BUTTON_GHOST_CLASSES =
  "flex items-center justify-center p-1.5 rounded hover:bg-accent transition-colors cursor-pointer text-muted-foreground hover:text-foreground";

export const getActionButtonClasses = (variant: ActionRendererVariant = "default"): string =>
  variant === "ghost" ? ACTION_BUTTON_GHOST_CLASSES : ACTION_BUTTON_CLASSES;
