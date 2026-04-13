import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useDictation hook.
 *
 * Since @testing-library/react is not available in this project,
 * we verify the source code contains the correct patterns for:
 * - Imports (uploadFile, logger)
 * - State management (isRecording)
 * - Refs (mediaRecorder, stream, chunks)
 * - MIME type detection
 * - Guard clauses and error handling
 */
describe("useDictation", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useDictation.ts"), "utf-8");

  it("should import uploadFile from filePicker shared and logger from lib/logger", () => {
    expect(hookSource).toContain('import { uploadFile } from "@/widgets/filePicker/shared"');
    expect(hookSource).toContain('import { logger } from "@/lib/logger"');
  });

  it("should manage isRecording state with useState(false)", () => {
    expect(hookSource).toContain("useState(false)");
  });

  it("should use refs for mediaRecorder, stream, and chunks", () => {
    expect(hookSource).toContain("useRef<MediaRecorder | null>(null)");
    expect(hookSource).toContain("useRef<MediaStream | null>(null)");
    expect(hookSource).toContain("useRef<Blob[]>([])");
  });

  it("should define supportedMimeTypes array with MediaRecorder.isTypeSupported usage", () => {
    expect(hookSource).toContain("supportedMimeTypes");
    expect(hookSource).toContain("audio/webm;codecs=opus");
    expect(hookSource).toContain("MediaRecorder.isTypeSupported");
  });

  it("should return early from startRecording if dictationUploadUrl is falsy", () => {
    expect(hookSource).toContain("if (!dictationUploadUrl) return");
  });

  it("should stop all tracks and null streamRef on stop", () => {
    expect(hookSource).toContain("getTracks().forEach((track) => track.stop())");
    expect(hookSource).toContain("streamRef.current = null");
  });

  it("should guard MediaRecorder.stop() with state !== inactive check", () => {
    expect(hookSource).toContain('state !== "inactive"');
  });

  it("should guard upload with empty blob check", () => {
    expect(hookSource).toContain("if (blob.size === 0) return");
  });

  it("should call uploadFile with dictationUploadUrl, blob, and dictation options", () => {
    expect(hookSource).toContain("uploadFile(dictationUploadUrl, blob,");
    expect(hookSource).toContain('filename: "dictation.webm"');
    expect(hookSource).toContain("extraFields: { mimeType:");
  });

  it("should log errors for upload failures and microphone access failures", () => {
    expect(hookSource).toContain('logger.error("Dictation upload error:"');
    expect(hookSource).toContain('logger.error("Error accessing microphone for dictation:"');
  });

  it("should reset isRecording to false in the startRecording catch block", () => {
    // Find the catch block in startRecording and verify setIsRecording(false) is there
    const catchIndex = hookSource.indexOf("} catch (err) {");
    expect(catchIndex).toBeGreaterThan(-1);

    const catchBlock = hookSource.slice(catchIndex, hookSource.indexOf("}", catchIndex + 15) + 1);
    expect(catchBlock).toContain("setIsRecording(false)");
  });
});
