import ErrorBoundary from "@/components/ErrorBoundary";
import React, { useRef, useEffect } from "react";
import { loadingState, renderWidgetTree } from "../widgetRenderer";
import { useBackend } from "@/hooks/use-backend";
import { EventHandlerProvider } from "@/components/event-handler";
import { StreamHandlerProvider } from "@/components/stream-handler";
import { BreakpointProvider } from "@/hooks/use-breakpoint-context";

interface AppHostWidgetProps {
  id: string;
  appId: string;
  appArgs: string | null;
  parentId: string | null;
}

export const AppHostWidget: React.FC<AppHostWidgetProps> = ({ appId, appArgs, parentId }) => {
  const { widgetTree, eventHandler, subscribeToStream } = useBackend(
    appId,
    appArgs,
    parentId,
    false,
  );
  const containerRef = useRef<HTMLDivElement>(null);
  const previousAppIdRef = useRef(appId);

  useEffect(() => {
    if (!containerRef.current || widgetTree == null) return;
    if (previousAppIdRef.current === appId) return;

    const el = containerRef.current;
    el.scrollTop = 0;
    const parent = el.parentElement;
    if (parent) {
      const { overflowY } = getComputedStyle(parent);
      if (overflowY === "auto" || overflowY === "scroll") {
        parent.scrollTop = 0;
      }
    }
    previousAppIdRef.current = appId;
  }, [widgetTree, appId]);

  return (
    <div ref={containerRef} className="w-full h-full p-4 overflow-y-auto">
      <ErrorBoundary>
        <BreakpointProvider>
          <EventHandlerProvider eventHandler={eventHandler}>
            <StreamHandlerProvider subscribeToStream={subscribeToStream}>
              <>{renderWidgetTree(widgetTree || loadingState())}</>
            </StreamHandlerProvider>
          </EventHandlerProvider>
        </BreakpointProvider>
      </ErrorBoundary>
    </div>
  );
};
