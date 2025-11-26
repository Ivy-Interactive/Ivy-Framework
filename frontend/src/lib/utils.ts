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
/**
 * Extracts the content after the app:// protocol prefix using regex.
 */
function extractAppProtocolContent(url: string): string {
  const match = url.match(/^app:\/\/(.+)$/);
  return match ? match[1] : '';
}

export function convertAppUrlToPath(appUrl: string): string {
  // Use inline regex pattern matching
  if (!/^app:\/\//.test(appUrl)) {
    return appUrl;
  }

  // Extract app ID and any existing query string using regex
  const appId = extractAppProtocolContent(appUrl);
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

// Allowlist for trusted hosts; update as needed for trusted deployments
const ALLOWED_IVY_HOSTS = [
  window.location.origin,
  // 'https://your-cdn.com', // add extra trusted hostnames here if relevant
];

function isAllowedIvyHost(origin: string): boolean {
  try {
    const url = new URL(origin);
    const normalizedOrigin = url.origin.replace(/\/+$/, '').toLowerCase();
    const currentUrl = new URL(window.location.origin);

    // Only allow http and https protocols
    if (url.protocol !== 'http:' && url.protocol !== 'https:') {
      return false;
    }

    // Allow if it matches the current origin exactly (protocol, hostname, and port)
    if (url.origin === currentUrl.origin) {
      return true;
    }

    // For development: allow same hostname with different port, but require same protocol
    // This enables development workflows where frontend and backend run on different ports
    // SECURITY: We require protocol matching to prevent protocol downgrade attacks
    // (e.g., preventing http://localhost:3000 from being accepted when current origin is https://localhost:5000)
    // Only allow this for localhost/127.0.0.1 to prevent security issues in production
    const localhostVariants = ['localhost', '127.0.0.1', '[::1]', '::1'];
    const isCurrentLocalhost = localhostVariants.includes(
      currentUrl.hostname.toLowerCase()
    );
    const isUrlLocalhost = localhostVariants.includes(
      url.hostname.toLowerCase()
    );

    // Allow if both are localhost variants AND protocols match
    // This allows different ports during development but prevents protocol downgrade attacks
    if (isCurrentLocalhost && isUrlLocalhost) {
      // Require protocol matching to prevent security vulnerabilities
      // An attacker controlling a different localhost port should not be able to
      // downgrade from HTTPS to HTTP or vice versa
      if (url.protocol === currentUrl.protocol) {
        return true;
      }
      // Reject if protocols don't match (security: prevent protocol downgrade)
      return false;
    }

    // Check against the allowlist
    // Normalize each allowed origin and compare (avoid creating URL objects in loop)
    return ALLOWED_IVY_HOSTS.some(allowed => {
      try {
        const allowedUrl = new URL(allowed);
        const normalizedAllowed = allowedUrl.origin
          .replace(/\/+$/, '')
          .toLowerCase();
        return normalizedAllowed === normalizedOrigin;
      } catch {
        // Skip invalid URLs in allowlist
        return false;
      }
    });
  } catch {
    return false;
  }
}

export function getIvyHost(): string {
  // Never trust user-supplied ivyHost from URL parameters.
  // Only use meta tag or real origin.
  // Query parameters are user-controllable and should never be trusted for security-sensitive operations.
  const metaHost = document
    .querySelector('meta[name="ivy-host"]')
    ?.getAttribute('content');

  if (metaHost) {
    try {
      // Parse the metaHost - it might be a full URL or just a hostname
      let url: URL;
      if (metaHost.includes('://')) {
        // It's a full URL
        url = new URL(metaHost);
      } else {
        // It's just a hostname, construct a URL with https protocol
        url = new URL(`https://${metaHost}`);
      }

      // Must be http(s) and must be in the allowlist
      if (url.protocol === 'https:' || url.protocol === 'http:') {
        const metaOrigin = url.origin;
        if (isAllowedIvyHost(metaOrigin)) {
          return metaOrigin;
        }
      }
    } catch {
      // Ignore parse errors and fall back
    }
  }

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

// Re-export URL validation functions from dedicated module
export {
  getCurrentOrigin,
  _getCurrentOriginRef,
  validateRedirectUrl,
  validateLinkUrl,
  validateMediaUrl,
  validateImageUrl,
  validateAudioUrl,
  validateVideoUrl,
  type ValidateMediaUrlOptions,
  // URL type detection helpers
  isExternalUrl,
  isAnchorLink,
  isAppProtocol,
  isRelativePath,
  isDataUrl,
  isBlobUrl,
  isStandardUrl,
  isFullUrl,
  normalizeRelativePath,
} from './urlValidation';
