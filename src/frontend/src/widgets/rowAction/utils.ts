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
  "flex items-center justify-center p-1.5 rounded bg-secondary text-secondary-foreground hover:bg-muted hover:text-foreground transition-colors cursor-pointer border border-border";

export const ACTION_BUTTON_GHOST_CLASSES =
  "flex items-center justify-center p-1.5 rounded hover:bg-accent hover:text-accent-foreground transition-colors cursor-pointer text-muted-foreground";

export const getActionButtonClasses = (variant: ActionRendererVariant = "default"): string =>
  variant === "ghost" ? ACTION_BUTTON_GHOST_CLASSES : ACTION_BUTTON_CLASSES;
