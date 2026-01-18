/**
 * Event handler type provided by Ivy to external widgets.
 */
export type EventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

/**
 * Stream subscription function provided by Ivy to external widgets.
 * Returns an unsubscribe function.
 */
export type StreamSubscriber = (
  streamId: string,
  onData: (data: unknown) => void
) => () => void;
