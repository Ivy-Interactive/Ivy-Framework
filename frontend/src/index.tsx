import { StrictMode, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from '@/components/ui/resizable';
import './index.css';
import React from 'react';

function App() {
  const [chatPanelUrl, setChatPanelUrl] = React.useState('');

  useEffect(() => {
    const handleMessage = (event: MessageEvent) => {
      if (event.data.type === '$SetChatPanelUrl') {
        setChatPanelUrl(event.data.url);
      }
    };
    window.addEventListener('message', handleMessage);
    return () => window.removeEventListener('message', handleMessage);
  }, []);

  if (!chatPanelUrl) {
    return (
      <div className="h-screen w-screen">
        <iframe src="app.html" className="w-full h-full border-none" />
      </div>
    );
  }

  return (
    <ResizablePanelGroup direction="horizontal" className="h-screen v-screen">
      <ResizablePanel defaultSize={75}>
        <div className="h-screen w-full">
          <iframe src="app.html" className="w-full h-full border-none" />
        </div>
      </ResizablePanel>

      <ResizableHandle className="border" />

      <ResizablePanel defaultSize={25} minSize={15}>
        <div className="h-screen w-full">
          <iframe src={chatPanelUrl} className="w-full h-full border-none" />
        </div>
      </ResizablePanel>
    </ResizablePanelGroup>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
