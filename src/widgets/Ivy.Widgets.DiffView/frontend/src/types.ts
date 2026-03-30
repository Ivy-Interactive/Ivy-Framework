/**
 * Event handler type provided by Ivy to external widgets.
 */
export type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;
