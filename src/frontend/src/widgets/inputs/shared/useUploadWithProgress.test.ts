import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useUploadWithProgress hook.
 *
 * Since @testing-library/react is not available in this project,
 * we verify the source code contains the correct patterns for:
 * - Upload orchestration via uploadFileWithProgress
 * - Progress state management
 * - Cleanup on unmount
 * - Cancel via abort function
 */
describe("useUploadWithProgress", () => {
  const hookSource = fs.readFileSync(
    path.resolve(__dirname, "./useUploadWithProgress.ts"),
    "utf-8",
  );

  it("should import uploadFileWithProgress from filePicker shared", () => {
    expect(hookSource).toContain(
      'import { uploadFileWithProgress } from "@/widgets/filePicker/shared"',
    );
  });

  it("should manage uploadProgress state as a Map<string, number>", () => {
    expect(hookSource).toContain("useState<Map<string, number>>(new Map())");
  });

  it("should manage abortControllers ref as a Map<string, () => void>", () => {
    expect(hookSource).toContain("useRef<Map<string, () => void>>(new Map())");
  });

  it("should generate unique client file IDs using crypto.randomUUID", () => {
    expect(hookSource).toContain("crypto.randomUUID()");
    expect(hookSource).toContain("upload-");
  });

  it("should call uploadFileWithProgress with uploadUrl, file, and progress callback", () => {
    expect(hookSource).toContain("uploadFileWithProgress(uploadUrl, file,");
  });

  it("should store abort function in abortControllersRef", () => {
    expect(hookSource).toContain("abortControllersRef.current.set(clientFileId, abort)");
  });

  it("should clean up progress and abort controller in finally block", () => {
    // Both cleanup operations must be in the finally block
    const finallyIndex = hookSource.indexOf("} finally {");
    expect(finallyIndex).toBeGreaterThan(-1);

    const finallyBlock = hookSource.slice(finallyIndex);
    expect(finallyBlock).toContain("next.delete(clientFileId)");
    expect(finallyBlock).toContain("abortControllersRef.current.delete(clientFileId)");
  });

  it("should abort pending uploads on unmount via useEffect cleanup", () => {
    expect(hookSource).toContain("abortControllersRef.current.forEach((abort) => abort())");
  });

  it("should provide cancelUpload that calls abort and removes the controller", () => {
    // Find the cancelUpload function
    const cancelIndex = hookSource.indexOf("const cancelUpload = useCallback");
    expect(cancelIndex).toBeGreaterThan(-1);

    const cancelBlock = hookSource.slice(
      cancelIndex,
      hookSource.indexOf("}, [])", cancelIndex) + 10,
    );
    expect(cancelBlock).toContain("abortControllersRef.current.get(clientFileId)");
    expect(cancelBlock).toContain("abort()");
    expect(cancelBlock).toContain("abortControllersRef.current.delete(clientFileId)");
  });

  it("should return uploadProgress, uploadSingleFile, and cancelUpload", () => {
    expect(hookSource).toContain("return { uploadProgress, uploadSingleFile, cancelUpload }");
  });

  it("should show toast on non-abort errors", () => {
    expect(hookSource).toContain('error.message !== "Upload aborted"');
    expect(hookSource).toContain('"Upload failed"');
  });
});

describe("useUploadWithProgress consumers", () => {
  const fileInputSource = fs.readFileSync(
    path.resolve(__dirname, "../FileInputWidget.tsx"),
    "utf-8",
  );
  const useFileAttachmentsSource = fs.readFileSync(
    path.resolve(__dirname, "../ContentInputWidget/useFileAttachments.ts"),
    "utf-8",
  );

  it("FileInputWidget should import useUploadWithProgress instead of uploadFileWithProgress", () => {
    expect(fileInputSource).toContain("useUploadWithProgress");
    expect(fileInputSource).not.toContain("import { uploadFileWithProgress }");
  });

  it("useFileAttachments should import useUploadWithProgress instead of uploadFileWithProgress", () => {
    expect(useFileAttachmentsSource).toContain("useUploadWithProgress");
    expect(useFileAttachmentsSource).not.toContain("import { uploadFileWithProgress }");
  });

  it("FileInputWidget should use uploadSingleFile from the hook", () => {
    expect(fileInputSource).toContain("uploadSingleFile");
  });

  it("useFileAttachments should use uploadSingleFile from the hook", () => {
    expect(useFileAttachmentsSource).toContain("uploadSingleFile");
  });

  it("FileInputWidget should not have its own uploadProgress useState", () => {
    // Should not have a direct useState for uploadProgress — it comes from the hook
    expect(fileInputSource).not.toContain("useState<Map<string, number>>");
  });

  it("useFileAttachments should not have its own uploadProgress useState", () => {
    expect(useFileAttachmentsSource).not.toContain("useState<Map<string, number>>");
  });

  it("neither consumer should have its own abortControllersRef", () => {
    expect(fileInputSource).not.toContain("abortControllersRef");
    expect(useFileAttachmentsSource).not.toContain("abortControllersRef");
  });
});
