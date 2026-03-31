import { describe, it, expect } from "vitest";
import { validateLinkUrl, validateMediaUrl } from "@/lib/url";
import { normalizeNestedFences } from "./MarkdownRenderer";

/**
 * Tests for URL validation in markdown links.
 *
 * This test file verifies that the validateLinkUrl function correctly handles
 * URLs that appear in markdown content, ensuring that:
 * 1. Safe URLs (http/https, relative paths, app://, anchors) are validated correctly
 * 2. Dangerous URLs (javascript:, data:, etc.) are sanitized
 * 3. The validation is applied consistently across different markdown link formats
 */
describe("Markdown link URL validation", () => {
  describe("safe markdown link URLs", () => {
    it("should validate absolute http URLs in markdown links", () => {
      const url = "http://example.com/page";
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain("http://example.com");
    });

    it("should validate absolute https URLs in markdown links", () => {
      const url = "https://example.com/page";
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain("https://example.com");
    });

    it("should validate relative paths in markdown links", () => {
      const url = "/dashboard";
      const result = validateLinkUrl(url);
      expect(result).toBe("/dashboard");
    });

    it("should validate app:// URLs in markdown links", () => {
      const url = "app://dashboard";
      const result = validateLinkUrl(url);
      expect(result).toBe("app://dashboard");
    });

    it("should validate anchor links in markdown", () => {
      const url = "#section1";
      const result = validateLinkUrl(url);
      expect(result).toBe("#section1");
    });

    it("should validate URLs with query parameters", () => {
      const url = "https://example.com/page?param=value";
      const result = validateLinkUrl(url);
      expect(result).toBe("https://example.com/page?param=value");
    });

    it("should validate URLs with fragments", () => {
      const url = "https://example.com/page#section";
      const result = validateLinkUrl(url);
      expect(result).toBe("https://example.com/page#section");
    });
  });

  describe("dangerous markdown link URLs (sad path)", () => {
    it("should sanitize javascript: protocol in markdown links", () => {
      const url = 'javascript:alert("xss")';
      const result = validateLinkUrl(url);
      expect(result).toBe("#");
    });

    it("should sanitize data: protocol in markdown links", () => {
      const url = 'data:text/html,<script>alert("xss")</script>';
      const result = validateLinkUrl(url);
      expect(result).toBe("#");
    });

    it("should sanitize file: protocol in markdown links", () => {
      const url = "file:///etc/passwd";
      const result = validateLinkUrl(url);
      expect(result).toBe("#");
    });

    it("should sanitize vbscript: protocol in markdown links", () => {
      const url = 'vbscript:msgbox("xss")';
      const result = validateLinkUrl(url);
      expect(result).toBe("#");
    });

    it("should sanitize javascript: protocol with encoded characters", () => {
      // URL-encoded javascript: becomes a relative path, which is safe
      const url = 'javascript%3Aalert("xss")';
      const result = validateLinkUrl(url);
      // This is treated as a relative path, which is safe
      expect(result).toBeTruthy();
      expect(result).not.toContain("javascript:");
    });

    it("should sanitize mixed case dangerous protocols", () => {
      const urls = ['JAVASCRIPT:alert("xss")', "Data:text/html,<script>", "FILE:///etc/passwd"];
      urls.forEach((url) => {
        const result = validateLinkUrl(url);
        expect(result).toBe("#");
      });
    });

    it("should handle URLs with protocol-like strings in paths", () => {
      // These are actually safe - javascript: in a path is not executed as a protocol
      const urls = [
        'http://example.com/javascript:alert("xss")',
        "https://example.com/data:text/html,<script>",
        '/path/javascript:alert("xss")',
      ];
      urls.forEach((url) => {
        const result = validateLinkUrl(url);
        // These are valid URLs/paths - the "protocol" is part of the path, not the actual protocol
        expect(result).toBeTruthy();
        // The URL parser correctly identifies http/https as the protocol, not javascript:
        if (url.startsWith("http://") || url.startsWith("https://")) {
          expect(result).toContain("http");
        }
      });
    });
  });

  describe("markdown link edge cases (sad path)", () => {
    it("should handle null href in markdown links", () => {
      const result = validateLinkUrl(null);
      expect(result).toBe("#");
    });

    it("should handle undefined href in markdown links", () => {
      const result = validateLinkUrl(undefined);
      expect(result).toBe("#");
    });

    it("should handle empty string href in markdown links", () => {
      const result = validateLinkUrl("");
      expect(result).toBe("#");
    });

    it("should handle whitespace-only href in markdown links", () => {
      const result = validateLinkUrl("   ");
      expect(result).toBe("#");
    });

    it("should handle newlines and tabs in URLs", () => {
      const urls = ["https://example.com\n", "https://example.com\t", "\nhttps://example.com"];
      urls.forEach((url) => {
        const result = validateLinkUrl(url);
        // Should sanitize or trim
        expect(result).not.toContain("\n");
        expect(result).not.toContain("\t");
      });
    });

    it("should handle malformed URLs", () => {
      const malformedUrls = [
        "://malformed",
        "http://",
        "https://",
        "http:///path",
        "not-a-url",
        "invalid://protocol",
      ];
      malformedUrls.forEach((url) => {
        const result = validateLinkUrl(url);
        // Should return safe fallback or sanitized version
        expect(result).toBeTruthy();
        if (result !== "#") {
          // If not rejected, should be a valid relative path or safe URL
          expect(result.startsWith("/") || result.startsWith("http")).toBe(true);
        }
      });
    });

    it("should handle URLs with control characters", () => {
      const url = "https://example.com/\x00\x01\x02";
      const result = validateLinkUrl(url);
      // Should sanitize control characters
      expect(result).toBeTruthy();
    });

    it("should trim whitespace from markdown link URLs", () => {
      const url = "  https://example.com  ";
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain("https://example.com");
      expect(result).not.toContain("  ");
    });

    it("should handle relative paths without leading slash", () => {
      const url = "relative-path";
      const result = validateLinkUrl(url);
      expect(result).toBe("/relative-path");
    });
  });

  describe("markdown reference-style links", () => {
    it("should validate URLs in reference-style markdown links", () => {
      // Reference-style: [Link Text][1] with [1]: https://example.com/page
      const url = "https://example.com/page";
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain("https://example.com");
    });

    it("should validate relative URLs in reference-style links", () => {
      const url = "/dashboard";
      const result = validateLinkUrl(url);
      expect(result).toBe("/dashboard");
    });
  });

  describe("markdown autolink URLs", () => {
    it("should validate autolink http URLs", () => {
      const url = "http://example.com";
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain("http://example.com");
    });

    it("should validate autolink https URLs", () => {
      const url = "https://example.com";
      const result = validateLinkUrl(url);
      expect(result).toBeTruthy();
      expect(result).toContain("https://example.com");
    });
  });

  describe("multiple links in markdown", () => {
    it("should validate each link independently", () => {
      const urls = [
        "https://example.com/page1",
        "/dashboard",
        "https://external.com",
        "app://dashboard",
        "#section1",
      ];

      urls.forEach((url) => {
        const result = validateLinkUrl(url);
        expect(result).toBeTruthy();
        expect(result).not.toBe("#");
      });
    });

    it("should sanitize dangerous links in mixed content", () => {
      const safeUrls = ["https://example.com", "/dashboard"];
      const dangerousUrls = ['javascript:alert("xss")', "data:text/html,<script>"];

      safeUrls.forEach((url) => {
        const result = validateLinkUrl(url);
        expect(result).toBeTruthy();
        expect(result).not.toBe("#");
      });

      dangerousUrls.forEach((url) => {
        const result = validateLinkUrl(url);
        expect(result).toBe("#");
      });
    });
  });
});

