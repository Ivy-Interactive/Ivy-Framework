import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import * as utils from './utils';
import * as urlValidation from './urlValidation';

describe('validateRedirectUrl', () => {
  let getCurrentOriginSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    // Create a mock function and replace the internal reference
    const mockFn = vi.fn(() => 'https://example.com');
    urlValidation._getCurrentOriginRef.getCurrentOrigin = mockFn;
    getCurrentOriginSpy = mockFn;
  });

  describe('valid relative paths', () => {
    it('should accept valid relative paths', () => {
      expect(utils.validateRedirectUrl('/dashboard')).toBe('/dashboard');
      expect(utils.validateRedirectUrl('/users/profile')).toBe(
        '/users/profile'
      );
      expect(utils.validateRedirectUrl('/app/settings')).toBe('/app/settings');
      expect(utils.validateRedirectUrl('/')).toBe('/');
    });

    it('should accept relative paths with query strings', () => {
      expect(utils.validateRedirectUrl('/path?query=value')).toBe(
        '/path?query=value'
      );
      expect(utils.validateRedirectUrl('/path#fragment')).toBe(
        '/path#fragment'
      );
    });
  });

  describe('external URLs', () => {
    it('should reject external URLs when allowExternal is false', () => {
      expect(utils.validateRedirectUrl('http://evil.com', false)).toBeNull();
      expect(utils.validateRedirectUrl('https://evil.com', false)).toBeNull();
    });

    it('should allow same-origin URLs when allowExternal is false', () => {
      expect(utils.validateRedirectUrl('https://example.com', false)).toBe(
        'https://example.com/'
      );
      expect(utils.validateRedirectUrl('https://example.com/path', false)).toBe(
        'https://example.com/path'
      );
    });

    it('should allow external URLs when allowExternal is true', () => {
      expect(utils.validateRedirectUrl('http://example.com', true)).toBe(
        'http://example.com/'
      );
      expect(utils.validateRedirectUrl('https://other.com', true)).toBe(
        'https://other.com/'
      );
    });

    it('should reject URLs with different ports', () => {
      getCurrentOriginSpy.mockReturnValue('https://example.com:8080');
      expect(
        utils.validateRedirectUrl('https://example.com:5000', false)
      ).toBeNull();
    });

    it('should reject URLs with different schemes', () => {
      getCurrentOriginSpy.mockReturnValue('http://example.com');
      expect(
        utils.validateRedirectUrl('https://example.com', false)
      ).toBeNull();
    });
  });

  describe('dangerous protocols', () => {
    it('should reject javascript: protocol', () => {
      expect(
        utils.validateRedirectUrl('javascript:alert("xss")', true)
      ).toBeNull();
    });

    it('should reject data: protocol', () => {
      expect(
        utils.validateRedirectUrl(
          'data:text/html,<script>alert("xss")</script>',
          true
        )
      ).toBeNull();
    });

    it('should reject file: protocol', () => {
      expect(utils.validateRedirectUrl('file:///etc/passwd', true)).toBeNull();
    });

    it('should reject vbscript: protocol', () => {
      expect(
        utils.validateRedirectUrl('vbscript:msgbox("xss")', true)
      ).toBeNull();
    });
  });

  describe('invalid inputs', () => {
    it('should return null for null input', () => {
      expect(utils.validateRedirectUrl(null)).toBeNull();
    });

    it('should return null for undefined input', () => {
      expect(utils.validateRedirectUrl(undefined)).toBeNull();
    });

    it('should return null for empty string', () => {
      expect(utils.validateRedirectUrl('')).toBeNull();
    });

    it('should return null for whitespace-only string', () => {
      expect(utils.validateRedirectUrl('   ')).toBeNull();
      expect(utils.validateRedirectUrl('\t')).toBeNull();
    });

    it('should trim whitespace', () => {
      expect(utils.validateRedirectUrl('  /dashboard  ')).toBe('/dashboard');
    });
  });

  describe('relative paths with colons', () => {
    it('should reject relative paths containing colons', () => {
      expect(utils.validateRedirectUrl('/path:with:colons')).toBeNull();
    });
  });

  describe('invalid URL formats', () => {
    it('should return null for malformed URLs', () => {
      expect(utils.validateRedirectUrl('not-a-url', true)).toBeNull();
      expect(utils.validateRedirectUrl('://malformed', true)).toBeNull();
      expect(utils.validateRedirectUrl('invalid://url', true)).toBeNull();
    });
  });
});

