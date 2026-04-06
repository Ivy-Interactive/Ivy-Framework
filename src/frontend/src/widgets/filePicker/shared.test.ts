import { describe, it, expect, vi, beforeEach } from "vitest";
import { uploadFileWithProgress } from "./shared";

// Mock XMLHttpRequest
class MockXHR {
  static instances: MockXHR[] = [];

  upload = {
    onprogress: null as ((e: ProgressEvent) => void) | null,
  };
  onload: (() => void) | null = null;
  onerror: (() => void) | null = null;
  onabort: (() => void) | null = null;
  status = 200;
  statusText = "OK";
  readyState = 0;

  openMethod = "";
  openUrl = "";
  sentData: FormData | null = null;

  open(method: string, url: string) {
    this.openMethod = method;
    this.openUrl = url;
  }

  send(data: FormData) {
    this.sentData = data;
  }

  abort() {
    this.onabort?.();
  }

  constructor() {
    MockXHR.instances.push(this);
  }

  // Helper to simulate progress events
  simulateProgress(loaded: number, total: number) {
    this.upload.onprogress?.(
      new ProgressEvent("progress", {
        lengthComputable: true,
        loaded,
        total,
      }),
    );
  }

  // Helper to simulate successful load
  simulateSuccess(status = 200) {
    this.status = status;
    this.statusText = "OK";
    this.onload?.();
  }

  // Helper to simulate failure
  simulateError() {
    this.onerror?.();
  }
}

beforeEach(() => {
  MockXHR.instances = [];
  vi.stubGlobal("XMLHttpRequest", MockXHR);

  // Mock document.querySelector for getFullUrl
  vi.spyOn(document, "querySelector").mockReturnValue(null);
});

describe("uploadFileWithProgress", () => {
  it("should send the file via XMLHttpRequest POST", () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    uploadFileWithProgress("/api/upload", file);

    const xhr = MockXHR.instances[0];
    expect(xhr.openMethod).toBe("POST");
    expect(xhr.openUrl).toBe("/api/upload");
    expect(xhr.sentData).toBeInstanceOf(FormData);
  });

  it("should call onProgress with progress ratio", () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });
    const onProgress = vi.fn();

    uploadFileWithProgress("/api/upload", file, onProgress);

    const xhr = MockXHR.instances[0];
    xhr.simulateProgress(50, 100);

    expect(onProgress).toHaveBeenCalledWith(0.5);
  });

  it("should call onProgress multiple times as upload progresses", () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });
    const onProgress = vi.fn();

    uploadFileWithProgress("/api/upload", file, onProgress);

    const xhr = MockXHR.instances[0];
    xhr.simulateProgress(25, 100);
    xhr.simulateProgress(75, 100);
    xhr.simulateProgress(100, 100);

    expect(onProgress).toHaveBeenCalledTimes(3);
    expect(onProgress).toHaveBeenNthCalledWith(1, 0.25);
    expect(onProgress).toHaveBeenNthCalledWith(2, 0.75);
    expect(onProgress).toHaveBeenNthCalledWith(3, 1);
  });

  it("should resolve promise on successful upload", async () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    const { promise } = uploadFileWithProgress("/api/upload", file);

    const xhr = MockXHR.instances[0];
    xhr.simulateSuccess(200);

    await expect(promise).resolves.toBeUndefined();
  });

  it("should reject promise on HTTP error status", async () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    const { promise } = uploadFileWithProgress("/api/upload", file);

    const xhr = MockXHR.instances[0];
    xhr.status = 500;
    xhr.statusText = "Internal Server Error";
    xhr.onload?.();

    await expect(promise).rejects.toThrow("Upload failed: Internal Server Error");
  });

  it("should reject promise on network error", async () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    const { promise } = uploadFileWithProgress("/api/upload", file);

    const xhr = MockXHR.instances[0];
    xhr.simulateError();

    await expect(promise).rejects.toThrow("Upload failed");
  });

  it("should abort the upload when abort() is called", async () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    const { promise, abort } = uploadFileWithProgress("/api/upload", file);

    abort();

    await expect(promise).rejects.toThrow("Upload aborted");
  });

  it("should use getFullUrl to resolve the upload URL", () => {
    const mockMeta = document.createElement("meta");
    mockMeta.setAttribute("name", "ivy-host");
    mockMeta.setAttribute("content", "https://myhost.com");
    vi.spyOn(document, "querySelector").mockReturnValue(mockMeta);

    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    uploadFileWithProgress("/api/upload", file);

    const xhr = MockXHR.instances[0];
    expect(xhr.openUrl).toBe("https://myhost.com/api/upload");
  });

  it("should work without onProgress callback", async () => {
    const file = new File(["test content"], "test.txt", { type: "text/plain" });

    const { promise } = uploadFileWithProgress("/api/upload", file);

    const xhr = MockXHR.instances[0];
    // Progress event should not throw even without callback
    xhr.simulateProgress(50, 100);
    xhr.simulateSuccess();

    await expect(promise).resolves.toBeUndefined();
  });
});
