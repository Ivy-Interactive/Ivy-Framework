import { useContext, useEffect, useRef } from 'react';
import { StreamSubscriber } from './types';
import { StreamHandlerContext } from './context';

export const useStreamSubscriber = (): StreamSubscriber => {
  const context = useContext(StreamHandlerContext);
  if (!context) {
    throw new Error(
      'useStreamSubscriber must be used within a StreamHandlerProvider'
    );
  }
  return context.subscribeToStream;
};

/**
 * Hook to subscribe to a stream and receive data.
 * Automatically handles subscription lifecycle.
 */
export const useStream = <T = unknown>(
  streamId: string | undefined,
  onData: (data: T) => void
): void => {
  const subscribeToStream = useStreamSubscriber();
  const callbackRef = useRef(onData);
  useEffect(() => {
    callbackRef.current = onData;
  });

  useEffect(() => {
    if (!streamId) return;

    const unsubscribe = subscribeToStream(streamId, data => {
      callbackRef.current(data as T);
    });

    return unsubscribe;
  }, [streamId, subscribeToStream]);
};
