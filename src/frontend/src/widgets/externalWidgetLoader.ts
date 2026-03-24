import React from "react";
import { logger } from "@/lib/logger";
import { getIvyHost } from "@/lib/utils";

/**
 * External widget registration info received from the backend.
 */
export interface ExternalWidgetInfo {
  typeName: string;
  scriptUrl: string;
  styleUrl?: string;
  exportName: string;
  globalName?: string;
}

/**
 * Cache for loaded external widget components.
 */
const loadedComponents = new Map<string, React.ComponentType<Record<string, unknown>>>();

/**
 * Cache for in-flight loading promises to prevent duplicate loads.
 */
const loadingPromises = new Map<string, Promise<React.ComponentType<Record<string, unknown>>>>();

/**
 * Set of stylesheets that have been loaded.
 */
const loadedStylesheets = new Set<string>();

/**
 * Registry of external widgets received from the backend.
 */
let externalWidgetRegistry: Map<string, ExternalWidgetInfo> = new Map();

/**
 * Updates the external widget registry with new data from the backend.
 */
export const setExternalWidgetRegistry = (
  widgets: ExternalWidgetInfo[] | null | undefined,
): void => {
  if (!widgets) return;

  externalWidgetRegistry = new Map(widgets.map((w) => [w.typeName, w]));
  // logger.info('External widget registry updated', {
  //   count: widgets.length,
  //   widgets: widgets.map(w => w.typeName),
  // });
};

/**
 * Gets the external widget registry.
 */
export const getExternalWidgetRegistry = (): Map<string, ExternalWidgetInfo> => {
  return externalWidgetRegistry;
};

/**
 * Checks if a widget type is an external widget.
 */
export const isExternalWidget = (typeName: string): boolean => {
  return externalWidgetRegistry.has(typeName);
};

/**
 * Gets info about an external widget.
 */
export const getExternalWidgetInfo = (typeName: string): ExternalWidgetInfo | undefined => {
  return externalWidgetRegistry.get(typeName);
};

/**
 * Cache for scripts that have been loaded or are currently loading.
 */
const scriptPromises = new Map<string, Promise<void>>();

/**
 * Loads a script via script tag and returns a promise that resolves when loaded.
 */
const loadScript = (url: string): Promise<void> => {
  const existing = scriptPromises.get(url);
  if (existing) {
    return existing;
  }

  const promise = new Promise<void>((resolve, reject) => {
    const script = document.createElement("script");
    script.src = url;
    script.async = true;

    script.onload = () => {
      //logger.debug('Loaded external widget script', { url });
      resolve();
    };

    script.onerror = () => {
      scriptPromises.delete(url); // allow retrying on failure
      reject(new Error(`Failed to load script: ${url}`));
    };

    document.head.appendChild(script);
  });

  scriptPromises.set(url, promise);
  return promise;
};

/**
 * Loads a stylesheet if it hasn't been loaded yet.
 */
const loadStylesheet = (url: string): void => {
  if (loadedStylesheets.has(url)) return;

  const link = document.createElement("link");
  link.rel = "stylesheet";
  link.href = url;
  document.head.appendChild(link);
  loadedStylesheets.add(url);

  //logger.debug('Loaded external widget stylesheet', { url });
};

/**
 * Loads an external widget component dynamically.
 * Returns a cached component if already loaded.
 */
export const loadExternalWidget = async (
  typeName: string,
): Promise<React.ComponentType<Record<string, unknown>>> => {
  // Return cached component if available
  const cached = loadedComponents.get(typeName);
  if (cached) {
    return cached;
  }

  // Return in-flight promise if already loading
  const inFlight = loadingPromises.get(typeName);
  if (inFlight) {
    return inFlight;
  }

  const widgetInfo = externalWidgetRegistry.get(typeName);
  if (!widgetInfo) {
    throw new Error(`External widget '${typeName}' not found in registry`);
  }

  // Get the backend host for constructing full URLs
  const backendHost = getIvyHost();

  // Load stylesheet if specified
  if (widgetInfo.styleUrl) {
    loadStylesheet(`${backendHost}${widgetInfo.styleUrl}`);
  }

  // Create loading promise
  const loadPromise = (async () => {
    try {
      const fullScriptUrl = `${backendHost}${widgetInfo.scriptUrl}`;
      // logger.debug('Loading external widget', {
      //   typeName,
      //   scriptUrl: fullScriptUrl,
      // });

      // Load IIFE script via script tag (dynamic import doesn't work with IIFE)
      await loadScript(fullScriptUrl);

      // Get the component from the global variable
      // The IIFE exports to window[globalName] where globalName is specified in the widget attribute
      // Falls back to the widget's class name (last part of typeName) for backwards compatibility
      const globalName = widgetInfo.globalName ?? typeName.split(".").pop() ?? typeName;
      const globalModule = (window as unknown as Record<string, Record<string, unknown>>)[
        globalName
      ];

      // logger.info('External widget lookup', {
      //   typeName,
      //   globalName,
      //   exportName: widgetInfo.exportName,
      //   globalModuleExists: !!globalModule,
      //   globalModuleKeys: globalModule ? Object.keys(globalModule) : [],
      // });

      if (!globalModule) {
        throw new Error(`Global '${globalName}' not found after loading external widget script`);
      }

      // Get the exported component
      const Component = (
        widgetInfo.exportName === "default"
          ? (globalModule.default ?? globalModule[globalName])
          : globalModule[widgetInfo.exportName]
      ) as React.ComponentType<Record<string, unknown>> | undefined;

      if (!Component) {
        throw new Error(
          `Export '${widgetInfo.exportName}' not found in external widget module '${typeName}'. Available exports: ${Object.keys(globalModule).join(", ")}`,
        );
      }

      // Cache the loaded component
      loadedComponents.set(typeName, Component);

      // logger.info('External widget loaded successfully', { typeName });

      return Component as React.ComponentType<Record<string, unknown>>;
    } catch (error) {
      logger.error("Failed to load external widget", { typeName, error });
      throw error;
    } finally {
      // Clean up loading promise
      loadingPromises.delete(typeName);
    }
  })();

  loadingPromises.set(typeName, loadPromise);
  return loadPromise;
};

/**
 * Gets a cached external widget component, or null if not loaded.
 */
export const getCachedExternalWidget = (
  typeName: string,
): React.ComponentType<Record<string, unknown>> | null => {
  return loadedComponents.get(typeName) ?? null;
};

/**
 * Cache for React.lazy wrappers per type name.
 */
const lazyComponents = new Map<
  string,
  React.LazyExoticComponent<React.ComponentType<Record<string, unknown>>>
>();

/**
 * Creates a React.lazy component for an external widget.
 * This allows the widget to be rendered with Suspense.
 * Caches the lazy wrapper to maintain component identity across renders.
 */
export const createLazyExternalWidget = (
  typeName: string,
): React.LazyExoticComponent<React.ComponentType<Record<string, unknown>>> => {
  let lazy = lazyComponents.get(typeName);
  if (!lazy) {
    lazy = React.lazy(() =>
      loadExternalWidget(typeName).then((Component) => ({
        default: Component,
      })),
    );
    lazyComponents.set(typeName, lazy);
  }
  return lazy;
};