describe('validateLinkUrl', () => {
  describe('valid relative paths', () => {
    it('should accept valid relative paths', () => {
      expect(utils.validateLinkUrl('/dashboard')).toBe('/dashboard');
      expect(utils.validateLinkUrl('/users/profile')).toBe('/users/profile');
      expect(utils.validateLinkUrl('/app/settings')).toBe('/app/settings');
      expect(utils.validateLinkUrl('/')).toBe('/');
    });

    it('should accept relative paths with query strings and fragments', () => {
      expect(utils.validateLinkUrl('/path?query=value')).toBe(
        '/path?query=value'
      );
      expect(utils.validateLinkUrl('/path#fragment')).toBe('/path#fragment');
    });
  });

  describe('valid http/https URLs', () => {
    it('should accept valid http URLs', () => {
      const result = utils.validateLinkUrl('http://example.com');
      expect(result).toBeTruthy();
      expect(result).toContain('http://example.com');
    });

    it('should accept valid https URLs', () => {
      const result = utils.validateLinkUrl('https://example.com');
      expect(result).toBeTruthy();
      expect(result).toContain('https://example.com');
    });

    it('should accept URLs with paths', () => {
      expect(utils.validateLinkUrl('https://example.com/path')).toBe(
        'https://example.com/path'
      );
    });

    it('should accept URLs with query strings', () => {
      expect(
        utils.validateLinkUrl('https://example.com/path?query=value')
      ).toBe('https://example.com/path?query=value');
    });

    it('should accept URLs with fragments', () => {
      expect(utils.validateLinkUrl('https://example.com/path#fragment')).toBe(
        'https://example.com/path#fragment'
      );
    });

    it('should accept URLs with ports', () => {
      expect(utils.validateLinkUrl('http://example.com:8080/path')).toBe(
        'http://example.com:8080/path'
      );
    });
  });

  describe('app:// URLs', () => {
    it('should accept valid app:// URLs', () => {
      expect(utils.validateLinkUrl('app://dashboard')).toBe('app://dashboard');
      expect(utils.validateLinkUrl('app://users/profile')).toBe(
        'app://users/profile'
      );
      expect(utils.validateLinkUrl('app://my-app')).toBe('app://my-app');
    });

    it('should accept app:// URLs with query strings', () => {
      expect(utils.validateLinkUrl('app://path?query=value')).toBe(
        'app://path?query=value'
      );
      expect(utils.validateLinkUrl('app://MyApp?param=value')).toBe(
        'app://MyApp?param=value'
      );
    });

    it('should reject app:// URLs with fragments', () => {
      expect(utils.validateLinkUrl('app://path#fragment')).toBe('#');
    });

    it('should accept app:// URLs with ampersands in query strings', () => {
      expect(
        utils.validateLinkUrl('app://path?param1=value1&param2=value2')
      ).toBe('app://path?param1=value1&param2=value2');
    });

    it('should reject app:// URLs with multiple colons', () => {
      expect(utils.validateLinkUrl('app://path:extra:colons')).toBe('#');
    });
  });

  describe('anchor links', () => {
    it('should accept valid anchor links', () => {
      expect(utils.validateLinkUrl('#section1')).toBe('#section1');
      expect(utils.validateLinkUrl('#heading-2')).toBe('#heading-2');
      expect(utils.validateLinkUrl('#_anchor')).toBe('#_anchor');
    });

    it('should reject anchor links with query strings', () => {
      expect(utils.validateLinkUrl('#anchor?query=value')).toBe('#');
    });

    it('should reject anchor links with ampersands', () => {
      expect(utils.validateLinkUrl('#anchor&evil')).toBe('#');
    });

    it('should accept anchor links with colons', () => {
      expect(utils.validateLinkUrl('#anchor:colons')).toBe('#anchor:colons');
      expect(utils.validateLinkUrl('#section:value')).toBe('#section:value');
    });
  });

  describe('dangerous protocols', () => {
    it('should reject javascript: protocol', () => {
      expect(utils.validateLinkUrl('javascript:alert("xss")')).toBe('#');
    });

    it('should reject data: protocol', () => {
      expect(
        utils.validateLinkUrl('data:text/html,<script>alert("xss")</script>')
      ).toBe('#');
    });

    it('should reject file: protocol', () => {
      expect(utils.validateLinkUrl('file:///etc/passwd')).toBe('#');
    });

    it('should reject vbscript: protocol', () => {
      expect(utils.validateLinkUrl('vbscript:msgbox("xss")')).toBe('#');
    });
  });

  describe('invalid inputs', () => {
    it('should return # for null input', () => {
      expect(utils.validateLinkUrl(null)).toBe('#');
    });

    it('should return # for undefined input', () => {
      expect(utils.validateLinkUrl(undefined)).toBe('#');
    });

    it('should return # for empty string', () => {
      expect(utils.validateLinkUrl('')).toBe('#');
    });

    it('should return # for whitespace-only string', () => {
      // After trimming, whitespace becomes empty string, which should return #
      expect(utils.validateLinkUrl('   ')).toBe('#');
      expect(utils.validateLinkUrl('\t')).toBe('#');
    });

    it('should trim whitespace', () => {
      expect(utils.validateLinkUrl('  /dashboard  ')).toBe('/dashboard');
    });
  });

  describe('relative paths with colons', () => {
    it('should reject relative paths containing colons', () => {
      expect(utils.validateLinkUrl('/path:with:colons')).toBe('#');
    });
  });

  describe('relative paths without leading slash', () => {
    it('should add leading slash to relative paths', () => {
      expect(utils.validateLinkUrl('relative-path')).toBe('/relative-path');
      expect(utils.validateLinkUrl('path/without/leading/slash')).toBe(
        '/path/without/leading/slash'
      );
    });
  });

  describe('invalid URL formats', () => {
    it('should return # for malformed URLs with colons', () => {
      expect(utils.validateLinkUrl('invalid://url')).toBe('#');
      expect(utils.validateLinkUrl('://malformed')).toBe('#');
    });

    it('should treat URLs without colons as relative paths', () => {
      expect(utils.validateLinkUrl('not-a-url')).toBe('/not-a-url');
    });
  });

  describe('edge cases', () => {
    it('should handle URLs with subdomains', () => {
      const result = utils.validateLinkUrl('https://subdomain.example.com');
      expect(result).toBeTruthy();
      expect(result).toContain('https://subdomain.example.com');
    });

    it('should handle URLs with multiple path segments', () => {
      expect(
        utils.validateLinkUrl('https://example.com/path/to/resource')
      ).toBe('https://example.com/path/to/resource');
    });

    it('should handle URLs with encoded characters', () => {
      expect(
        utils.validateLinkUrl('https://example.com/path%20with%20spaces')
      ).toBe('https://example.com/path%20with%20spaces');
    });
  });
});