describe("Markdown inline code icon detection", () => {
  const iconPattern = /^Icons\.([A-Z][a-zA-Z0-9]*)$/;

  it("should match Icons.ChevronDown pattern", () => {
    const match = "Icons.ChevronDown".match(iconPattern);
    expect(match).not.toBeNull();
    expect(match![1]).toBe("ChevronDown");
  });

  it("should match Icons.Search pattern", () => {
    const match = "Icons.Search".match(iconPattern);
    expect(match).not.toBeNull();
    expect(match![1]).toBe("Search");
  });

  it("should not match NotAnIcon.Foo", () => {
    const match = "NotAnIcon.Foo".match(iconPattern);
    expect(match).toBeNull();
  });

  it("should not match Icons. with no icon name", () => {
    const match = "Icons.".match(iconPattern);
    expect(match).toBeNull();
  });

  it("should not match Icons.lowercase (must start with uppercase)", () => {
    const match = "Icons.chevronDown".match(iconPattern);
    expect(match).toBeNull();
  });

  it("should not match Icon.X (singular, wrong prefix)", () => {
    const match = "Icon.ChevronDown".match(iconPattern);
    expect(match).toBeNull();
  });

  it("should not match code with extra text around Icons.X", () => {
    const match = "use Icons.ChevronDown here".match(iconPattern);
    expect(match).toBeNull();
  });

  it("should match Icons with numbers like Icons.H1", () => {
    const match = "Icons.H1".match(iconPattern);
    expect(match).not.toBeNull();
    expect(match![1]).toBe("H1");
  });
});

