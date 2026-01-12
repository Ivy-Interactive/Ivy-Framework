import React, { Suspense } from 'react';
import { useEventHandler } from '@/components/event-handler';

interface ExternalWidgetWrapperProps {
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

/**
 * Creates a wrapped version of an external widget component.
 */
export const wrapExternalWidget = (
  LazyComponent: React.LazyExoticComponent<
    React.ComponentType<Record<string, unknown>>
  >
): React.FC<Record<string, unknown>> => {
  const WrappedComponent: React.FC<Record<string, unknown>> = props => (
    <Suspense>
      <ExternalWidgetWrapper Component={LazyComponent} props={props}>
        {props.children as React.ReactNode}
      </ExternalWidgetWrapper>
    </Suspense>
  );

  WrappedComponent.displayName = 'ExternalWidget';
  return WrappedComponent;
};
