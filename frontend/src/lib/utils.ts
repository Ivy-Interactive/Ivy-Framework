import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
import { textBlockClassMap } from './textBlockClassMap';
import routingConstants from '../routing-constants.json' assert { type: 'json' };

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function getAppId(): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  const appIdFromParams = urlParams.get('appId');
  if (appIdFromParams) {
    return appIdFromParams;
  }

  // If no appId parameter, try to parse from path
  const path = window.location.pathname.toLowerCase();
  const originalPath = window.location.pathname;

  // Skip if path is empty or just "/"
  if (!path || path === '/') {
    return null;
  }

  // Skip if path starts with any excluded pattern (must be exact segment match)
  if (
    routingConstants.excludedPaths.some(
      excluded => path === excluded || path.startsWith(excluded + '/')
    )
  ) {
    return null;
  }

  // Skip if path has a static file extension
  if (routingConstants.staticFileExtensions.some(ext => path.endsWith(ext))) {
    return null;
  }

  // Convert path to appId
  // Remove leading slash and use the rest as appId
  const appId = originalPath.replace(/^\/+/, '');

  // Only convert if the path looks like an app ID (contains at least one segment and no dots)
  if (appId && !appId.includes('.')) {
    return appId;
  }

  return null;
}

export function getAppArgs(): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('appArgs');
}

export function getParentId(): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('parentId');
}

export function getChromeParam(): boolean {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('chrome')?.toLowerCase() !== 'false';
}

/**
 * Converts an app:// URL to a regular browser path.
 * Preserves query parameters from the current URL (especially chrome=false) when in chrome=false mode.
 *
 * @param appUrl - The app:// URL to convert (e.g., "app://MyApp" or "app://MyApp?param=value")
 * @returns The converted path (e.g., "/MyApp" or "/MyApp?param=value&chrome=false")
 */
export function convertAppUrlToPath(appUrl: string): string {
  if (!appUrl.startsWith('app://')) {
    return appUrl;
  }

  // Extract app ID and any existing query string
  const appId = appUrl.substring(6); // Remove "app://"
  const [appPath, existingQueryString] = appId.split('?');

  // Build the path
  let path = `/${appPath}`;

  // Preserve chrome=false if we're currently in chrome=false mode
  const isChromeFalse = !getChromeParam();
  const queryParams = new URLSearchParams(existingQueryString || '');

  if (isChromeFalse && !queryParams.has('chrome')) {
    queryParams.set('chrome', 'false');
  }

  // Combine existing query params with chrome param
  const finalQueryString = queryParams.toString();
  if (finalQueryString) {
    path += `?${finalQueryString}`;
  }

  return path;
}

function generateUUID(): string {
  if (typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return '10000000-1000-4000-8000-100000000000'.replace(/[018]/g, (c: string) =>
    (
      Number(c) ^
      (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (Number(c) / 4)))
    ).toString(16)
  );
}

export function getMachineId(): string {
  let id = localStorage.getItem('machineId');
  if (!id) {
    id = generateUUID();
    localStorage.setItem('machineId', id);
  }
  return id;
}

export function getIvyHost(): string {
  const urlParams = new URLSearchParams(window.location.search);
  const ivyHost = urlParams.get('ivyHost');
  if (ivyHost) return ivyHost;

  const metaHost = document
    .querySelector('meta[name="ivy-host"]')
    ?.getAttribute('content');
  if (metaHost) return metaHost;

  return window.location.origin;
}

export function camelCase(titleCase: unknown): unknown {
  if (typeof titleCase !== 'string') {
    return titleCase;
  }
  return titleCase.charAt(0).toLowerCase() + titleCase.slice(1);
}

// Shared Ivy tag-to-class map for headings, paragraphs, lists, tables, etc.
export const ivyTagClassMap = textBlockClassMap;

/**
 * Gets the current origin for same-origin validation.
 * Exported for testing purposes - can be mocked in tests.
 */
export function getCurrentOrigin(): string {
  if (typeof window === 'undefined' || !window.location) {
    return '';
  }
  return window.location.origin;
}

