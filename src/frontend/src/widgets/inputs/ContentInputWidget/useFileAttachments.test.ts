import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for useFileAttachments hook.
 *
 * Since @testing-library/react is not available in this project,
 * we verify the source code contains the correct patterns for:
 * - Paste event handling with guard conditions
 * - Drop event handling with guard conditions
 * - Drag state management
 * - File picker interaction
 * - Upload pipeline with validation
 */
describe("useFileAttachments", () => {
  const source = fs.readFileSync(path.resolve(__dirname, "./useFileAttachments.ts"), "utf-8");

  describe("imports and dependencies", () => {
    it("should import useUploadWithProgress from shared", () => {
      expect(source).toContain(
        'import { useUploadWithProgress } from "../shared/useUploadWithProgress"',
      );
    });

    it("should import validateFileWithToast and validateFileCount from file-input-validation", () => {
      expect(source).toContain(
        'import { validateFileWithToast, validateFileCount } from "../file-input-validation"',
      );
    });

    it("should not import uploadFileWithProgress directly", () => {
      expect(source).not.toContain("import { uploadFileWithProgress }");
    });
  });

  describe("handlePaste guards", () => {
    it("should access e.clipboardData.items", () => {
      expect(source).toContain("e.clipboardData");
    });

    it("should check kind === 'file' on clipboard items", () => {
      expect(source).toContain('kind === "file"');
    });

    it("should call e.preventDefault() when files are found", () => {
      const handlePasteIndex = source.indexOf("const handlePaste");
      expect(handlePasteIndex).toBeGreaterThan(-1);

      const handlePasteBlock = source.slice(
        handlePasteIndex,
        source.indexOf("\n  const", handlePasteIndex + 1),
      );
      expect(handlePasteBlock).toContain("e.preventDefault()");
    });

    it("should show toast when uploadUrl is missing for file paste", () => {
      const handlePasteIndex = source.indexOf("const handlePaste");
      expect(handlePasteIndex).toBeGreaterThan(-1);

      const handlePasteBlock = source.slice(
        handlePasteIndex,
        source.indexOf("\n  const", handlePasteIndex + 1),
      );
      expect(handlePasteBlock).toContain("!uploadUrl");
      expect(handlePasteBlock).toContain("toast(");
      expect(handlePasteBlock).toContain('"Upload not available"');
    });
  });

  describe("handleDrop guards", () => {
    it("should contain e.preventDefault() and e.stopPropagation()", () => {
      const handleDropIndex = source.indexOf("const handleDrop");
      expect(handleDropIndex).toBeGreaterThan(-1);

      const handleDropBlock = source.slice(
        handleDropIndex,
        source.indexOf("\n  const", handleDropIndex + 1),
      );
      expect(handleDropBlock).toContain("e.preventDefault()");
      expect(handleDropBlock).toContain("e.stopPropagation()");
    });

    it("should contain setIsDragging(false)", () => {
      const handleDropIndex = source.indexOf("const handleDrop");
      expect(handleDropIndex).toBeGreaterThan(-1);

      const handleDropBlock = source.slice(
        handleDropIndex,
        source.indexOf("\n  const", handleDropIndex + 1),
      );
      expect(handleDropBlock).toContain("setIsDragging(false)");
    });

    it("should contain disabled guard for early return", () => {
      const handleDropIndex = source.indexOf("const handleDrop");
      expect(handleDropIndex).toBeGreaterThan(-1);

      const handleDropBlock = source.slice(
        handleDropIndex,
        source.indexOf("\n  const", handleDropIndex + 1),
      );
      expect(handleDropBlock).toContain("disabled");
    });

    it("should show toast when uploadUrl is missing on drop", () => {
      const handleDropIndex = source.indexOf("const handleDrop");
      expect(handleDropIndex).toBeGreaterThan(-1);

      const handleDropBlock = source.slice(
        handleDropIndex,
        source.indexOf("\n  const", handleDropIndex + 1),
      );
      expect(handleDropBlock).toContain("!uploadUrl");
      expect(handleDropBlock).toContain("toast(");
      expect(handleDropBlock).toContain('"Upload not available"');
    });

    it("should include uploadUrl in handleDrop dependency array", () => {
      const handleDropIndex = source.indexOf("const handleDrop");
      expect(handleDropIndex).toBeGreaterThan(-1);

      const handleDropBlock = source.slice(
        handleDropIndex,
        source.indexOf("\n  const", handleDropIndex + 1),
      );
      expect(handleDropBlock).toContain("[disabled, uploadUrl, uploadFiles],");
    });

    it("should access e.dataTransfer.files", () => {
      expect(source).toContain("e.dataTransfer.files");
    });
  });

  describe("drag handlers", () => {
    it("onDragEnter should check !disabled before setIsDragging(true)", () => {
      const dragEnterIndex = source.indexOf("const handleDragEnter");
      expect(dragEnterIndex).toBeGreaterThan(-1);

      const dragEnterBlock = source.slice(
        dragEnterIndex,
        source.indexOf("\n  const", dragEnterIndex + 1),
      );
      expect(dragEnterBlock).toContain("!disabled");
      expect(dragEnterBlock).toContain("setIsDragging(true)");
    });

    it("onDragOver should check !disabled", () => {
      const dragOverIndex = source.indexOf("const handleDragOver");
      expect(dragOverIndex).toBeGreaterThan(-1);

      const dragOverBlock = source.slice(
        dragOverIndex,
        source.indexOf("\n  const", dragOverIndex + 1),
      );
      expect(dragOverBlock).toContain("!disabled");
    });

    it("onDragLeave should set isDragging to false", () => {
      const dragLeaveIndex = source.indexOf("const handleDragLeave");
      expect(dragLeaveIndex).toBeGreaterThan(-1);

      const dragLeaveBlock = source.slice(
        dragLeaveIndex,
        source.indexOf("\n  const", dragLeaveIndex + 1),
      );
      expect(dragLeaveBlock).toContain("setIsDragging(false)");
    });
  });

  describe("handleFileInputChange guards", () => {
    it("should show toast when uploadUrl is missing for file input", () => {
      const handleFileInputChangeIndex = source.indexOf("const handleFileInputChange");
      expect(handleFileInputChangeIndex).toBeGreaterThan(-1);

      const handleFileInputChangeBlock = source.slice(
        handleFileInputChangeIndex,
        source.indexOf("\n  const", handleFileInputChangeIndex + 1),
      );
      expect(handleFileInputChangeBlock).toContain("!uploadUrl");
      expect(handleFileInputChangeBlock).toContain("toast(");
      expect(handleFileInputChangeBlock).toContain('"Upload not available"');
    });

    it("should not call uploadFiles when uploadUrl is missing", () => {
      const handleFileInputChangeIndex = source.indexOf("const handleFileInputChange");
      expect(handleFileInputChangeIndex).toBeGreaterThan(-1);

      const handleFileInputChangeBlock = source.slice(
        handleFileInputChangeIndex,
        source.indexOf("\n  const", handleFileInputChangeIndex + 1),
      );
      // The !uploadUrl check should appear before uploadFiles call, with a return
      const uploadUrlCheckIndex = handleFileInputChangeBlock.indexOf("!uploadUrl");
      const returnIndex = handleFileInputChangeBlock.indexOf("return;", uploadUrlCheckIndex);
      const uploadFilesIndex = handleFileInputChangeBlock.indexOf("await uploadFiles");
      expect(uploadUrlCheckIndex).toBeGreaterThan(-1);
      expect(returnIndex).toBeGreaterThan(-1);
      expect(returnIndex).toBeLessThan(uploadFilesIndex);
    });

    it("should reset e.target.value even when uploadUrl is missing", () => {
      const handleFileInputChangeIndex = source.indexOf("const handleFileInputChange");
      expect(handleFileInputChangeIndex).toBeGreaterThan(-1);

      const handleFileInputChangeBlock = source.slice(
        handleFileInputChangeIndex,
        source.indexOf("\n  const", handleFileInputChangeIndex + 1),
      );
      // There should be two e.target.value = "" assignments (one in guard, one after upload)
      const matches = handleFileInputChangeBlock.match(/e\.target\.value\s*=\s*""/g);
      expect(matches).not.toBeNull();
      expect(matches!.length).toBe(2);
    });

    it("should include uploadUrl in handleFileInputChange dependency array", () => {
      const handleFileInputChangeIndex = source.indexOf("const handleFileInputChange");
      expect(handleFileInputChangeIndex).toBeGreaterThan(-1);

      const handleFileInputChangeBlock = source.slice(
        handleFileInputChangeIndex,
        source.indexOf("\n  const", handleFileInputChangeIndex + 1),
      );
      expect(handleFileInputChangeBlock).toContain("[uploadUrl, uploadFiles],");
    });
  });

  describe("openFilePicker guard", () => {
    it("should contain !disabled && fileInputRef.current guard", () => {
      expect(source).toContain("!disabled && fileInputRef.current");
    });
  });

  describe("upload pipeline", () => {
    it("uploadFiles should call validateFileCount before processing", () => {
      const uploadFilesIndex = source.indexOf("const uploadFiles");
      expect(uploadFilesIndex).toBeGreaterThan(-1);

      const uploadFilesBlock = source.slice(
        uploadFilesIndex,
        source.indexOf("\n  const", uploadFilesIndex + 1),
      );
      expect(uploadFilesBlock).toContain("validateFileCount");
    });

    it("handleUploadFile should contain !uploadUrl guard for early return", () => {
      const handleUploadIndex = source.indexOf("const handleUploadFile");
      expect(handleUploadIndex).toBeGreaterThan(-1);

      const handleUploadBlock = source.slice(
        handleUploadIndex,
        source.indexOf("\n  const", handleUploadIndex + 1),
      );
      expect(handleUploadBlock).toContain("!uploadUrl");
    });

    it("handleUploadFile should call validateFileWithToast", () => {
      const handleUploadIndex = source.indexOf("const handleUploadFile");
      expect(handleUploadIndex).toBeGreaterThan(-1);

      const handleUploadBlock = source.slice(
        handleUploadIndex,
        source.indexOf("\n  const", handleUploadIndex + 1),
      );
      expect(handleUploadBlock).toContain("validateFileWithToast");
    });

    it("handleUploadFile should call uploadSingleFile", () => {
      const handleUploadIndex = source.indexOf("const handleUploadFile");
      expect(handleUploadIndex).toBeGreaterThan(-1);

      const handleUploadBlock = source.slice(
        handleUploadIndex,
        source.indexOf("\n  const", handleUploadIndex + 1),
      );
      expect(handleUploadBlock).toContain("uploadSingleFile");
    });
  });

  describe("return shape", () => {
    it("should return isDragging", () => {
      expect(source).toContain("isDragging,");
    });

    it("should return dragHandlers", () => {
      expect(source).toContain("dragHandlers,");
    });

    it("should return handlePaste", () => {
      expect(source).toContain("handlePaste,");
    });

    it("should return openFilePicker", () => {
      expect(source).toContain("openFilePicker,");
    });

    it("should return handleFileInputChange", () => {
      expect(source).toContain("handleFileInputChange,");
    });

    it("should return fileInputRef", () => {
      expect(source).toContain("fileInputRef,");
    });

    it("should return uploadProgress", () => {
      expect(source).toContain("uploadProgress,");
    });

    it("should return cancelUpload", () => {
      expect(source).toContain("cancelUpload,");
    });
  });
});
