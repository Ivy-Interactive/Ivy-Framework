import { StrictMode, useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import { renderWidgetTree, loadingState } from '@/widgets/WidgetRenderer';
import { useBackend } from '@/hooks/use-backend';
import { Toaster } from '@/components/ui/toaster';
import ErrorBoundary from './components/ErrorBoundary';
import { EventHandlerProvider } from './components/EventHandlerContext';
import { TextShimmer } from './components/TextShimmer';
import MadeWithIvy from './components/MadeWithIvy';
import { ThemeProvider } from './components/ThemeProvider';
import { getAppArgs, getAppId, getParentId } from './lib/utils';
import { hasLicensedFeature } from './lib/license';

function ConnectionModal() {
  return (
    <div className="fixed inset-0 bg-background/80 flex items-center justify-center z-[1000]">
      <div className="px-8 py-4 bg-card border border-border rounded-lg shadow-lg">
        <TextShimmer>Connection lost. Trying to reconnect...</TextShimmer>
      </div>
    </div>
  );
}

function App() {
  const appId = getAppId();
  const appArgs = getAppArgs();
  const parentId = getParentId();
  const { widgetTree, eventHandler, disconnected } = useBackend(
    appId,
    appArgs,
    parentId
  );
  const [removeBranding, setRemoveBranding] = useState(false);

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
            <Toaster />
            {disconnected && <ConnectionModal />}
          </>
        </EventHandlerProvider>
      </ErrorBoundary>
    </ThemeProvider>
  );
}

const container = document.getElementById('root');
if (!container) throw new Error('Failed to find root element');

interface WindowWithReactRoot extends Window {
  __reactRoot?: ReturnType<typeof createRoot>;
}

let root = (window as WindowWithReactRoot).__reactRoot;
if (!root) {
  root = createRoot(container);
  (window as WindowWithReactRoot).__reactRoot = root;
}

root.render(
  <StrictMode>
    <App />
  </StrictMode>
);
