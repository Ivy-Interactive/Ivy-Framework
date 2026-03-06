import React from 'react';
import { useEventHandler } from '@/components/event-handler';

export interface ExternalWidgetWrapperProps {
  Component: React.ComponentType<Record<string, unknown>>;
  props: Record<string, unknown>;
  children?: React.ReactNode;
}

/**
 * Wrapper component that provides the event handler to external widgets.
 * External widgets receive the event handler as a prop called `onIvyEvent`.
 */
export const ExternalWidgetWrapper: React.FC<ExternalWidgetWrapperProps> = ({
  Component,
  props,
  children,
}) => {
  const eventHandler = useEventHandler();

  // Pass the event handler as a prop so external widgets can trigger events
  const enhancedProps = {
    ...props,
    onIvyEvent: eventHandler,
  };

  return <Component {...enhancedProps}>{children}</Component>;
};
