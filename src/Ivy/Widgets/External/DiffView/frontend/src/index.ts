/**
 * Ivy.Widgets.DiffView - Entry Point
 *
 * This file exports the widget components that will be loaded by the Ivy framework.
 */

import { DiffView } from "./DiffView";

// Explicitly assign to window for IIFE compatibility
if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_DiffView = {
    DiffView,
  };
}

export { DiffView };
