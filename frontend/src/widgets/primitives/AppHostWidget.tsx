import ErrorBoundary from '@/components/ErrorBoundary';
import { EventHandlerProvider } from '@/components/EventHandlerContext';
import React, { useLayoutEffect, useRef, useEffect } from 'react';
import { loadingState, renderWidgetTree } from '../WidgetRenderer';
import { useBackend } from '@/hooks/use-backend';

interface AppHostWidgetProps {
  id: string;
  appId: string;
  appArgs: string | null;
  parentId: string | null;
}

// Helper to create a unique key for each page
function getPageKey(
  appId: string,
  appArgs: string | null,
  parentId: string | null
) {
  return `${appId}::${appArgs ?? ''}::${parentId ?? ''}`;
}

export const AppHostWidget: React.FC<AppHostWidgetProps> = ({
  appId,
  appArgs,
  parentId,
}) => {
  const { widgetTree, eventHandler } = useBackend(appId, appArgs, parentId);
  const containerRef = useRef<HTMLDivElement>(null);
  const isPopNavigation = useRef(false);
  const scrollPositions = useRef<Map<string, number>>(new Map());
  const lastPageKey = useRef<string | null>(null);
  const currentPageKey = getPageKey(appId, appArgs, parentId);

  // Listen for popstate (back/forward navigation)
  useEffect(() => {
    const onPopState = () => {
      isPopNavigation.current = true;
    };
    window.addEventListener('popstate', onPopState);
    return () => window.removeEventListener('popstate', onPopState);
  }, []);

  // Save scroll position before widgetTree changes (before navigation)
  useEffect(() => {
    return () => {
      if (containerRef.current && lastPageKey.current) {
        const scrollTop = containerRef.current.scrollTop;
        scrollPositions.current.set(lastPageKey.current, scrollTop);
      }
    };
  }, [widgetTree]);

  // Save scroll position on scroll events
  useEffect(() => {
    const container = containerRef.current;
    if (!container || !currentPageKey) return;

    const handleScroll = () => {
      const scrollTop = container.scrollTop;
      scrollPositions.current.set(currentPageKey, scrollTop);
    };

    container.addEventListener('scroll', handleScroll);
    return () => container.removeEventListener('scroll', handleScroll);
  }, [currentPageKey]);

  // Restore scroll position after widgetTree changes (after navigation)
  useLayoutEffect(() => {
    if (containerRef.current && currentPageKey) {
      const saved = scrollPositions.current.get(currentPageKey);

      if (typeof saved === 'number') {
        containerRef.current.scrollTop = saved;
      } else if (!isPopNavigation.current) {
        containerRef.current.scrollTop = 0;
      }
      isPopNavigation.current = false;
    }
    lastPageKey.current = currentPageKey;
  }, [widgetTree, currentPageKey]);

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
