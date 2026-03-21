import { useEffect, useState } from "react";
import { renderWidgetTree, loadingState } from "@/widgets/widgetRenderer";
import { useBackend } from "@/hooks/use-backend";
import { Toaster } from "@/components/ui/toaster";
import { ErrorSheet } from "@/components/ErrorSheet";
import ErrorBoundary from "./ErrorBoundary";
import MadeWithIvy from "./MadeWithIvy";
import { DevTools } from "./DevTools";
import {
  getAppArgs,
  getAppId,
  getChromeParam,
  getParentId,
  wrapAppContent,
  isDevToolsEnabled,
} from "@/lib/utils";
import { hasLicensedFeature } from "@/lib/license";
import { ConnectionModal } from "./ConnectionModal";
import { ThemeProvider } from "./theme-provider";
import { EventHandlerProvider } from "./event-handler";
import { StreamHandlerProvider } from "./stream-handler";

export function App() {
  const appId = getAppId();
  const appArgs = getAppArgs();
  const parentId = getParentId();
  const chrome = getChromeParam();

  const { connection, widgetTree, eventHandler, subscribeToStream, disconnected } = useBackend(
    appId,
    appArgs,
    parentId,
    chrome,
  );
  const [removeBranding, setRemoveBranding] = useState(true);

  useEffect(() => {
    hasLicensedFeature("RemoveBranding").then(setRemoveBranding);
  }, []);

  useEffect(() => {
    const handlePopState = (event: PopStateEvent) => {
      const chrome = getChromeParam();
      if (chrome) {
        const newAppId = getAppId();
        connection?.invoke("Navigate", newAppId, event.state).catch((err) => {
          console.error("SignalR Error when sending Navigate:", err);
        });
      }
    };

    window.addEventListener("popstate", handlePopState);
    return () => {
      window.removeEventListener("popstate", handlePopState);
    };
  }, [connection]);

  return (
    <ThemeProvider defaultTheme="system" storageKey="ivy-ui-theme">
      <ErrorBoundary>
        <EventHandlerProvider eventHandler={eventHandler}>
          <StreamHandlerProvider subscribeToStream={subscribeToStream}>
            <>
              {!removeBranding && <MadeWithIvy />}
              {isDevToolsEnabled() && <DevTools />}
              {wrapAppContent(renderWidgetTree(widgetTree || loadingState()))}
              <ErrorSheet />
              <Toaster />
              {disconnected && <ConnectionModal />}
            </>
          </StreamHandlerProvider>
        </EventHandlerProvider>
      </ErrorBoundary>
    </ThemeProvider>
  );
}
