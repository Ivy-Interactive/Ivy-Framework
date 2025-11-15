import { describe, it, expect, vi, beforeEach } from 'vitest';
import * as utils from './utils';

describe('validateRedirectUrl', () => {
  let getCurrentOriginSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    // Create a mock function and replace the internal reference
    const mockFn = vi.fn(() => 'https://example.com');
    utils._getCurrentOriginRef.getCurrentOrigin = mockFn;
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
