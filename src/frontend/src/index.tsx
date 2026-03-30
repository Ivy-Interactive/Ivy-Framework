import React, { StrictMode } from "react";
import * as ReactDOMClient from "react-dom/client";
import * as ReactDOM from "react-dom";
import * as ReactJsxRuntime from "react/jsx-runtime";
import "./index.css";
import { App } from "./components/App";

// Expose React globally for external widgets (IIFE format)
// Merge react-dom and react-dom/client exports for full API coverage
(window as unknown as Record<string, unknown>).React = React;
(window as unknown as Record<string, unknown>).ReactDOM = {
  ...ReactDOM,
  ...ReactDOMClient,
};
(window as unknown as Record<string, unknown>).ReactJsxRuntime = ReactJsxRuntime;

// Polyfill require for Vite-Plus Rolldown CJS externals bug in IIFE widget bundles
(window as any).require = (moduleName: string) => {
  if (moduleName === "react") return React;
  if (moduleName === "react-dom") return { ...ReactDOM, ...ReactDOMClient };
  if (moduleName === "react/jsx-runtime") return ReactJsxRuntime;
  throw new Error(`Module '${moduleName}' not found in Ivy browser require polyfill`);
};

const container = document.getElementById("root");
if (!container) throw new Error("Failed to find root element");

interface WindowWithReactRoot extends Window {
  __reactRoot?: ReturnType<typeof ReactDOMClient.createRoot>;
}

let root = (window as WindowWithReactRoot).__reactRoot;
if (!root) {
  root = ReactDOMClient.createRoot(container);
  (window as WindowWithReactRoot).__reactRoot = root;
}

// Toggle via VITE_STRICT_MODE=false in .env.development or command line
const useStrictMode = import.meta.env.VITE_STRICT_MODE !== "false";

root.render(
  useStrictMode ? (
    <StrictMode>
      <App />
    </StrictMode>
  ) : (
    <App />
  ),
);
