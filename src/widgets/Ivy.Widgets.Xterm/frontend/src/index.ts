/**
 * Ivy.Widgets.Xterm - Entry Point
 *
 * This file exports the widget components that will be loaded by the Ivy framework.
 */

import { Terminal } from './Terminal';

// Explicitly assign to window for IIFE compatibility
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_Xterm = {
    Terminal,
  };
}

export { Terminal };
