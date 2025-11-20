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
    const afterProtocol = url.substring(7); // After "app://"
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
    // Additional validation: prevent protocol injection
    if (url.includes('://') && !url.startsWith('data:')) {
      return null;
    }
    return url;
  }

  // Allow blob: URLs (for client-side generated media)
  if (url.startsWith('blob:')) {
    // Additional validation: ensure blob URL's origin matches current origin
    // This prevents attacks like blob:https://attacker.com/uuid
    try {
      const blobUrl = new URL(url);
      const currentOrigin = _getCurrentOriginRef.getCurrentOrigin();
      if (blobUrl.origin !== currentOrigin) {
        return null;
      }
    } catch {
      return null;
    }
    return url;
  }

  // Allow app:// URLs (Ivy internal navigation)
  if (url.startsWith('app://')) {
    // Validate app:// URLs don't contain dangerous characters
    if (!/^app:\/\/[^:#]*(\?[^#]*)?$/.test(url)) {
      return null;
    }
    // Additional check: prevent protocol injection
    const afterProtocol = url.substring(7);
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