describe("normalizeNestedFences", () => {
  it("should pass through content with no code fences unchanged", () => {
    const content = "Hello world\n\nSome paragraph.";
    expect(normalizeNestedFences(content)).toBe(content);
  });

  it("should pass through content with a single code fence unchanged", () => {
    const content = "```csharp\nConsole.WriteLine();\n```";
    expect(normalizeNestedFences(content)).toBe(content);
  });

  it("should normalize nested fences with matching backtick counts", () => {
    const input = ["```markdown", "```csharp", 'Console.WriteLine("hello");', "```", "```"].join(
      "\n",
    );

    const expected = [
      "````markdown",
      "```csharp",
      'Console.WriteLine("hello");',
      "```",
      "````",
    ].join("\n");

    expect(normalizeNestedFences(input)).toBe(expected);
  });

  it("should handle triple nesting by escalating fence lengths", () => {
    const input = [
      "```markdown",
      "Here is an example:",
      "```markdown",
      "```csharp",
      "var x = 1;",
      "```",
      "```",
      "End of example.",
      "```",
    ].join("\n");

    const expected = [
      "`````markdown",
      "Here is an example:",
      "````markdown",
      "```csharp",
      "var x = 1;",
      "```",
      "````",
      "End of example.",
      "`````",
    ].join("\n");

    expect(normalizeNestedFences(input)).toBe(expected);
  });

  it("should preserve language info strings on fences", () => {
    const input = ["```markdown", "```python", "print('hello')", "```", "```"].join("\n");

    const result = normalizeNestedFences(input);
    expect(result).toContain("````markdown");
    expect(result).toContain("```python");
  });

  it("should handle multiple independent nested blocks", () => {
    const input = [
      "```markdown",
      "```js",
      "const x = 1;",
      "```",
      "```",
      "",
      "```markdown",
      "```py",
      "x = 1",
      "```",
      "```",
    ].join("\n");

    const result = normalizeNestedFences(input);
    // Both outer fences should be increased
    const lines = result.split("\n");
    expect(lines[0]).toBe("````markdown");
    expect(lines[4]).toBe("````");
    expect(lines[6]).toBe("````markdown");
    expect(lines[10]).toBe("````");
  });

  it("should handle fences with no info string at top level", () => {
    const content = "```\nplain code\n```";
    expect(normalizeNestedFences(content)).toBe(content);
  });
});

describe("Markdown image URL validation with local files", () => {
  it("should validate file: protocol in markdown images when dangerouslyAllowLocalFiles is enabled", () => {
    const url = "file:///C:/Users/test/image.png";
    const result = validateMediaUrl(url, { mediaType: "image", dangerouslyAllowLocalFiles: true });
    expect(result).toBe("file:///C:/Users/test/image.png");
  });

  it("should handle Windows absolute paths in markdown images when enabled", () => {
    const windowsPaths = ["C:\\Foo\\bar.png", "D:\\Screenshots\\test.jpg", "C:/Foo/bar.png"];
    windowsPaths.forEach((path) => {
      const result = validateMediaUrl(path, {
        mediaType: "image",
        dangerouslyAllowLocalFiles: true,
      });
      expect(result).toBeTruthy();
      expect(result).toContain("file:///");
    });
  });

  it("should reject local files by default for security", () => {
    expect(validateMediaUrl("file:///C:/Users/test/image.png", { mediaType: "image" })).toBeNull();
    expect(validateMediaUrl("C:\\Foo\\bar.png", { mediaType: "image" })).toBeNull();
  });
});
