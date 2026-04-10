import { lazy, ComponentType } from "react";

function isChunkLoadError(error: unknown): boolean {
  if (error instanceof Error) {
    return (
      error.message.includes("Failed to fetch dynamically imported module") ||
      error.message.includes("Loading chunk") ||
      error.message.includes("Loading CSS chunk") ||
      error.message.includes("Failed to load script")
    );
  }
  return false;
}

export function lazyWithRetry<T extends ComponentType<any>>(
  factory: () => Promise<{ default: T }>,
): React.LazyExoticComponent<T> {
  return lazy(() =>
    factory().catch((error) => {
      if (isChunkLoadError(error)) {
        const reloadedKey = "vite-chunk-reload";
        if (!sessionStorage.getItem(reloadedKey)) {
          sessionStorage.setItem(reloadedKey, "1");
          window.location.reload();
          // Return dummy to satisfy types (page is reloading)
          return new Promise<{ default: T }>(() => {});
        }
        // Already reloaded once — clear flag and throw to avoid loop
        sessionStorage.removeItem(reloadedKey);
      }
      throw error;
    }),
  );
}
