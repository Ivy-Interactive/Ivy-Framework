import { describe, it, expect } from 'vitest';

/**
 * Tests for markdown table cell escaping in DocumentTools.
 *
 * This test file verifies that the escaping logic in DocumentTools correctly
 * escapes backslashes and pipes to prevent injection attacks in markdown tables.
 * The escaping logic is inline in the extractTableWithAssociatedContent function.
 */
describe('DocumentTools markdown table cell escaping', () => {
  /**
   * Helper function that replicates the escaping logic from DocumentTools.tsx
   * This allows us to test the escaping behavior in isolation.
   */
  function escapeMarkdownTableCell(text: string): string {
    // Escape backslashes first, then pipes (for markdown table cells) to prevent injection attacks
    const escaped = text.replace(/\\/g, '\\\\');
    return escaped.replace(/\|/g, '\\|');
  }

  describe('basic escaping', () => {
    it('should escape pipe characters', () => {
      expect(escapeMarkdownTableCell('test|value')).toBe('test\\|value');
      expect(escapeMarkdownTableCell('|pipe|')).toBe('\\|pipe\\|');
    });

    it('should escape backslashes', () => {
      expect(escapeMarkdownTableCell('test\\value')).toBe('test\\\\value');
      expect(escapeMarkdownTableCell('\\backslash\\')).toBe(
        '\\\\backslash\\\\'
      );
    });

    it('should escape both backslashes and pipes', () => {
      expect(escapeMarkdownTableCell('test\\|value')).toBe('test\\\\\\|value');
      expect(escapeMarkdownTableCell('\\|pipe\\|')).toBe('\\\\\\|pipe\\\\\\|');
    });
  });

  describe('security - preventing injection', () => {
    it('should escape backslashes before pipes to prevent double-escaping issues', () => {
      // If we escape pipes first, then backslashes, we'd get wrong results
      // Input: \| should become \\| (backslash escaped first, then pipe)
      const input = '\\|';
      const result = escapeMarkdownTableCell(input);
      // Should be: \\ (escaped backslash) + \| (escaped pipe) = \\\|
      expect(result).toBe('\\\\\\|');
    });

    it('should handle multiple backslashes correctly', () => {
      expect(escapeMarkdownTableCell('\\\\')).toBe('\\\\\\\\');
      expect(escapeMarkdownTableCell('test\\\\value')).toBe(
        'test\\\\\\\\value'
      );
    });

    it('should handle multiple pipes correctly', () => {
      expect(escapeMarkdownTableCell('|||')).toBe('\\|\\|\\|');
      expect(escapeMarkdownTableCell('test|||value')).toBe(
        'test\\|\\|\\|value'
      );
    });

    it('should handle mixed special characters', () => {
      expect(escapeMarkdownTableCell('\\|\\|\\')).toBe('\\\\\\|\\\\\\|\\\\');
      expect(escapeMarkdownTableCell('test\\|value|data')).toBe(
        'test\\\\\\|value\\|data'
      );
    });
  });

  describe('normal text', () => {
    it('should not modify text without special characters', () => {
      expect(escapeMarkdownTableCell('normal text')).toBe('normal text');
      expect(escapeMarkdownTableCell('123')).toBe('123');
      expect(escapeMarkdownTableCell('test-value')).toBe('test-value');
    });

    it('should handle empty strings', () => {
      expect(escapeMarkdownTableCell('')).toBe('');
    });

    it('should handle whitespace', () => {
      expect(escapeMarkdownTableCell('  test  ')).toBe('  test  ');
      expect(escapeMarkdownTableCell('test\nvalue')).toBe('test\nvalue');
    });
  });

  describe('edge cases', () => {
    it('should handle text with only backslashes', () => {
      expect(escapeMarkdownTableCell('\\')).toBe('\\\\');
      expect(escapeMarkdownTableCell('\\\\\\')).toBe('\\\\\\\\\\\\');
    });

    it('should handle text with only pipes', () => {
      expect(escapeMarkdownTableCell('|')).toBe('\\|');
      expect(escapeMarkdownTableCell('|||')).toBe('\\|\\|\\|');
    });

    it('should handle complex markdown-like content', () => {
      const input = 'Column|Header\\Value';
      const result = escapeMarkdownTableCell(input);
      expect(result).toBe('Column\\|Header\\\\Value');
      // Verify all pipes are escaped
      expect(result.split('\\|').length).toBeGreaterThan(1); // All pipes should be escaped
      // Verify all backslashes are part of escape sequences
      // In the result, backslashes should only appear as \\ or \|
      const backslashCount = (result.match(/\\\\/g) || []).length;
      const pipeEscapeCount = (result.match(/\\\|/g) || []).length;
      // Total backslashes in result should equal 2 * (backslashCount + pipeEscapeCount)
      // This ensures all backslashes are escaped
      expect(result.match(/\\/g)?.length).toBe(
        2 * backslashCount + pipeEscapeCount
      );
    });
  });
});
