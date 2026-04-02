import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { useStreamSubscriber } from "@/components/stream-handler";
import { logger } from "@/lib/logger";

export interface ExternalWidgetWrapperProps {
  Component: React.ComponentType<Record<string, unknown>>;
  props: Record<string, unknown>;
  children?: React.ReactNode;
}

/**
 * Wrapper component that provides the event handler and stream subscriber to external widgets.
 * External widgets receive:
 * - `eventHandler`: callback to trigger events to the backend
 * - `subscribeToStream`: callback to subscribe to server-to-client streams
 */
export const ExternalWidgetWrapper: React.FC<ExternalWidgetWrapperProps> = ({
  Component,
  props,
  children,
}) => {
  const eventHandler = useEventHandler();
  const subscribeToStream = useStreamSubscriber();

  // Debug: log when external widget gets events prop
  React.useEffect(() => {
    if (props.events && (props.events as string[]).length > 0) {
      logger.debug("[ExternalWidgetWrapper] Rendering with events:", {
        events: props.events,
        eventHandlerPresent: !!eventHandler,
      });
    }
  }, [props.events, eventHandler]);

  // Pass the event handler and stream subscriber as props so external widgets can use them
  const enhancedProps = {
    ...props,
    eventHandler,
    subscribeToStream,
  };

  return <Component {...enhancedProps}>{children}</Component>;
};
