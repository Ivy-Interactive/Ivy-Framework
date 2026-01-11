/**
 * Ivy.Widgets.Tiptap - Entry Point
 *
 * This file exports the widget components that will be loaded by the Ivy framework.
 */

import { TiptapInputWidget } from './TiptapInputWidget';

// Explicitly assign to window for IIFE compatibility
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).TiptapInput = {
    TiptapInputWidget,
    default: TiptapInputWidget,
  };
}

export { TiptapInputWidget };
export { TiptapInputWidget as default };
