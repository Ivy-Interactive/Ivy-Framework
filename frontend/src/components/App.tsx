import { useEffect, useState } from 'react';
import { renderWidgetTree, loadingState } from '@/widgets/WidgetRenderer';
import { useBackend } from '@/hooks/use-backend';
import { Toaster } from '@/components/ui/toaster';
import { ErrorSheet } from '@/components/ErrorSheet';
import ErrorBoundary from './ErrorBoundary';
import MadeWithIvy from './MadeWithIvy';
import { getAppArgs, getAppId, getParentId } from '@/lib/utils';
import { hasLicensedFeature } from '@/lib/license';
import { ConnectionModal } from './ConnectionModal';
import { ThemeProvider } from './theme-provider';
import { EventHandlerProvider } from './event-handler';

export function App() {
  const appId = getAppId();
  const appArgs = getAppArgs();
  const parentId = getParentId();
  const { widgetTree, eventHandler, disconnected } = useBackend(
    appId,
    appArgs,
    parentId
  );
  const [removeBranding, setRemoveBranding] = useState(true);

  useEffect(() => {
    hasLicensedFeature('RemoveBranding').then(setRemoveBranding);
  }, []);

  return (
    <ThemeProvider defaultTheme="light" storageKey="ivy-ui-theme">
      <ErrorBoundary>
        <EventHandlerProvider eventHandler={eventHandler}>
          <>
            {!removeBranding && <MadeWithIvy />}
            {renderWidgetTree(widgetTree || loadingState())}
            <ErrorSheet />
            <Toaster />
            {disconnected && <ConnectionModal />}
          </>
        </EventHandlerProvider>
      </ErrorBoundary>
    </ThemeProvider>
  );
}
