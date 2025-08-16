import ErrorBoundary from '@/components/ErrorBoundary';
import React, { useRef } from 'react';
import { loadingState, renderWidgetTree } from '../WidgetRenderer';
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
  const { widgetTree, eventHandler } = useBackend(appId, appArgs, parentId);
  const containerRef = useRef<HTMLDivElement>(null);

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
