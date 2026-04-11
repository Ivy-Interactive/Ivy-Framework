import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for FileInputWidget.
 *
 * Since @testing-library/react is not available in this project,
 * we verify the source code contains the correct patterns for:
 * - Upload file validation with guards
 * - File change handling with count validation
 * - Drop handling with disabled guard and count validation
 * - Drag state management
 */
describe("FileInputWidget", () => {
  const source = fs.readFileSync(path.resolve(__dirname, "./FileInputWidget.tsx"), "utf-8");

  /**
   * Helper: extract a useCallback block starting at `const <name> = useCallback`
   * and ending at the next top-level `const ` declaration (2-space indent).
   */
  function extractBlock(name: string): string {
    const marker = `const ${name} = useCallback`;
    const start = source.indexOf(marker);
    expect(start).toBeGreaterThan(-1);
    const nextConst = source.indexOf("\n  const ", start + 1);
    return source.slice(start, nextConst > start ? nextConst : undefined);
  }

  describe("imports and dependencies", () => {
    it("should import toast from use-toast", () => {
      expect(source).toContain('import { toast } from "@/hooks/use-toast"');
    });

    it("should import validateFileWithToast and validateFileCount", () => {
      expect(source).toContain("import { validateFileWithToast, validateFileCount }");
    });

    it("should import useUploadWithProgress from shared", () => {
      expect(source).toContain(
        'import { useUploadWithProgress } from "./shared/useUploadWithProgress"',
      );
    });

    it("should import useEventHandler", () => {
      expect(source).toContain('import { useEventHandler } from "@/components/event-handler"');
    });

    it("should import useDialogBlurTracking from shared", () => {
      expect(source).toContain(
        'import { useDialogBlurTracking } from "./shared/useDialogBlurTracking"',
      );
    });
  });

  describe("handleUploadFile", () => {
    it("should contain !uploadUrl guard for early return", () => {
      const block = extractBlock("handleUploadFile");
      expect(block).toContain("if (!uploadUrl)");
      expect(block).toContain("toast({");
      expect(block).toContain("return;");
    });

    it("should call validateFileWithToast with file, accept, maxFileSize, minFileSize", () => {
      const block = extractBlock("handleUploadFile");
      expect(block).toContain("validateFileWithToast");
      expect(block).toContain("{ file, accept, maxFileSize, minFileSize }");
    });

    it("should call uploadSingleFile(uploadUrl, file) on success", () => {
      const block = extractBlock("handleUploadFile");
      expect(block).toContain("uploadSingleFile(uploadUrl, file)");
    });
  });

  describe("handleChange", () => {
    it("should contain early return for no files selected", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("!files || files.length === 0");
    });

    it("should compute currentFileCount using Array.isArray(value) check", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("Array.isArray(value) ? value.length : value ? 1 : 0");
    });

    it("should call validateFileCount with currentFileCount, files.length, maxFiles", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("validateFileCount(currentFileCount, files.length, maxFiles)");
    });

    it("should show destructive toast with title from countValidation on invalid count", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("toast(");
      expect(block).toContain('"Too many files"');
      expect(block).toContain('"destructive"');
    });

    it("should reset e.target.value on validation failure", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain('e.target.value = ""');
    });

    it("should support multiple files via Promise.all with Array.from(files).map(handleUploadFile)", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("Promise.all(Array.from(files).map(handleUploadFile))");
    });

    it("should support single file via handleUploadFile(files[0])", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("handleUploadFile(files[0])");
    });

    it("should reset input value after upload", () => {
      const block = extractBlock("handleChange");
      // The second e.target.value = "" is after upload
      const firstReset = block.indexOf('e.target.value = ""');
      const secondReset = block.indexOf('e.target.value = ""', firstReset + 1);
      expect(secondReset).toBeGreaterThan(firstReset);
    });
  });

  describe("handleDrop", () => {
    it("should call e.preventDefault() and e.stopPropagation()", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("e.preventDefault()");
      expect(block).toContain("e.stopPropagation()");
    });

    it("should set isDragging to false", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("setIsDragging(false)");
    });

    it("should contain disabled guard for early return", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("if (disabled) return");
    });

    it("should check files.length === 0 with early return", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("files.length === 0");
    });

    it("should call validateFileCount with currentFileCount calculation", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("validateFileCount(currentFileCount, files.length, maxFiles)");
    });

    it("should show destructive toast on invalid file count", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("toast(");
      expect(block).toContain('"Too many files"');
      expect(block).toContain('"destructive"');
    });

    it("should support multiple and single file upload paths", () => {
      const block = extractBlock("handleDrop");
      expect(block).toContain("files.map(handleUploadFile)");
      expect(block).toContain("handleUploadFile(files[0])");
    });
  });

  describe("handleCancel", () => {
    it("should check uploadProgress.has(fileId) for client-side cancel", () => {
      const block = extractBlock("handleCancel");
      expect(block).toContain("uploadProgress.has(fileId)");
    });

    it("should call cancelClientUpload(fileId) when upload is in progress", () => {
      const block = extractBlock("handleCancel");
      expect(block).toContain("cancelClientUpload(fileId)");
    });

    it("should dispatch OnCancel server event when not a client upload", () => {
      const block = extractBlock("handleCancel");
      expect(block).toContain('handleEvent("OnCancel", id, [fileId])');
    });

    it("should check hasCancelHandler before dispatching server event", () => {
      const block = extractBlock("handleCancel");
      expect(block).toContain("hasCancelHandler");
    });

    it("should reset inputRef.current.value to empty string", () => {
      const block = extractBlock("handleCancel");
      expect(block).toContain('inputRef.current.value = ""');
    });

    it("should include uploadProgress, cancelClientUpload, hasCancelHandler, handleEvent, id in dependency array", () => {
      const block = extractBlock("handleCancel");
      expect(block).toContain(
        "[uploadProgress, cancelClientUpload, hasCancelHandler, handleEvent, id]",
      );
    });
  });

  describe("drag handlers", () => {
    it("handleDragEnter should check !disabled before setIsDragging(true)", () => {
      const block = extractBlock("handleDragEnter");
      expect(block).toContain("!disabled");
      expect(block).toContain("setIsDragging(true)");
    });

    it("handleDragOver should check !disabled before setIsDragging(true)", () => {
      const block = extractBlock("handleDragOver");
      expect(block).toContain("!disabled");
      expect(block).toContain("setIsDragging(true)");
    });

    it("handleDragLeave should call setIsDragging(false)", () => {
      const block = extractBlock("handleDragLeave");
      expect(block).toContain("setIsDragging(false)");
    });
  });

  describe("openFileDialog", () => {
    it("should guard on !disabled && inputRef.current", () => {
      const block = extractBlock("openFileDialog");
      expect(block).toContain("!disabled && inputRef.current");
    });

    it("should call markDialogOpened when hasBlurHandler is true", () => {
      const block = extractBlock("openFileDialog");
      expect(block).toContain("markDialogOpened()");
    });

    it("should call inputRef.current.click()", () => {
      const block = extractBlock("openFileDialog");
      expect(block).toContain("inputRef.current.click()");
    });

    it("should include disabled, hasBlurHandler, and markDialogOpened in dependency array", () => {
      const block = extractBlock("openFileDialog");
      expect(block).toContain("[disabled, hasBlurHandler, markDialogOpened]");
    });
  });

  describe("blur-tracking via useDialogBlurTracking hook", () => {
    it("should use useDialogBlurTracking hook instead of inline useEffect", () => {
      expect(source).toContain("useDialogBlurTracking({");
      // The inline useEffect with handleWindowFocus should no longer exist
      expect(source).not.toContain("const handleWindowFocus = ()");
    });

    it("should pass enabled: hasBlurHandler to the hook", () => {
      expect(source).toContain("enabled: hasBlurHandler");
    });

    it("should pass onBlur callback that fires OnBlur event", () => {
      expect(source).toContain('onBlur: () => handleEvent("OnBlur", id, [])');
    });

    it("should destructure markDialogOpened, markFilesSelected, markBlurFired from hook", () => {
      expect(source).toContain("{ markDialogOpened, markFilesSelected, markBlurFired }");
    });

    it("should call markFilesSelected in handleChange when files are selected", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("markFilesSelected()");
    });

    it("should call markBlurFired in handleChange after upload", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("markBlurFired()");
    });

    it("should include markBlurFired in handleChange dependency array", () => {
      const block = extractBlock("handleChange");
      expect(block).toContain("markBlurFired");
    });
  });
});
