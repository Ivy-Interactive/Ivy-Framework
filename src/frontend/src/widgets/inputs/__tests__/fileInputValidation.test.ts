import { describe, it, expect } from "vitest";
import { validateSingleFile, validateFileCount } from "../file-input-validation";
import { formatBytes } from "@/lib/formatters";

// ---------------------------------------------------------------------------
// formatBytes
// ---------------------------------------------------------------------------

describe("formatBytes", () => {
  it("formats 0 bytes", () => {
    expect(formatBytes(0)).toBe("0 B");
  });

  it("formats 1024 bytes as KB", () => {
    expect(formatBytes(1024)).toBe("1.00 KB");
  });

  it("formats 1048576 bytes as MB", () => {
    expect(formatBytes(1048576)).toBe("1.00 MB");
  });

  it("formats 1536 bytes as 1.50 KB", () => {
    expect(formatBytes(1536)).toBe("1.50 KB");
  });

  it("formats large values in GB", () => {
    expect(formatBytes(1073741824)).toBe("1.00 GB");
  });

  it("drops decimals for values >= 10", () => {
    // 10240 bytes = 10 KB, should show no decimals
    expect(formatBytes(10240)).toBe("10 KB");
  });

  it("formats negative bytes as 0 B", () => {
    expect(formatBytes(-1024)).toBe("0 B");
  });

  it("formats negative fractional bytes as 0 B", () => {
    expect(formatBytes(-0.5)).toBe("0 B");
  });

  it("formats NaN as 0 B", () => {
    expect(formatBytes(NaN)).toBe("0 B");
  });

  it("formats Infinity as 0 B", () => {
    expect(formatBytes(Infinity)).toBe("0 B");
  });

  it("formats -Infinity as 0 B", () => {
    expect(formatBytes(-Infinity)).toBe("0 B");
  });
});

// ---------------------------------------------------------------------------
// validateSingleFile
// ---------------------------------------------------------------------------

describe("validateSingleFile", () => {
  it("returns valid for file with no constraints", () => {
    const file = new File(["content"], "test.txt", { type: "text/plain" });
    expect(validateSingleFile({ file })).toEqual({ valid: true });
  });

  it("rejects wrong MIME type with wildcard accept", () => {
    const file = new File(["content"], "test.txt", { type: "text/plain" });
    const result = validateSingleFile({ file, accept: "image/*" });
    expect(result.valid).toBe(false);
    expect(result.title).toBe("Invalid file type");
  });

  it("accepts correct MIME wildcard", () => {
    const file = new File(["x"], "photo.png", { type: "image/png" });
    const result = validateSingleFile({ file, accept: "image/*" });
    expect(result.valid).toBe(true);
  });

  it("rejects wrong extension with extension-based accept", () => {
    const file = new File(["x"], "test.gif", { type: "image/gif" });
    const result = validateSingleFile({ file, accept: ".png,.jpg" });
    expect(result.valid).toBe(false);
  });

  it("accepts correct extension", () => {
    const file = new File(["x"], "photo.png", { type: "image/png" });
    const result = validateSingleFile({ file, accept: ".png,.jpg" });
    expect(result.valid).toBe(true);
  });

  it("accepts exact MIME type match", () => {
    const file = new File(["x"], "photo.png", { type: "image/png" });
    const result = validateSingleFile({ file, accept: "image/png" });
    expect(result.valid).toBe(true);
  });

  it("rejects file exceeding maxFileSize", () => {
    const content = "x".repeat(2000);
    const file = new File([content], "big.txt", { type: "text/plain" });
    const result = validateSingleFile({ file, maxFileSize: 1000 });
    expect(result.valid).toBe(false);
    expect(result.title).toBe("File too large");
    expect(result.error).toContain("big.txt");
  });

  it("rejects file below minFileSize", () => {
    const file = new File(["x"], "tiny.txt", { type: "text/plain" });
    const result = validateSingleFile({ file, minFileSize: 1000 });
    expect(result.valid).toBe(false);
    expect(result.title).toBe("File too small");
    expect(result.error).toContain("tiny.txt");
  });

  it("accepts file within size bounds", () => {
    const content = "x".repeat(500);
    const file = new File([content], "ok.txt", { type: "text/plain" });
    const result = validateSingleFile({ file, minFileSize: 100, maxFileSize: 1000 });
    expect(result.valid).toBe(true);
  });
});

// ---------------------------------------------------------------------------
// validateFileCount
// ---------------------------------------------------------------------------

describe("validateFileCount", () => {
  it("returns valid when under limit", () => {
    expect(validateFileCount(2, 1, 5)).toEqual({ valid: true });
  });

  it("returns invalid when exceeding limit", () => {
    const result = validateFileCount(3, 3, 5);
    expect(result.valid).toBe(false);
    expect(result.title).toBe("Too many files");
    expect(result.error).toContain("2 more file");
  });

  it("returns invalid when already at limit (remaining=0)", () => {
    const result = validateFileCount(5, 1, 5);
    expect(result.valid).toBe(false);
    expect(result.error).toContain("already reached");
  });

  it("returns valid when maxFiles is undefined", () => {
    expect(validateFileCount(100, 50, undefined)).toEqual({ valid: true });
  });

  it("returns valid when exactly at limit", () => {
    expect(validateFileCount(3, 2, 5)).toEqual({ valid: true });
  });
});
