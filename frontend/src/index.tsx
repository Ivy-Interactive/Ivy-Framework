import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import { App } from './components/App';

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
