/**
 * Ivy.Widgets.Leaflet - Entry Point
 *
 * This file exports the widget components that will be loaded by the Ivy framework.
 */

import { Map } from './Map';

// Explicitly assign to window for IIFE compatibility
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_Leaflet = {
    Map,
  };
}

export { Map };