describe('getIvyHost', () => {
  const defaultOrigin = window.location.origin;

  const setIvyHostMeta = (value: string) => {
    const meta = document.createElement('meta');
    meta.setAttribute('name', 'ivy-host');
    meta.setAttribute('content', value);
    document.head.appendChild(meta);
  };

  beforeEach(() => {
    document.head.innerHTML = '';
  });

  afterEach(() => {
    document.head.innerHTML = '';
  });

  it('returns the meta host when it matches the current hostname', () => {
    // Mock current origin to match the test expectation
    const originalOrigin = window.location.origin;
    Object.defineProperty(window, 'location', {
      value: {
        ...window.location,
        origin: 'https://localhost:5173',
        hostname: 'localhost',
      },
      writable: true,
      configurable: true,
    });

    try {
      setIvyHostMeta('https://localhost:5173');
      expect(utils.getIvyHost()).toBe('https://localhost:5173');
    } finally {
      // Restore original location
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: originalOrigin,
        },
        writable: true,
        configurable: true,
      });
    }
  });

  it('supports hostname-only meta values by assuming https', () => {
    // Mock current origin to be https://localhost for this test
    const originalOrigin = window.location.origin;
    Object.defineProperty(window, 'location', {
      value: {
        ...window.location,
        origin: 'https://localhost',
        hostname: 'localhost',
      },
      writable: true,
      configurable: true,
    });

    try {
      setIvyHostMeta('localhost');
      expect(utils.getIvyHost()).toBe('https://localhost');
    } finally {
      // Restore original location
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: originalOrigin,
        },
        writable: true,
        configurable: true,
      });
    }
  });

  it('falls back to the current origin when the meta host is not allowed', () => {
    setIvyHostMeta('https://malicious.example.com');
    expect(utils.getIvyHost()).toBe(defaultOrigin);
  });

  it('falls back when the meta tag contains an invalid value', () => {
    setIvyHostMeta('not a url');
    expect(utils.getIvyHost()).toBe(defaultOrigin);
  });

  it('falls back when no meta tag is present', () => {
    expect(utils.getIvyHost()).toBe(defaultOrigin);
  });

  describe('localhost variant matching for development', () => {
    // Store original location properties for cleanup
    const originalOrigin = window.location.origin;
    const originalHostname = window.location.hostname;

    beforeEach(() => {
      // Reset to original before each test
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    afterEach(() => {
      // Restore original location
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    it('allows localhost with different ports when current origin is localhost', () => {
      // Mock current origin as localhost
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('http://localhost:8080');
    });

    it('rejects localhost with different protocols (security: prevent protocol downgrade)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      // Should reject HTTP when current origin is HTTPS (prevent downgrade)
      setIvyHostMeta('http://localhost:5173');
      expect(utils.getIvyHost()).toBe('https://localhost:5173');
    });

    it('rejects HTTPS when current origin is HTTP (security: prevent protocol upgrade)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      // Should reject HTTPS when current origin is HTTP (prevent unexpected upgrade)
      setIvyHostMeta('https://localhost:5173');
      expect(utils.getIvyHost()).toBe('http://localhost:5173');
    });

    it('allows localhost with same protocol but different ports', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      // Should allow different ports when protocol matches
      setIvyHostMeta('https://localhost:8080');
      expect(utils.getIvyHost()).toBe('https://localhost:8080');
    });

    it('allows 127.0.0.1 with different ports when current origin is 127.0.0.1', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://127.0.0.1:5173',
          hostname: '127.0.0.1',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://127.0.0.1:8080');
      expect(utils.getIvyHost()).toBe('http://127.0.0.1:8080');
    });

    it('allows localhost and 127.0.0.1 to match when protocols match (same logical host)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://127.0.0.1:8080');
      expect(utils.getIvyHost()).toBe('http://127.0.0.1:8080');
    });

    it('rejects localhost and 127.0.0.1 match when protocols differ (security)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      // Should reject HTTP even for same logical host if protocol differs
      setIvyHostMeta('http://127.0.0.1:8080');
      expect(utils.getIvyHost()).toBe('https://localhost:5173');
    });

    it('allows 127.0.0.1 and localhost to match (reverse direction)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://127.0.0.1:5173',
          hostname: '127.0.0.1',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('http://localhost:8080');
    });

    it('allows [::1] (IPv6 localhost) with different ports', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://[::1]:5173',
          hostname: '[::1]',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://[::1]:8080');
      expect(utils.getIvyHost()).toBe('http://[::1]:8080');
    });

    it('allows localhost and [::1] to match when protocols match (same logical host)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://[::1]:8080');
      expect(utils.getIvyHost()).toBe('http://[::1]:8080');
    });

    it('rejects localhost and [::1] match when protocols differ (security)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      // Should reject HTTP even for same logical host if protocol differs
      setIvyHostMeta('http://[::1]:8080');
      expect(utils.getIvyHost()).toBe('https://localhost:5173');
    });

    it('rejects non-localhost hosts even with same hostname pattern', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://example.com:8080');
      // Should fall back to default origin, not allow the different port
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects localhost when current origin is not localhost (security)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://localhost:8080');
      // Should fall back to default origin, not allow localhost
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects non-localhost when current origin is localhost (security)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://example.com:8080');
      // Should fall back to default origin, not allow external host
      expect(utils.getIvyHost()).toBe('http://localhost:5173');
    });

    it('handles case-insensitive localhost matching', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://LOCALHOST:5173',
          hostname: 'LOCALHOST',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('http://localhost:8080');
    });

    it('allows exact origin match to take precedence over localhost variant check', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:5173',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://localhost:5173');
      expect(utils.getIvyHost()).toBe('http://localhost:5173');
    });
  });

  describe('production environment security', () => {
    const originalOrigin = window.location.origin;
    const originalHostname = window.location.hostname;

    beforeEach(() => {
      // Reset to original before each test
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    afterEach(() => {
      // Restore original location
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    it('requires exact origin match in production (non-localhost)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      // Exact match should work
      setIvyHostMeta('https://example.com');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects different ports in production (non-localhost)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      // Different port should be rejected
      setIvyHostMeta('https://example.com:8080');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects different protocols in production (non-localhost)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      // Different protocol should be rejected
      setIvyHostMeta('http://example.com');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects localhost variants in production environment', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      // Localhost should be rejected when current origin is production
      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects 127.0.0.1 in production environment', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://127.0.0.1:8080');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects IPv6 localhost in production environment', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('http://[::1]:8080');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects different subdomains in production', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('https://subdomain.example.com');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('rejects completely different domains in production', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('https://attacker.com');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('allows exact match with port in production when current origin has port', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com:8443',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('https://example.com:8443');
      expect(utils.getIvyHost()).toBe('https://example.com:8443');
    });

    it('rejects different port even when both have explicit ports in production', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com:8443',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta('https://example.com:9443');
      expect(utils.getIvyHost()).toBe('https://example.com:8443');
    });

    it('handles production environment with custom domain', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://app.mycompany.com',
          hostname: 'app.mycompany.com',
        },
        writable: true,
        configurable: true,
      });

      // Exact match should work
      setIvyHostMeta('https://app.mycompany.com');
      expect(utils.getIvyHost()).toBe('https://app.mycompany.com');

      // Different port should be rejected
      setIvyHostMeta('https://app.mycompany.com:8080');
      expect(utils.getIvyHost()).toBe('https://app.mycompany.com');
    });

    it('handles production environment with IP address (non-localhost)', () => {
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://192.168.1.100',
          hostname: '192.168.1.100',
        },
        writable: true,
        configurable: true,
      });

      // Exact match should work
      setIvyHostMeta('https://192.168.1.100');
      expect(utils.getIvyHost()).toBe('https://192.168.1.100');

      // Different port should be rejected
      setIvyHostMeta('https://192.168.1.100:8080');
      expect(utils.getIvyHost()).toBe('https://192.168.1.100');
    });

    it('verifies localhost matching only works when current origin is localhost', () => {
      // Test 1: localhost → localhost (should work)
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'http://localhost:3000',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('http://localhost:8080');

      // Test 2: production → localhost (should NOT work)
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });

    it('verifies protocol matching requirement in both environments', () => {
      // Development: protocol mismatch should be rejected
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://localhost:3000',
          hostname: 'localhost',
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta('http://localhost:8080');
      expect(utils.getIvyHost()).toBe('https://localhost:3000');

      // Production: protocol mismatch should also be rejected
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          origin: 'https://example.com',
          hostname: 'example.com',
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta('http://example.com');
      expect(utils.getIvyHost()).toBe('https://example.com');
    });
  });
});

