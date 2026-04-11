import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Source-analysis tests for the useDialogBlurTracking custom hook.
 *
 * Verifies the hook exports, ref management, event listener setup/cleanup,
 * and microtask usage by inspecting the source code.
 */
describe("useDialogBlurTracking", () => {
  const source = fs.readFileSync(path.resolve(__dirname, "./useDialogBlurTracking.ts"), "utf-8");

  it("should export useDialogBlurTracking function", () => {
    expect(source).toContain("export function useDialogBlurTracking");
  });

  it("should contain useRef calls for the three tracking refs", () => {
    expect(source).toContain("const dialogWasOpenRef = useRef(false)");
    expect(source).toContain("const filesSelectedInCurrentDialogRef = useRef(false)");
    expect(source).toContain("const blurFiredRef = useRef(false)");
  });

  it("should register window.addEventListener('focus', ...) when enabled", () => {
    expect(source).toContain('window.addEventListener("focus", handleWindowFocus)');
  });

  it("should remove event listener on cleanup (removeEventListener)", () => {
    expect(source).toContain('window.removeEventListener("focus", handleWindowFocus)');
  });

  it("should use queueMicrotask for timing coordination", () => {
    expect(source).toContain("queueMicrotask(");
  });

  it("should return markDialogOpened, markFilesSelected, markBlurFired", () => {
    expect(source).toContain("return { markDialogOpened, markFilesSelected, markBlurFired }");
  });

  it("should early return when not enabled", () => {
    expect(source).toContain("if (!enabled) return");
  });

  it("should check dialogWasOpenRef before handling focus", () => {
    expect(source).toContain("if (dialogWasOpenRef.current)");
  });

  it("should reset dialogWasOpenRef on focus", () => {
    expect(source).toContain("dialogWasOpenRef.current = false");
  });

  it("should check filesSelectedInCurrentDialogRef and blurFiredRef in microtask", () => {
    expect(source).toContain("!filesSelectedInCurrentDialogRef.current && !blurFiredRef.current");
  });

  it("should set blurFiredRef when firing blur", () => {
    expect(source).toContain("blurFiredRef.current = true");
  });

  it("should call onBlur callback", () => {
    expect(source).toContain("onBlur()");
  });

  describe("markDialogOpened", () => {
    it("should set dialogWasOpenRef to true", () => {
      expect(source).toContain("dialogWasOpenRef.current = true");
    });

    it("should reset filesSelectedInCurrentDialogRef to false", () => {
      const markDialogOpenedStart = source.indexOf("const markDialogOpened");
      const markDialogOpenedEnd = source.indexOf("};", markDialogOpenedStart);
      const block = source.slice(markDialogOpenedStart, markDialogOpenedEnd);
      expect(block).toContain("filesSelectedInCurrentDialogRef.current = false");
    });

    it("should reset blurFiredRef to false", () => {
      const markDialogOpenedStart = source.indexOf("const markDialogOpened");
      const markDialogOpenedEnd = source.indexOf("};", markDialogOpenedStart);
      const block = source.slice(markDialogOpenedStart, markDialogOpenedEnd);
      expect(block).toContain("blurFiredRef.current = false");
    });
  });

  describe("markFilesSelected", () => {
    it("should set filesSelectedInCurrentDialogRef to true", () => {
      const markFilesSelectedStart = source.indexOf("const markFilesSelected");
      const markFilesSelectedEnd = source.indexOf("};", markFilesSelectedStart);
      const block = source.slice(markFilesSelectedStart, markFilesSelectedEnd);
      expect(block).toContain("filesSelectedInCurrentDialogRef.current = true");
    });
  });

  describe("markBlurFired", () => {
    it("should guard against double-firing with blurFiredRef check", () => {
      const markBlurFiredStart = source.indexOf("const markBlurFired");
      const markBlurFiredEnd = source.indexOf("};", markBlurFiredStart);
      const block = source.slice(markBlurFiredStart, markBlurFiredEnd);
      expect(block).toContain("if (!blurFiredRef.current)");
    });

    it("should set blurFiredRef and call onBlur", () => {
      const markBlurFiredStart = source.indexOf("const markBlurFired");
      const markBlurFiredEnd = source.indexOf("};", markBlurFiredStart);
      const block = source.slice(markBlurFiredStart, markBlurFiredEnd);
      expect(block).toContain("blurFiredRef.current = true");
      expect(block).toContain("onBlur()");
    });
  });
});
