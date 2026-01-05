import ErrorBoundary from '@/components/ErrorBoundary';
import React, { useRef, useEffect } from 'react';
import { loadingState, renderWidgetTree } from '../widgetRenderer';
import { useBackend } from '@/hooks/use-backend';
import { EventHandlerProvider } from '@/components/event-handler';

interface AppHostWidgetProps {
  id: string;
  appId: string;
  appArgs: string | null;
  parentId: string | null;
}

export const AppHostWidget: React.FC<AppHostWidgetProps> = ({
  appId,
  appArgs,
  parentId,
}) => {
  const { widgetTree, eventHandler } = useBackend(
    appId,
    appArgs,
    parentId,
    false
  );
  const containerRef = useRef<HTMLDivElement>(null);
  const previousAppIdRef = useRef<string>(appId);
  const hasResetForCurrentApp = useRef<boolean>(false);

  useEffect(() => {
    // Reset scroll only once when navigating to a new app and content is loaded
    if (
      containerRef.current &&
      widgetTree &&
      previousAppIdRef.current !== appId &&
      !hasResetForCurrentApp.current
    ) {
      containerRef.current.scrollTop = 0;
      previousAppIdRef.current = appId;
      hasResetForCurrentApp.current = true;
    }

    // Reset the flag when appId changes (for next navigation)
    if (previousAppIdRef.current !== appId) {
      hasResetForCurrentApp.current = false;
    }
  }, [widgetTree, appId]);

  return (
    <div ref={containerRef} className="w-full h-full p-4 overflow-y-auto">
      <ErrorBoundary>
        <EventHandlerProvider eventHandler={eventHandler}>
          <>{renderWidgetTree(widgetTree || loadingState())}</>
        </EventHandlerProvider>
      </ErrorBoundary>
    </div>
  );
};
