import { describe, it, expect, vi, beforeEach } from "vitest";
import { resolveColor } from "./SignatureInputWidget";

vi.mock("@/lib/theme", () => ({
  getCSSVariable: vi.fn((variable: string) => {
    const vars: Record<string, string> = {
      "--red": "#ef4444",
      "--blue": "#3b82f6",
      "--foreground": "#000000",
    };
    return vars[variable] ?? "";
  }),
}));

describe("resolveColor", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("returns fallback when color is undefined", () => {
    expect(resolveColor(undefined, "#000000")).toBe("#000000");
  });

  it("returns fallback when color is empty string", () => {
    expect(resolveColor("", "#000000")).toBe("#000000");
  });

  it("passes through hex colors unchanged", () => {
    expect(resolveColor("#ff0000", "#000000")).toBe("#ff0000");
    expect(resolveColor("#abc", "#000000")).toBe("#abc");
  });

  it("passes through rgb colors unchanged", () => {
    expect(resolveColor("rgb(255, 0, 0)", "#000000")).toBe("rgb(255, 0, 0)");
    expect(resolveColor("rgba(255, 0, 0, 0.5)", "#000000")).toBe("rgba(255, 0, 0, 0.5)");
  });

  it("resolves named colors via CSS variables", () => {
    expect(resolveColor("red", "#000000")).toBe("#ef4444");
    expect(resolveColor("blue", "#000000")).toBe("#3b82f6");
  });

  it("resolves named colors case-insensitively", () => {
    expect(resolveColor("Red", "#000000")).toBe("#ef4444");
    expect(resolveColor("BLUE", "#000000")).toBe("#3b82f6");
  });

  it("returns fallback for unknown named colors", () => {
    expect(resolveColor("nonexistent", "#123456")).toBe("#123456");
  });
});