const mediaValidationCases = [
  {
    name: 'validateImageUrl',
    validate: utils.validateImageUrl,
    validDataUrl: 'data:image/png;base64,AAAA',
    invalidDataUrl: 'data:text/html;base64,AAAA',
  },
  {
    name: 'validateAudioUrl',
    validate: utils.validateAudioUrl,
    validDataUrl: 'data:audio/ogg;base64,AAAA',
    invalidDataUrl: 'data:text/plain;base64,AAAA',
  },
  {
    name: 'validateVideoUrl',
    validate: utils.validateVideoUrl,
    validDataUrl: 'data:video/mp4;base64,AAAA',
    invalidDataUrl: 'data:text/plain;base64,AAAA',
  },
] as const;

describe.each(mediaValidationCases)(
  '$name',
  ({ validate, validDataUrl, invalidDataUrl }) => {
    it('returns null for empty or non-string values', () => {
      expect(validate(null)).toBeNull();
      expect(validate(undefined)).toBeNull();
      expect(validate('')).toBeNull();
      expect(validate('   ')).toBeNull();
    });

    it('accepts valid https URLs', () => {
      expect(validate('https://example.com/resource')).toBe(
        'https://example.com/resource'
      );
    });

    it('accepts valid relative paths', () => {
      expect(validate('/media/resource.ext')).toBe('/media/resource.ext');
    });

    it('accepts valid data URLs of the correct media type', () => {
      expect(validate(validDataUrl)).toBe(validDataUrl);
    });

    it('rejects data URLs that use the wrong media type', () => {
      expect(validate(invalidDataUrl)).toBeNull();
    });

    describe('blob URL validation', () => {
      let getCurrentOriginSpy: ReturnType<typeof vi.fn>;

      beforeEach(() => {
        // Create a mock function and replace the internal reference
        const mockFn = vi.fn(() => 'https://example.com');
        urlValidation._getCurrentOriginRef.getCurrentOrigin = mockFn;
        getCurrentOriginSpy = mockFn;
      });

      it('accepts blob URLs with matching origin', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(validate('blob:https://example.com/1234-5678-90ab-cdef')).toBe(
          'blob:https://example.com/1234-5678-90ab-cdef'
        );
      });

      it('rejects blob URLs with different origin', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(
          validate('blob:https://attacker.com/1234-5678-90ab-cdef')
        ).toBeNull();
      });

      it('rejects blob URLs with different protocol', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(
          validate('blob:http://example.com/1234-5678-90ab-cdef')
        ).toBeNull();
      });

      it('rejects blob URLs with different port', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com:443');
        expect(
          validate('blob:https://example.com:8080/1234-5678-90ab-cdef')
        ).toBeNull();
      });

      it('accepts blob URLs with same origin but default port normalized', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        // Blob URL with explicit :443 should match origin without port
        expect(
          validate('blob:https://example.com:443/1234-5678-90ab-cdef')
        ).toBe('blob:https://example.com:443/1234-5678-90ab-cdef');
      });

      it('accepts blob URLs when both have default ports normalized', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com:443');
        expect(validate('blob:https://example.com/1234-5678-90ab-cdef')).toBe(
          'blob:https://example.com/1234-5678-90ab-cdef'
        );
      });

      it('rejects blob URLs when current origin is missing (SSR scenario)', () => {
        getCurrentOriginSpy.mockReturnValue('');
        expect(
          validate('blob:https://example.com/1234-5678-90ab-cdef')
        ).toBeNull();
      });

      it('rejects blob URLs with invalid format (no slash)', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(validate('blob:https://example.com')).toBeNull();
      });

      it('rejects blob URLs with invalid format (no origin)', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(validate('blob:/1234-5678-90ab-cdef')).toBeNull();
      });

      it('handles blob URLs with complex UUIDs', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        const complexBlobUrl =
          'blob:https://example.com/550e8400-e29b-41d4-a716-446655440000';
        expect(validate(complexBlobUrl)).toBe(complexBlobUrl);
      });

      it('handles blob URLs with paths after UUID', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        // Blob URLs typically have format blob:origin/uuid, but we only extract up to first slash
        const blobUrl = 'blob:https://example.com/uuid-123';
        expect(validate(blobUrl)).toBe(blobUrl);
      });

      it('handles http origins correctly', () => {
        getCurrentOriginSpy.mockReturnValue('http://localhost:8080');
        expect(validate('blob:http://localhost:8080/1234-5678-90ab-cdef')).toBe(
          'blob:http://localhost:8080/1234-5678-90ab-cdef'
        );
      });

      it('rejects blob URLs with http when current origin is https', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(
          validate('blob:http://example.com/1234-5678-90ab-cdef')
        ).toBeNull();
      });

      it('handles default http port normalization', () => {
        getCurrentOriginSpy.mockReturnValue('http://example.com');
        // Blob URL with explicit :80 should match origin without port
        expect(validate('blob:http://example.com:80/1234-5678-90ab-cdef')).toBe(
          'blob:http://example.com:80/1234-5678-90ab-cdef'
        );
      });

      // Additional edge cases for comprehensive coverage
      it('rejects blob URLs with different subdomain', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(validate('blob:https://subdomain.example.com/uuid')).toBeNull();
      });

      it('rejects blob URLs with malformed origin (no protocol)', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        expect(validate('blob:example.com/uuid')).toBeNull();
      });

      it('rejects blob URLs with malformed origin (invalid protocol)', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        // The validateMediaUrl function should reject javascript: protocol
        expect(validate('blob:javascript://example.com/uuid')).toBeNull();
      });

      it('handles blob URLs with custom ports correctly', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com:8443');
        expect(validate('blob:https://example.com:8443/uuid-123')).toBe(
          'blob:https://example.com:8443/uuid-123'
        );
      });

      it('rejects blob URLs with same host but different port', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com:8443');
        expect(validate('blob:https://example.com:9443/uuid-123')).toBeNull();
      });

      it('handles blob URLs with IPv4 addresses', () => {
        getCurrentOriginSpy.mockReturnValue('http://192.168.1.1:8080');
        expect(validate('blob:http://192.168.1.1:8080/uuid-123')).toBe(
          'blob:http://192.168.1.1:8080/uuid-123'
        );
      });

      it('rejects blob URLs with different IPv4 address', () => {
        getCurrentOriginSpy.mockReturnValue('http://192.168.1.1:8080');
        expect(validate('blob:http://192.168.1.2:8080/uuid-123')).toBeNull();
      });

      it('handles blob URLs with localhost variants', () => {
        getCurrentOriginSpy.mockReturnValue('http://localhost:3000');
        expect(validate('blob:http://localhost:3000/uuid-123')).toBe(
          'blob:http://localhost:3000/uuid-123'
        );
      });

      it('rejects blob URLs when blob origin is empty (double slash)', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        // This should be caught by the invalid format check
        expect(validate('blob://uuid-123')).toBeNull();
      });

      it('rejects blob URLs with protocol injection attempt', () => {
        getCurrentOriginSpy.mockReturnValue('https://example.com');
        // Attempt to inject javascript: protocol
        expect(validate('blob:javascript:alert(1)')).toBeNull();
      });
    });

    it('accepts safe app:// URLs and rejects app:// URLs with colons in the path', () => {
      expect(validate('app://media/resource')).toBe('app://media/resource');
      expect(validate('app://media:fragment#bad')).toBeNull();
    });

    it('rejects javascript protocol attempts', () => {
      expect(validate('javascript:alert(1)')).toBeNull();
    });

    it('rejects relative paths containing colons', () => {
      expect(validate('/media:bad:path')).toBeNull();
    });

    it('coerces colon-free relative strings into rooted paths', () => {
      expect(validate('images/resource.ext')).toBe('/images/resource.ext');
    });
  }
);
