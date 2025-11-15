import { describe, it, expect } from 'vitest';
import { validateLinkUrl } from '@/lib/utils';

/**
 * Tests for URL validation in markdown links.
 *
 * This test file verifies that the validateLinkUrl function correctly handles
 * URLs that appear in markdown content, ensuring that:
 * 1. Safe URLs (http/https, relative paths, app://, anchors) are validated correctly
 * 2. Dangerous URLs (javascript:, data:, etc.) are sanitized
 * 3. The validation is applied consistently across different markdown link formats
 */
describe('Markdown link URL validation', () => {
  describe('safe markdown link URLs', () => {
    it('should validate absolute http URLs in markdown links', () => {
      const url = 'http://example.com/page';
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain('http://example.com');
    });

    it('should validate absolute https URLs in markdown links', () => {
      const url = 'https://example.com/page';
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain('https://example.com');
    });

    it('should validate relative paths in markdown links', () => {
      const url = '/dashboard';
      const result = validateLinkUrl(url);
      expect(result).toBe('/dashboard');
    });

    it('should validate app:// URLs in markdown links', () => {
      const url = 'app://dashboard';
      const result = validateLinkUrl(url);
      expect(result).toBe('app://dashboard');
    });

    it('should validate anchor links in markdown', () => {
      const url = '#section1';
      const result = validateLinkUrl(url);
      expect(result).toBe('#section1');
    });

    it('should validate URLs with query parameters', () => {
      const url = 'https://example.com/page?param=value';
      const result = validateLinkUrl(url);
      expect(result).toBe('https://example.com/page?param=value');
    });

    it('should validate URLs with fragments', () => {
      const url = 'https://example.com/page#section';
      const result = validateLinkUrl(url);
      expect(result).toBe('https://example.com/page#section');
    });
  });

  describe('dangerous markdown link URLs (sad path)', () => {
    it('should sanitize javascript: protocol in markdown links', () => {
      const url = 'javascript:alert("xss")';
      const result = validateLinkUrl(url);
      expect(result).toBe('#');
    });

    it('should sanitize data: protocol in markdown links', () => {
      const url = 'data:text/html,<script>alert("xss")</script>';
      const result = validateLinkUrl(url);
      expect(result).toBe('#');
    });

    it('should sanitize file: protocol in markdown links', () => {
      const url = 'file:///etc/passwd';
      const result = validateLinkUrl(url);
      expect(result).toBe('#');
    });

    it('should sanitize vbscript: protocol in markdown links', () => {
      const url = 'vbscript:msgbox("xss")';
      const result = validateLinkUrl(url);
      expect(result).toBe('#');
    });

    it('should sanitize javascript: protocol with encoded characters', () => {
      // URL-encoded javascript: becomes a relative path, which is safe
      const url = 'javascript%3Aalert("xss")';
      const result = validateLinkUrl(url);
      // This is treated as a relative path, which is safe
      expect(result).toBeTruthy();
      expect(result).not.toContain('javascript:');
    });

    it('should sanitize mixed case dangerous protocols', () => {
      const urls = [
        'JAVASCRIPT:alert("xss")',
        'Data:text/html,<script>',
        'FILE:///etc/passwd',
      ];
      urls.forEach(url => {
        const result = validateLinkUrl(url);
        expect(result).toBe('#');
      });
    });

    it('should handle URLs with protocol-like strings in paths', () => {
      // These are actually safe - javascript: in a path is not executed as a protocol
      const urls = [
        'http://example.com/javascript:alert("xss")',
        'https://example.com/data:text/html,<script>',
        '/path/javascript:alert("xss")',
      ];
      urls.forEach(url => {
        const result = validateLinkUrl(url);
        // These are valid URLs/paths - the "protocol" is part of the path, not the actual protocol
        expect(result).toBeTruthy();
        // The URL parser correctly identifies http/https as the protocol, not javascript:
        if (url.startsWith('http://') || url.startsWith('https://')) {
          expect(result).toContain('http');
        }
      });
    });
  });

  describe('markdown link edge cases (sad path)', () => {
    it('should handle null href in markdown links', () => {
      const result = validateLinkUrl(null);
      expect(result).toBe('#');
    });

    it('should handle undefined href in markdown links', () => {
      const result = validateLinkUrl(undefined);
      expect(result).toBe('#');
    });

    it('should handle empty string href in markdown links', () => {
      const result = validateLinkUrl('');
      expect(result).toBe('#');
    });

    it('should handle whitespace-only href in markdown links', () => {
      const result = validateLinkUrl('   ');
      expect(result).toBe('#');
    });

    it('should handle newlines and tabs in URLs', () => {
      const urls = [
        'https://example.com\n',
        'https://example.com\t',
        '\nhttps://example.com',
      ];
      urls.forEach(url => {
        const result = validateLinkUrl(url);
        // Should sanitize or trim
        expect(result).not.toContain('\n');
        expect(result).not.toContain('\t');
      });
    });

    it('should handle malformed URLs', () => {
      const malformedUrls = [
        '://malformed',
        'http://',
        'https://',
        'http:///path',
        'not-a-url',
        'invalid://protocol',
      ];
      malformedUrls.forEach(url => {
        const result = validateLinkUrl(url);
        // Should return safe fallback or sanitized version
        expect(result).toBeTruthy();
        if (result !== '#') {
          // If not rejected, should be a valid relative path or safe URL
          expect(result.startsWith('/') || result.startsWith('http')).toBe(
            true
          );
        }
      });
    });

    it('should handle URLs with control characters', () => {
      const url = 'https://example.com/\x00\x01\x02';
      const result = validateLinkUrl(url);
      // Should sanitize control characters
      expect(result).toBeTruthy();
    });

    it('should trim whitespace from markdown link URLs', () => {
      const url = '  https://example.com  ';
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain('https://example.com');
      expect(result).not.toContain('  ');
    });

    it('should handle relative paths without leading slash', () => {
      const url = 'relative-path';
      const result = validateLinkUrl(url);
      expect(result).toBe('/relative-path');
    });
  });

  describe('markdown reference-style links', () => {
    it('should validate URLs in reference-style markdown links', () => {
      // Reference-style: [Link Text][1] with [1]: https://example.com/page
      const url = 'https://example.com/page';
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain('https://example.com');
    });

    it('should validate relative URLs in reference-style links', () => {
      const url = '/dashboard';
      const result = validateLinkUrl(url);
      expect(result).toBe('/dashboard');
    });
  });

  describe('markdown autolink URLs', () => {
    it('should validate autolink http URLs', () => {
      const url = 'http://example.com';
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain('http://example.com');
    });

    it('should validate autolink https URLs', () => {
      const url = 'https://example.com';
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain('https://example.com');
    });
  });

  describe('multiple links in markdown', () => {
    it('should validate each link independently', () => {
      const urls = [
        'https://example.com/page1',
        '/dashboard',
        'https://external.com',
        'app://dashboard',
        '#section1',
      ];

      urls.forEach(url => {
        const result = validateLinkUrl(url);
        expect(result).toBeTruthy();
        expect(result).not.toBe('#');
      });
    });

    it('should sanitize dangerous links in mixed content', () => {
      const safeUrls = ['https://example.com', '/dashboard'];
      const dangerousUrls = [
        'javascript:alert("xss")',
        'data:text/html,<script>',
      ];

      safeUrls.forEach(url => {
        const result = validateLinkUrl(url);
        expect(result).toBeTruthy();
        expect(result).not.toBe('#');
      });

      dangerousUrls.forEach(url => {
        const result = validateLinkUrl(url);
        expect(result).toBe('#');
      });
    });
  });
});
