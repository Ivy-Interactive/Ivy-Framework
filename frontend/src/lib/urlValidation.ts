/**
 * Extracts the content after the app:// protocol prefix using regex.
 * @param url - URL starting with app://
 * @returns Content after app://, or empty string if not an app:// URL
 */
function extractAppProtocolContent(url: string): string {
  const match = url.match(/^app:\/\/(.+)$/);
  return match ? match[1] : '';
}

/**
 * Extracts the anchor ID (content after the # symbol) using regex.
 * @param url - URL starting with #
 * @returns Anchor ID without the #, or empty string if not an anchor link
 */
export function extractAnchorId(url: string): string {
  const match = url.match(/^#(.+)$/);
  return match ? match[1] : '';
}

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
 * Normalizes an origin string by removing default ports.
 * This ensures that https://example.com and https://example.com:443 are treated as equal.
 * The URL.origin property already handles default port normalization, so we use it directly.
 * @param origin - The origin string to normalize (e.g., "https://example.com" or "https://example.com:443")
 * @returns The normalized origin string
 */
function normalizeOrigin(origin: string): string {
  if (!origin) return origin;

  try {
    // Ensure the origin has a protocol for parsing
    const originWithProtocol = origin.includes('://')
      ? origin
      : `https://${origin}`;

    const url = new URL(originWithProtocol);
    // url.origin already excludes default ports (443 for https, 80 for http)
    return url.origin;
  } catch {
    // If parsing fails, return as-is
    return origin;
  }
}

/**
 * URL type detection helpers
 */
export function isExternalUrl(url: string): boolean {
  return url.startsWith('http://') || url.startsWith('https://');
}

export function isAnchorLink(url: string): boolean {
  return /^#/.test(url);
}

export function isAppProtocol(url: string): boolean {
  return /^app:\/\//.test(url);
}

export function isMailtoUrl(url: string): boolean {
  return /^mailto:/i.test(url);
}

export function isRelativePath(url: string): boolean {
  return url.startsWith('/');
}

export function isDataUrl(url: string): boolean {
  return url.startsWith('data:');
}

export function isBlobUrl(url: string): boolean {
  return url.startsWith('blob:');
}

/**
 * Determines if a URL is a standard URL type that browsers handle natively
 * (external http/https, anchor links, app://, or relative paths)
 */
export function isStandardUrl(url: string): boolean {
  return (
    isExternalUrl(url) ||
    isAnchorLink(url) ||
    isAppProtocol(url) ||
    isRelativePath(url)
  );
}

/**
 * Checks if a URL is a full URL (http/https, data:, blob:, or app:)
 * as opposed to a relative path
 */
export function isFullUrl(url: string): boolean {
  return /^(https?:\/\/|data:|blob:|app:)/i.test(url);
}

/**
 * Normalizes a relative path by ensuring it starts with a leading slash
 */
export function normalizeRelativePath(path: string): string {
  return path.startsWith('/') ? path : `/${path}`;
}

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

  // Allow mailto: URLs for email links
  if (/^mailto:/i.test(url)) {
    // Basic validation: must have at least one character after mailto:
    // and should not contain dangerous characters or protocol injection
    const afterProtocol = url.substring(7); // After "mailto:"
    if (!afterProtocol || afterProtocol.trim() === '') {
      return '#';
    }
    // Check for protocol injection
    if (afterProtocol.includes('://')) {
      return '#';
    }
    // Validate mailto format: mailto:email@domain.com[?subject=...&body=...]
    // Allow query parameters for subject, body, cc, bcc but disallow fragments
    if (!/^mailto:[^#]+$/i.test(url)) {
      return '#';
    }
    return url;
  }

  // Allow app:// URLs (Ivy internal navigation)
  if (/^app:\/\//.test(url)) {
    // Validate app:// URLs don't contain dangerous characters
    // Pattern: app://[app-id][?query-params] where app-id has no colons/hashes, query-params have no #
    if (!/^app:\/\/[^:#]*(\?[^#]*)?$/.test(url)) {
      return '#';
    }
    // Additional check: prevent protocol injection attempts
    // Catches '://' in query params and colons that could be used for protocol injection
    const afterProtocol = extractAppProtocolContent(url);
    if (afterProtocol.includes('://') || afterProtocol.match(/:[^?&/]/)) {
      return '#';
    }
    return url;
  }

  // Allow anchor links (starting with #)
  // Use inline regex pattern matching
  if (/^#/.test(url)) {
    // Validate anchor links are safe
    // Allow colons in anchor IDs (HTML5 allows this), but prevent query params and fragments
    // Pattern: #[anchor-id] where anchor-id can contain colons but not ? or &
    if (!/^#[^?&]*$/.test(url)) {
      return '#';
    }
    // Additional check: prevent protocol injection attempts
    const afterHash = extractAnchorId(url);
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

/**
 * Options for URL validation
 */
export interface ValidateMediaUrlOptions {
  /**
   * Media type for data URL validation (e.g., 'image', 'audio', 'video')
   * If not specified, data URLs are rejected
   */
  mediaType?: 'image' | 'audio' | 'video';
  /**
   * Allowed protocols (default: ['http:', 'https:', 'data:', 'blob:', 'app:'])
   */
  allowedProtocols?: string[];
  /**
   * Whether to allow external URLs (default: true for media URLs)
   */
  allowExternal?: boolean;
}

/**
 * Unified function to validate and sanitize media URLs (images, audio, video) to prevent open redirect vulnerabilities.
 * This consolidates the common validation logic used across validateImageUrl, validateAudioUrl, and validateVideoUrl.
 *
 * @param url - The URL to validate
 * @param options - Validation options
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateMediaUrl(
  url: string | null | undefined,
  options: ValidateMediaUrlOptions = {}
): string | null {
  if (!url || typeof url !== 'string') {
    return null;
  }

  // Trim whitespace
  url = url.trim();

  // Handle empty string after trimming
  if (url === '') {
    return null;
  }

  const {
    mediaType,
    allowedProtocols = ['http:', 'https:', 'data:', 'blob:', 'app:'],
    allowExternal = true,
  } = options;

  // Allow data: URLs (for base64 encoded media)
  if (url.startsWith('data:')) {
    // If mediaType is specified, validate that it matches
    if (mediaType) {
      const dataUrlPattern = new RegExp(`^data:${mediaType}/`, 'i');
      if (!dataUrlPattern.test(url)) {
        return null;
      }
    } else {
      // If no mediaType specified, reject all data URLs
      return null;
    }
    return url;
  }

  // Allow blob: URLs (for client-side generated media)
  if (url.startsWith('blob:')) {
    // Additional validation: ensure blob URL's origin matches current origin
    // This prevents attacks like blob:https://attacker.com/uuid
    // Blob URLs have format: blob:<origin>/<uuid>
    // Note: new URL() returns origin as "null" for blob URLs (opaque origin),
    // so we must extract the origin from the blob URL string itself
    try {
      const currentOrigin = _getCurrentOriginRef.getCurrentOrigin();
      if (!currentOrigin) {
        // Cannot validate without current origin (e.g., SSR)
        return null;
      }

      // Extract origin from blob URL: blob:<origin>/<uuid>
      // Format is blob:<protocol>://<host>/<uuid>
      // We need to extract the origin part (protocol + host + optional port)
      const blobUrlWithoutPrefix = url.substring(5); // Remove "blob:"

      // Find the protocol separator "://"
      const protocolIndex = blobUrlWithoutPrefix.indexOf('://');
      if (protocolIndex === -1) {
        // Invalid blob URL format (no protocol)
        return null;
      }

      // Find the first "/" after the protocol and hostname
      // The origin ends at the first "/" that comes after "://"
      const afterProtocol = blobUrlWithoutPrefix.substring(protocolIndex + 3);
      const firstSlashIndex = afterProtocol.indexOf('/');

      if (firstSlashIndex === -1) {
        // Invalid blob URL format (no slash after origin)
        return null;
      }

      // Extract origin: protocol + "://" + hostname (and optional port)
      const blobOrigin = blobUrlWithoutPrefix.substring(
        0,
        protocolIndex + 3 + firstSlashIndex
      );

      // Normalize origins for comparison (handle default ports)
      const normalizedBlobOrigin = normalizeOrigin(blobOrigin);
      const normalizedCurrentOrigin = normalizeOrigin(currentOrigin);

      if (normalizedBlobOrigin !== normalizedCurrentOrigin) {
        return null;
      }
    } catch {
      return null;
    }
    return url;
  }

  // Allow app:// URLs (Ivy internal navigation)
  if (/^app:\/\//.test(url)) {
    // Validate app:// URLs don't contain dangerous characters
    // Pattern: app://[app-id][?query-params] where app-id has no colons/hashes, query-params have no #
    if (!/^app:\/\/[^:#]*(\?[^#]*)?$/.test(url)) {
      return null;
    }
    // Additional check: prevent protocol injection attempts
    // Catches '://' in query params and colons that could be used for protocol injection
    const afterProtocol = extractAppProtocolContent(url);
    if (afterProtocol.includes('://') || afterProtocol.match(/:[^?&/]/)) {
      return null;
    }
    return url;
  }

  // Allow relative paths (starting with /)
  if (url.startsWith('/')) {
    // Validate it's a safe relative path (no protocol, no javascript:, etc.)
    if (!/^\/[^:]*$/.test(url)) {
      return null;
    }
    return url;
  }

  // For absolute URLs, validate protocol
  try {
    const urlObj = new URL(url);

    // Only allow specified protocols (prevent javascript:, etc.)
    if (!allowedProtocols.includes(urlObj.protocol)) {
      return null;
    }

    // If external URLs are not allowed, only allow same-origin
    if (
      !allowExternal &&
      (urlObj.protocol === 'http:' || urlObj.protocol === 'https:')
    ) {
      const currentOrigin = _getCurrentOriginRef.getCurrentOrigin();
      if (!currentOrigin || urlObj.origin !== currentOrigin) {
        return null;
      }
    }

    return urlObj.toString();
  } catch {
    // Invalid URL format - treat as relative if it doesn't contain colons
    if (!url.includes(':')) {
      // Might be a relative path without leading slash
      return url.startsWith('/') ? url : `/${url}`;
    }
    return null;
  }
}

/**
 * Validates and sanitizes an image URL to prevent open redirect vulnerabilities.
 *
 * @param url - The image URL to validate
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateImageUrl(
  url: string | null | undefined
): string | null {
  return validateMediaUrl(url, { mediaType: 'image' });
}

/**
 * Validates and sanitizes an audio URL to prevent open redirect vulnerabilities.
 * Allows http/https URLs, data:audio URLs (for base64 audio), blob: URLs (for client-side audio)
 *
 * @param url - The audio URL to validate
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateAudioUrl(
  url: string | null | undefined
): string | null {
  return validateMediaUrl(url, { mediaType: 'audio' });
}

/**
 * Validates and sanitizes a video URL to prevent open redirect vulnerabilities.
 * Allows http/https URLs, data:video URLs (for base64 video), blob: URLs (for client-side video),
 * and safe relative paths. Prevents dangerous protocols and protocol injection.
 *
 * @param url - The video URL to validate
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateVideoUrl(
  url: string | null | undefined
): string | null {
  return validateMediaUrl(url, { mediaType: 'video' });
}
