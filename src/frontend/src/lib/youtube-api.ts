/**
 * Lazy singleton loader for the YouTube IFrame API.
 * Script is injected on first use and never unloaded.
 * Types provided by @types/youtube (YT namespace).
 */

declare global {
  interface Window {
    onYouTubeIframeAPIReady?: () => void;
  }
}

let loadPromise: Promise<void> | null = null;

/**
 * Loads the YouTube IFrame API once. Subsequent calls return the same promise.
 * Resolves when the API is ready (onYouTubeIframeAPIReady has fired).
 */
export function loadYouTubeAPI(): Promise<void> {
  if (loadPromise) {
    return loadPromise;
  }

  if (typeof window === 'undefined') {
    return Promise.reject(new Error('YouTube API requires window'));
  }

  if (window.YT?.Player) {
    loadPromise = Promise.resolve();
    return loadPromise;
  }

  loadPromise = new Promise<void>((resolve) => {
    const previousCallback = window.onYouTubeIframeAPIReady;
    window.onYouTubeIframeAPIReady = () => {
      previousCallback?.();
      resolve();
    };

    const script = document.createElement('script');
    script.src = 'https://www.youtube.com/iframe_api';
    script.async = true;
    const firstScript = document.getElementsByTagName('script')[0];
    firstScript?.parentNode?.insertBefore(script, firstScript);
  });

  return loadPromise;
}
