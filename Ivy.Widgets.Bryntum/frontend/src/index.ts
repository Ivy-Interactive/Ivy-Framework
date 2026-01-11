/**
 * Ivy.Widgets.Bryntum - Entry Point
 *
 * This file exports the widget components that will be loaded by the Ivy framework.
 */

import { Gantt } from './Gantt';

// Explicitly assign to window for IIFE compatibility
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_Bryntum = {
    Gantt,
  };
}

export { Gantt };