// Internal reference to getCurrentOrigin for use within this module
// Using an object wrapper so it can be modified in tests
export const _getCurrentOriginRef = {
  getCurrentOrigin: getCurrentOrigin,
};

/**
 * Validates and sanitizes a URL to prevent open redirect vulnerabilities.
 * Only allows relative paths (starting with /) or absolute URLs with http/https protocol.
 * For redirects, external URLs are only allowed if they match the current origin.
 *
 * @param url - The URL to validate
 * @param allowExternal - Whether to allow external URLs (default: false for redirects)
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateRedirectUrl(
  url: string | null | undefined,
  allowExternal: boolean = false
): string | null {
  if (!url || typeof url !== 'string') {
    return null;
  }

  // Trim whitespace
  url = url.trim();

  // Allow relative paths (starting with /)
  if (url.startsWith('/')) {
    // Validate it's a safe relative path (no protocol, no javascript:, etc.)
    if (!/^\/[^:]*$/.test(url)) {
      return null;
    }
    return url;
  }

  // For external URLs, validate protocol and optionally origin
  try {
    const urlObj = new URL(url);

    // Only allow http and https protocols
    if (urlObj.protocol !== 'http:' && urlObj.protocol !== 'https:') {
      return null;
    }

    // If external URLs are not allowed, only allow same-origin
    if (!allowExternal) {
      // Use the internal reference which points to the exported function
      // This allows mocking the exported function to work internally
      const currentOrigin = _getCurrentOriginRef.getCurrentOrigin();
      if (!currentOrigin || urlObj.origin !== currentOrigin) {
        return null;
      }
    }

    return urlObj.toString();
  } catch {
    // Invalid URL format
    return null;
  }
}

/**
 * Validates and sanitizes a URL for use in anchor tags or window.open.
 * Allows relative paths, external http/https URLs, and app:// URLs, but prevents dangerous protocols.
 *
 * @param url - The URL to validate
 * @returns The sanitized URL if valid, '#' otherwise
 */
export function validateLinkUrl(url: string | null | undefined): string {
  if (!url || typeof url !== 'string') {
    return '#';
  }

  // Trim whitespace
  url = url.trim();

  // Handle empty string after trimming
  if (url === '') {
    return '#';
  }

  // Allow app:// URLs (Ivy internal navigation)
  if (url.startsWith('app://')) {
    // Validate app:// URLs don't contain dangerous characters
    // Allow query parameters (? and &) but prevent fragments (#) and protocol injection (multiple colons)
    // Pattern: app://[app-id][?query-params] where query-params can contain & but not #
    if (!/^app:\/\/[^:#]*(\?[^#]*)?$/.test(url)) {
      return '#';
    }
    // Additional check: prevent protocol injection (multiple colons after app://)
    const afterProtocol = url.substring(6); // After "app://"
    if (afterProtocol.includes('://') || afterProtocol.match(/:[^?&/]/)) {
      return '#';
    }
    return url;
  }

  // Allow anchor links (starting with #)
  if (url.startsWith('#')) {
    // Validate anchor links are safe
    // Allow colons in anchor IDs (HTML5 allows this), but prevent query params and fragments
    // Pattern: #[anchor-id] where anchor-id can contain colons but not ? or &
    if (!/^#[^?&]*$/.test(url)) {
      return '#';
    }
    // Additional check: prevent protocol injection attempts
    const afterHash = url.substring(1);
    if (afterHash.includes('://')) {
      return '#';
    }
    return url;
  }

  // Allow relative paths (starting with /)
  if (url.startsWith('/')) {
    // Validate it's a safe relative path
    if (!/^\/[^:]*$/.test(url)) {
      return '#';
    }
    return url;
  }

  // For absolute URLs, validate protocol
  try {
    const urlObj = new URL(url);

    // Only allow http and https protocols (prevent javascript:, data:, etc.)
    if (urlObj.protocol !== 'http:' && urlObj.protocol !== 'https:') {
      return '#';
    }

    return urlObj.toString();
  } catch {
    // Invalid URL format - treat as relative if it doesn't contain colons
    if (!url.includes(':')) {
      // Might be a relative path without leading slash
      return url.startsWith('/') ? url : `/${url}`;
    }
    return '#';
  }
}
