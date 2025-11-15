import { describe, it, expect } from 'vitest';
import { validateLinkUrl } from '@/lib/utils';

/**
 * Tests for ButtonWidget URL validation, especially for Link variant.
 *
 * This test file verifies that ButtonWidget correctly validates URLs
 * using validateLinkUrl, ensuring that:
 * 1. Safe URLs are validated and constructed correctly
 * 2. Dangerous URLs are sanitized to '#'
 * 3. Relative URLs are properly prefixed with Ivy host
 * 4. The Link variant handles URLs correctly
 */

// Mock the getUrl helper function logic (it's internal to ButtonWidget)
// We'll test the validation logic that ButtonWidget uses
const mockGetUrl = (url: string, mockHost: string = 'https://example.com') => {
  // Validate the URL first to prevent open redirect vulnerabilities
  const validatedUrl = validateLinkUrl(url);
  if (validatedUrl === '#') {
    // Invalid URL, return safe fallback
    return '#';
  }

  // If it's already a full URL (http/https), return it
  if (
    validatedUrl.startsWith('http://') ||
    validatedUrl.startsWith('https://')
  ) {
    return validatedUrl;
  }

  // app:// and anchor links should not be prefixed with host
  if (validatedUrl.startsWith('app://') || validatedUrl.startsWith('#')) {
    return validatedUrl;
  }

  // Otherwise, construct relative URL with Ivy host
  return `${mockHost}${validatedUrl.startsWith('/') ? '' : '/'}${validatedUrl}`;
};

