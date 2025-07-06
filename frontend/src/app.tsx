import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './app.css';
import { renderWidgetTree, loadingState } from '@/widgets/WidgetRenderer';
import { useBackend } from '@/hooks/use-backend';
import { Toaster } from '@/components/ui/toaster';
import ErrorBoundary from './components/ErrorBoundary';
import { EventHandlerProvider } from './components/EventHandlerContext';
import { TextShimmer } from './components/TextShimmer';
import MadeWithIvy from './components/MadeWithIvy';
import { ThemeProvider } from './components/ThemeProvider';

function ConnectionModal() {
  return (
    <div
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
      }}
    >
      <div
        style={{
          padding: '1rem 2rem',
          backgroundColor: 'white',
          borderRadius: '4px',
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.2)',
        }}
      >
        <TextShimmer>Connection lost. Trying to reconnect...</TextShimmer>
      </div>
    </div>
  );
}

function App() {
  const { widgetTree, eventHandler, disconnected, removeIvyBranding } =
    useBackend();

  return (
    <ThemeProvider defaultTheme="light" storageKey="ivy-ui-theme">
      <ErrorBoundary>
        <EventHandlerProvider eventHandler={eventHandler}>
          <>
            {!removeIvyBranding && <MadeWithIvy />}
            {renderWidgetTree(widgetTree || loadingState())}
            <Toaster />
            {disconnected && <ConnectionModal />}
          </>
        </EventHandlerProvider>
      </ErrorBoundary>
    </ThemeProvider>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
