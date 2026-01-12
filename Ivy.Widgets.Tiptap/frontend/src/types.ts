/**
 * Event handler type provided by Ivy to external widgets.
 */
export type IvyEventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;
