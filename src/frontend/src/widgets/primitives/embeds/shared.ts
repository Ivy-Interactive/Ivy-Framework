// Shared utilities for embed components
export const sanitizeUrl = (url: string): string | null => {
  try {
    const urlObj = new URL(url);

    // Only allow http and https protocols
    if (urlObj.protocol !== 'http:' && urlObj.protocol !== 'https:') {
      return null;
    }

    // Validate hostname for known embed providers
    const allowedHosts = [
      'youtube.com',
      'youtu.be',
      'twitter.com',
      'x.com',
      'facebook.com',
      'instagram.com',
      'linkedin.com',
      'pinterest.com',
      'pin.it',
      'github.com',
      'gist.github.com',
      'reddit.com',
      'tiktok.com',
    ];

    const hostname = urlObj.hostname.toLowerCase();
    const isAllowed = allowedHosts.some(
      host => hostname === host || hostname.endsWith('.' + host)
    );

    if (!isAllowed) {
      return null;
    }

    return urlObj.toString();
  } catch {
    return null;
  }
};

export const sanitizeText = (text: string): string => {
  return text.replace(/[^a-zA-Z0-9\s-_]/g, '').trim();
};

export const sanitizeId = (id: string): string => {
  return id.replace(/[^a-zA-Z0-9-_]/g, '');
};

export const isValidUrl = (url: string): boolean => {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
};

// Script loading utilities
const loadedScripts = new Set<string>();

export const loadScript = (src: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    // Check if script is already loaded
    if (loadedScripts.has(src)) {
      resolve();
      return;
    }

    // Check if script already exists in DOM
    const existingScript = document.querySelector(`script[src="${src}"]`);
    if (existingScript) {
      loadedScripts.add(src);
      resolve();
      return;
    }

    // Create and load new script
    const script = document.createElement('script');
    script.src = src;
    script.async = true;
    script.onload = () => {
      loadedScripts.add(src);
      resolve();
    };
    script.onerror = () => {
      reject(new Error(`Failed to load script: ${src}`));
    };
    document.head.appendChild(script);
  });
};
