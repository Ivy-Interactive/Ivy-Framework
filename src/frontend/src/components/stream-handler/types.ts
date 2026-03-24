/**
 * Function to subscribe to a stream.
 * Returns an unsubscribe function.
 */
export type StreamSubscriber = (streamId: string, onData: (data: unknown) => void) => () => void;

export interface StreamHandlerContextProps {
  subscribeToStream: StreamSubscriber;
}
