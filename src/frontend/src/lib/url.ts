/**
 * Resolve a relative path against the ivy-host meta tag.
 * Used for upload URLs when running behind a reverse proxy.
 */
export function getFullUrl(path: string): string {
  const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
  if (ivyHostMeta) {
    const host = ivyHostMeta.getAttribute("content");
    return host + path;
  }
  return path;
}

/**
 * Extracts the content after the app:// protocol prefix using regex.
 * @param url - URL starting with app://
 * @returns Content after app://, or empty string if not an app:// URL
 */
function extractAppProtocolContent(url: string): string {
  const match = url.match(/^app:\/\/(.+)$/);
  return match ? match[1] : "";
}

/**
 * Extracts the anchor ID (content after the # symbol) using regex.
 * @param url - URL starting with #
 * @returns Anchor ID without the #, or empty string if not an anchor link
 */
export function extractAnchorId(url: string): string {
  const match = url.match(/^#(.+)$/);
  return match ? match[1] : "";
}

/**
 * Gets the current origin for same-origin validation.
 * Exported for testing purposes - can be mocked in tests.
 */
export function getCurrentOrigin(): string {
  if (typeof window === "undefined" || !window.location) {
    return "";
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
    const originWithProtocol = origin.includes("://") ? origin : `https://${origin}`;

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
  return url.startsWith("http://") || url.startsWith("https://");
}

export function isAnchorLink(url: string): boolean {
  return url.startsWith("#");
}

export function isAppProtocol(url: string): boolean {
  return url.startsWith("app://");
}

export function isMailtoUrl(url: string): boolean {
  return /^mailto:/i.test(url);
}

export function isRelativePath(url: string): boolean {
  return url.startsWith("/");
}

export function isDataUrl(url: string): boolean {
  return url.startsWith("data:");
}

export function isBlobUrl(url: string): boolean {
  return url.startsWith("blob:");
}

/**
 * Determines if a URL is a standard URL type that browsers handle natively
 * (external http/https, anchor links, app://, or relative paths)
 */
export function isStandardUrl(url: string): boolean {
  return isExternalUrl(url) || isAnchorLink(url) || isAppProtocol(url) || isRelativePath(url);
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
  return path.startsWith("/") ? path : `/${path}`;
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
  allowExternal: boolean = false,
): string | null {
  if (!url || typeof url !== "string") {
    return null;
  }

  // Trim whitespace
  url = url.trim();

  // Allow anchor links (starting with #)
  if (url.startsWith("#")) {
    if (!/^#[^?&]*$/.test(url)) {
      return null;
    }
    const afterHash = extractAnchorId(url);
    if (afterHash.includes("://")) {
      return null;
    }
    return url;
  }

  // Allow relative paths (starting with /)
  if (url.startsWith("/")) {
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
    if (urlObj.protocol !== "http:" && urlObj.protocol !== "https:") {
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

const DANGEROUS_PROTOCOLS = new Set(["javascript:", "data:", "vbscript:", "blob:"]);

/**
 * Validates and sanitizes a URL for use in anchor tags or window.open.
 * Allows relative paths, external http/https URLs, and app:// URLs, but prevents dangerous protocols.
 *
 * @param url - The URL to validate
 * @param options.allowCustomProtocols - When true, allows any URL scheme that isn't dangerous
 *   (javascript:, data:, vbscript:, blob:). Useful when an onLinkClick handler intercepts all
 *   navigation, so custom schemes like plan:// are passed to application code rather than the browser.
 * @returns The sanitized URL if valid, '#' otherwise
 */
export function validateLinkUrl(
  url: string | null | undefined,
  options?: { allowCustomProtocols?: boolean },
): string {
  if (!url || typeof url !== "string") {
    return "#";
  }

  // Trim whitespace
  url = url.trim();

  // Handle empty string after trimming
  if (url === "") {
    return "#";
  }

  // Allow mailto: URLs for email links
  if (/^mailto:/i.test(url)) {
    // Basic validation: must have at least one character after mailto:
    // and should not contain dangerous characters or protocol injection
    const afterProtocol = url.substring(7); // After "mailto:"
    if (!afterProtocol || afterProtocol.trim() === "") {
      return "#";
    }
    // Check for protocol injection
    if (afterProtocol.includes("://")) {
      return "#";
    }
    // Validate mailto format: mailto:email@domain.com[?subject=...&body=...]
    // Allow query parameters for subject, body, cc, bcc but disallow fragments
    if (!/^mailto:[^#]+$/i.test(url)) {
      return "#";
    }
    return url;
  }

  // Allow app:// URLs (Ivy internal navigation), including optional #fragment for article deep links
  if (url.startsWith("app://")) {
    const rest = url.slice(6);
    if (rest.includes("://")) {
      return "#";
    }
    const hashIdx = rest.indexOf("#");
    const beforeHash = hashIdx >= 0 ? rest.slice(0, hashIdx) : rest;
    const fragment = hashIdx >= 0 ? rest.slice(hashIdx) : "";
    // Path + query (before #): same rules as before — no : except in ?query values handled below
    if (!/^[^:#]*(\?[^#]*)?$/.test(beforeHash)) {
      return "#";
    }
    if (fragment && !/^#[^?&]*$/.test(fragment)) {
      return "#";
    }
    // Additional check: prevent protocol injection attempts
    const afterProtocol = extractAppProtocolContent(url);
    if (afterProtocol.match(/:[^?&#/]/)) {
      return "#";
    }
    return url;
  }

  // Allow anchor links (starting with #)
  // Use inline regex pattern matching
  if (url.startsWith("#")) {
    // Validate anchor links are safe
    // Allow colons in anchor IDs (HTML5 allows this), but prevent query params and fragments
    // Pattern: #[anchor-id] where anchor-id can contain colons but not ? or &
    if (!/^#[^?&]*$/.test(url)) {
      return "#";
    }
    // Additional check: prevent protocol injection attempts
    const afterHash = extractAnchorId(url);
    if (afterHash.includes("://")) {
      return "#";
    }
    return url;
  }

  // Allow relative paths (starting with /)
  if (url.startsWith("/")) {
    // Validate it's a safe relative path
    if (!/^\/[^:]*$/.test(url)) {
      return "#";
    }
    return url;
  }

  // For absolute URLs, validate protocol
  try {
    const urlObj = new URL(url);

    if (options?.allowCustomProtocols) {
      if (DANGEROUS_PROTOCOLS.has(urlObj.protocol)) {
        return "#";
      }
    } else {
      if (urlObj.protocol !== "http:" && urlObj.protocol !== "https:") {
        return "#";
      }
    }

    return urlObj.toString();
  } catch {
    // Invalid URL format - treat as relative if it doesn't contain colons
    if (!url.includes(":")) {
      // Might be a relative path without leading slash
      return url.startsWith("/") ? url : `/${url}`;
    }
    return "#";
  }
}

/**
 * Checks if a string is a Windows absolute path
 */
export function isWindowsAbsolutePath(path: string): boolean {
  return /^[a-zA-Z]:[\\/]/.test(path);
}

/**
 * Converts a Windows absolute path to a file:// URL
 */
export function windowsPathToFileUrl(path: string): string {
  // Normalize backslashes to forward slashes
  const normalized = path.replace(/\\/g, "/");
  // Ensure proper file:// URL format
  return `file:///${normalized}`;
}

/**
 * Options for URL validation
 */
export interface ValidateMediaUrlOptions {
  /**
   * Media type for data URL validation (e.g., 'image', 'audio', 'video')
   * If not specified, data URLs are rejected
   */
  mediaType?: "image" | "audio" | "video";
  /**
   * Allowed protocols (default: ['http:', 'https:', 'data:', 'blob:', 'app:'])
   */
  allowedProtocols?: string[];
  /**
   * Whether to allow external URLs (default: true for media URLs)
   */
  allowExternal?: boolean;
  /**
   * DANGEROUS: Allow file:// protocol URLs for local file access.
   * Only enable this in trusted contexts (e.g., local development/testing).
   * Default: false
   */
  dangerouslyAllowLocalFiles?: boolean;
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
  options: ValidateMediaUrlOptions = {},
): string | null {
  if (!url || typeof url !== "string") {
    return null;
  }

  // Trim whitespace
  url = url.trim();

  // Handle empty string after trimming
  if (url === "") {
    return null;
  }

  const {
    mediaType,
    allowedProtocols = ["http:", "https:", "data:", "blob:", "app:"],
    allowExternal = true,
    dangerouslyAllowLocalFiles = false,
  } = options;

  // Allow data: URLs (for base64 encoded media)
  if (url.startsWith("data:")) {
    // If mediaType is specified, validate that it matches
    if (mediaType) {
      const dataUrlPattern = new RegExp(`^data:${mediaType}/`, "i");
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
  if (url.startsWith("blob:")) {
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
      const protocolIndex = blobUrlWithoutPrefix.indexOf("://");
      if (protocolIndex === -1) {
        // Invalid blob URL format (no protocol)
        return null;
      }

      // Find the first "/" after the protocol and hostname
      // The origin ends at the first "/" that comes after "://"
      const afterProtocol = blobUrlWithoutPrefix.substring(protocolIndex + 3);
      const firstSlashIndex = afterProtocol.indexOf("/");

      if (firstSlashIndex === -1) {
        // Invalid blob URL format (no slash after origin)
        return null;
      }

      // Extract origin: protocol + "://" + hostname (and optional port)
      const blobOrigin = blobUrlWithoutPrefix.substring(0, protocolIndex + 3 + firstSlashIndex);

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

  // Allow file:// URLs only if explicitly enabled (security-sensitive)
  if (url.startsWith("file://") || isWindowsAbsolutePath(url)) {
    // Only allow if dangerouslyAllowLocalFiles is explicitly set to true
    if (!dangerouslyAllowLocalFiles) {
      return null;
    }

    // Convert Windows paths to file:// URLs
    let fileUrl = url;
    if (isWindowsAbsolutePath(url)) {
      fileUrl = windowsPathToFileUrl(url);
    }

    // Basic validation: ensure it's a proper file URL
    try {
      const urlObj = new URL(fileUrl);
      if (urlObj.protocol !== "file:") {
        return null;
      }
      return fileUrl;
    } catch {
      return null;
    }
  }

  // Allow app:// URLs (Ivy internal navigation)
  if (url.startsWith("app://")) {
    // Validate app:// URLs don't contain dangerous characters
    // Pattern: app://[app-id][?query-params] where app-id has no colons/hashes, query-params have no #
    if (!/^app:\/\/[^:#]*(\?[^#]*)?$/.test(url)) {
      return null;
    }
    // Additional check: prevent protocol injection attempts
    // Catches '://' in query params and colons that could be used for protocol injection
    const afterProtocol = extractAppProtocolContent(url);
    if (afterProtocol.includes("://") || afterProtocol.match(/:[^?&/]/)) {
      return null;
    }
    return url;
  }

  // Allow relative paths (starting with /)
  if (url.startsWith("/")) {
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
    if (!allowExternal && (urlObj.protocol === "http:" || urlObj.protocol === "https:")) {
      const currentOrigin = _getCurrentOriginRef.getCurrentOrigin();
      if (!currentOrigin || urlObj.origin !== currentOrigin) {
        return null;
      }
    }

    return urlObj.toString();
  } catch {
    // Invalid URL format - treat as relative if it doesn't contain colons
    if (!url.includes(":")) {
      // Might be a relative path without leading slash
      return url.startsWith("/") ? url : `/${url}`;
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
export function validateImageUrl(url: string | null | undefined): string | null {
  return validateMediaUrl(url, { mediaType: "image" });
}

/**
 * Validates and sanitizes an audio URL to prevent open redirect vulnerabilities.
 * Allows http/https URLs, data:audio URLs (for base64 audio), blob: URLs (for client-side audio)
 *
 * @param url - The audio URL to validate
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateAudioUrl(url: string | null | undefined): string | null {
  return validateMediaUrl(url, { mediaType: "audio" });
}

/**
 * Validates and sanitizes a video URL to prevent open redirect vulnerabilities.
 * Allows http/https URLs, data:video URLs (for base64 video), blob: URLs (for client-side video),
 * and safe relative paths. Prevents dangerous protocols and protocol injection.
 *
 * @param url - The video URL to validate
 * @returns The sanitized URL if valid, null otherwise
 */
export function validateVideoUrl(url: string | null | undefined): string | null {
  return validateMediaUrl(url, { mediaType: "video" });
}

/**
 * Mapping of allowed embed hostnames to their platform names.
 * This prevents attacks like:
 * - https://evil.com/youtube.com (substring matching)
 * - https://youtube.com.evil.com (subdomain attacks)
 */
const EMBED_HOST_TO_PLATFORM: Record<string, string> = {
  "youtube.com": "youtube",
  "youtu.be": "youtube",
  "twitter.com": "twitter",
  "x.com": "twitter",
  "facebook.com": "facebook",
  "instagram.com": "instagram",
  "linkedin.com": "linkedin",
  "pinterest.com": "pinterest",
  "pin.it": "pinterest",
  "github.com": "github",
  "gist.github.com": "github",
  "reddit.com": "reddit",
  "tiktok.com": "tiktok",
};

/**
 * Validates an embed URL and returns the platform name if valid.
 * This function properly validates hostnames to prevent attacks like:
 * - https://evil.com/youtube.com (substring matching)
 * - https://youtube.com.evil.com (subdomain attacks)
 *
 * @param url - The embed URL to validate
 * @returns The platform name if valid, null otherwise
 */
export function validateEmbedUrl(url: string | null | undefined): string | null {
  if (!url || typeof url !== "string") {
    return null;
  }

  // Trim whitespace
  url = url.trim();

  // Handle empty string after trimming
  if (url === "") {
    return null;
  }

  try {
    const urlObj = new URL(url);

    // Only allow http and https protocols
    if (urlObj.protocol !== "http:" && urlObj.protocol !== "https:") {
      return null;
    }

    // Get hostname in lowercase for comparison
    const hostname = urlObj.hostname.toLowerCase();

    // Check if hostname matches any allowed host (exact match or subdomain)
    for (const [allowedHost, platform] of Object.entries(EMBED_HOST_TO_PLATFORM)) {
      if (hostname === allowedHost || hostname.endsWith("." + allowedHost)) {
        return platform;
      }
    }

    return null;
  } catch {
    // Invalid URL format
    return null;
  }
}
