/**
 * Ivy.Widgets.Tiptap - Entry Point
 *
 * This file exports the widget components that will be loaded by the Ivy framework.
 */

import { TiptapInput } from "./TiptapInput";

// Explicitly assign to window for IIFE compatibility
if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).TiptapInput = {
    TiptapInput,
  };
}

export { TiptapInput };