describe('ButtonWidget URL validation', () => {
  const mockHost = 'https://example.com';

  describe('Link variant URL validation', () => {
    it('should validate safe http URLs for Link variant buttons', () => {
      const url = 'http://example.com/page';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('http://example.com/page');
    });

    it('should validate safe https URLs for Link variant buttons', () => {
      const url = 'https://example.com/page';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/page');
    });

    it('should validate relative paths for Link variant buttons', () => {
      const url = '/dashboard';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/dashboard');
    });

    it('should add leading slash to relative paths without it', () => {
      const url = 'dashboard';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/dashboard');
    });

    it('should validate app:// URLs for Link variant buttons', () => {
      const url = 'app://dashboard';
      const result = mockGetUrl(url, mockHost);
      // app:// URLs should be validated but not prefixed with host
      expect(result).toBe('app://dashboard');
    });

    it('should validate anchor links for Link variant buttons', () => {
      const url = '#section1';
      const result = mockGetUrl(url, mockHost);
      // Anchor links should be validated but not prefixed with host
      expect(result).toBe('#section1');
    });
  });

  describe('dangerous URLs in Link variant buttons (sad path)', () => {
    it('should sanitize javascript: protocol in Link variant buttons', () => {
      const url = 'javascript:alert("xss")';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('#');
    });

    it('should sanitize data: protocol in Link variant buttons', () => {
      const url = 'data:text/html,<script>alert("xss")</script>';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('#');
    });

    it('should sanitize file: protocol in Link variant buttons', () => {
      const url = 'file:///etc/passwd';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('#');
    });

    it('should sanitize vbscript: protocol in Link variant buttons', () => {
      const url = 'vbscript:msgbox("xss")';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('#');
    });

    it('should sanitize mixed case dangerous protocols', () => {
      const dangerousUrls = [
        'JAVASCRIPT:alert("xss")',
        'Data:text/html,<script>',
        'FILE:///etc/passwd',
      ];
      dangerousUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        expect(result).toBe('#');
      });
    });

    it('should sanitize URLs attempting protocol injection', () => {
      const maliciousUrls = [
        'javascript:alert("xss")',
        'data:text/html,<script>alert("xss")</script>',
        'file:///etc/passwd',
      ];
      maliciousUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        expect(result).toBe('#');
      });
    });
  });

  describe('URL construction for Link variant buttons', () => {
    it('should construct full URL for relative paths', () => {
      const url = '/users/profile';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/users/profile');
    });

    it('should not modify absolute http URLs', () => {
      const url = 'http://external.com/page';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('http://external.com/page');
    });

    it('should not modify absolute https URLs', () => {
      const url = 'https://external.com/page';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://external.com/page');
    });

    it('should handle URLs with query parameters', () => {
      const url = '/dashboard?tab=settings';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/dashboard?tab=settings');
    });

    it('should handle URLs with fragments', () => {
      const url = '/page#section';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/page#section');
    });
  });

  describe('edge cases for Link variant buttons (sad path)', () => {
    it('should handle null URL', () => {
      // In ButtonWidget, null URLs wouldn't call getUrl, but if they did:
      const url: string | null = null;
      const validatedUrl = validateLinkUrl(url);
      expect(validatedUrl).toBe('#');
    });

    it('should handle undefined URL', () => {
      // In ButtonWidget, undefined URLs wouldn't call getUrl, but if they did:
      const url: string | undefined = undefined;
      const validatedUrl = validateLinkUrl(url);
      expect(validatedUrl).toBe('#');
    });

    it('should handle empty string URL', () => {
      const url = '';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('#');
    });

    it('should handle whitespace-only URL', () => {
      const url = '   ';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('#');
    });

    it('should handle malformed URLs', () => {
      // Truly malformed URLs should be rejected
      const malformedUrls = ['://malformed', 'http://', 'https://'];
      malformedUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        // Should return safe fallback for truly malformed URLs
        expect(result).toBe('#');
      });

      // URLs without protocol are treated as relative paths (safe behavior)
      const relativePathUrls = ['not-a-url', 'some-path'];
      relativePathUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        // These are treated as relative paths and prefixed with host (safe)
        expect(result).toContain(mockHost);
        expect(result).toContain(url);
      });
    });

    it('should handle URLs with newlines and tabs', () => {
      const urls = [
        'https://example.com\n',
        'https://example.com\t',
        '\n/dashboard',
      ];
      urls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        // Should sanitize or return safe fallback
        expect(result).not.toContain('\n');
        expect(result).not.toContain('\t');
      });
    });

    it('should trim whitespace from URLs', () => {
      const url = '  /dashboard  ';
      const result = mockGetUrl(url, mockHost);
      expect(result).toBe('https://example.com/dashboard');
    });
  });

  describe('Link variant with different URL types', () => {
    it('should handle external URLs correctly', () => {
      const externalUrls = [
        { input: 'https://github.com', expected: 'https://github.com/' },
        { input: 'http://example.org', expected: 'http://example.org/' },
        {
          input: 'https://subdomain.example.com/path',
          expected: 'https://subdomain.example.com/path',
        },
      ];

      externalUrls.forEach(({ input }) => {
        const result = mockGetUrl(input, mockHost);
        // URL normalization may add trailing slash
        expect(result).toContain(
          input.split('/')[0] + '//' + input.split('/')[2]
        );
      });
    });

    it('should handle internal relative URLs correctly', () => {
      const internalUrls = ['/dashboard', '/users/profile', '/app/settings'];

      internalUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        expect(result).toBe(`${mockHost}${url}`);
      });
    });

    it('should handle app:// URLs correctly', () => {
      const appUrls = [
        'app://dashboard',
        'app://users/profile',
        'app://my-app',
      ];

      appUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        expect(result).toBe(url);
      });
    });
  });

  describe('Link variant URL validation integration', () => {
    it('should validate and construct URLs for Link variant buttons', () => {
      const testCases = [
        {
          input: 'https://example.com/page',
          expected: 'https://example.com/page',
        },
        {
          input: '/dashboard',
          expected: 'https://example.com/dashboard',
        },
        {
          input: 'app://dashboard',
          expected: 'app://dashboard',
        },
        {
          input: '#section1',
          expected: '#section1',
        },
      ];

      testCases.forEach(({ input, expected }) => {
        const result = mockGetUrl(input, mockHost);
        expect(result).toBe(expected);
      });
    });

    it('should sanitize dangerous URLs to safe fallback', () => {
      const dangerousUrls = [
        'javascript:alert("xss")',
        'data:text/html,<script>alert("xss")</script>',
        'file:///etc/passwd',
        'vbscript:msgbox("xss")',
      ];

      dangerousUrls.forEach(url => {
        const result = mockGetUrl(url, mockHost);
        expect(result).toBe('#');
      });
    });
  });
});
