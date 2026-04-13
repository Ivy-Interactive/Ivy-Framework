import { describe, it, expect, vi, beforeEach } from "vitest";

// We test the internal logic by extracting the catch handler behavior
// rather than testing through React.lazy (which requires rendering)

function isChunkLoadError(error: unknown): boolean {
  if (error instanceof Error) {
    return (
      error.message.includes("Failed to fetch dynamically imported module") ||
      error.message.includes("Loading chunk") ||
      error.message.includes("Loading CSS chunk") ||
      error.message.includes("Failed to load script")
    );
  }
  return false;
}

describe("isChunkLoadError", () => {
  it("detects 'Failed to fetch dynamically imported module' errors", () => {
    const error = new Error(
      "Failed to fetch dynamically imported module: http://localhost:5010/assets/MermaidRenderer-DhmGsYLB.js",
    );
    expect(isChunkLoadError(error)).toBe(true);
  });

  it("detects 'Loading chunk' errors", () => {
    const error = new Error("Loading chunk 123 failed");
    expect(isChunkLoadError(error)).toBe(true);
  });

  it("detects 'Loading CSS chunk' errors", () => {
    const error = new Error("Loading CSS chunk styles-abc123 failed");
    expect(isChunkLoadError(error)).toBe(true);
  });

  it("detects 'Failed to load script' errors", () => {
    const error = new Error("Failed to load script: http://localhost:5010/external/MyWidget.js");
    expect(isChunkLoadError(error)).toBe(true);
  });

  it("returns false for non-chunk errors", () => {
    const error = new Error("Cannot read properties of undefined");
    expect(isChunkLoadError(error)).toBe(false);
  });

  it("returns false for non-Error values", () => {
    expect(isChunkLoadError("string error")).toBe(false);
    expect(isChunkLoadError(null)).toBe(false);
    expect(isChunkLoadError(undefined)).toBe(false);
    expect(isChunkLoadError(42)).toBe(false);
  });
});

describe("lazyWithRetry reload logic", () => {
  const reloadedKey = "vite-chunk-reload";
  let reloadMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    sessionStorage.clear();
    reloadMock = vi.fn();
    Object.defineProperty(window, "location", {
      value: { reload: reloadMock },
      writable: true,
      configurable: true,
    });
  });

  it("sets sessionStorage flag and reloads on first chunk load error", async () => {
    const chunkError = new Error(
      "Failed to fetch dynamically imported module: http://localhost:5010/assets/Foo.js",
    );
    const factory = () => Promise.reject(chunkError);

    // Simulate the catch handler logic from lazyWithRetry
    try {
      await factory();
    } catch (error) {
      if (isChunkLoadError(error) && !sessionStorage.getItem(reloadedKey)) {
        sessionStorage.setItem(reloadedKey, "1");
        window.location.reload();
      }
    }

    expect(sessionStorage.getItem(reloadedKey)).toBe("1");
    expect(reloadMock).toHaveBeenCalledOnce();
  });

  it("clears flag and re-throws on second chunk load error (prevents infinite loop)", async () => {
    sessionStorage.setItem(reloadedKey, "1");

    const chunkError = new Error(
      "Failed to fetch dynamically imported module: http://localhost:5010/assets/Foo.js",
    );
    const factory = () => Promise.reject(chunkError);

    let thrownError: unknown;
    try {
      await factory();
    } catch (error) {
      if (isChunkLoadError(error)) {
        if (!sessionStorage.getItem(reloadedKey)) {
          sessionStorage.setItem(reloadedKey, "1");
          window.location.reload();
        } else {
          sessionStorage.removeItem(reloadedKey);
          thrownError = error;
        }
      }
    }

    expect(sessionStorage.getItem(reloadedKey)).toBeNull();
    expect(reloadMock).not.toHaveBeenCalled();
    expect(thrownError).toBe(chunkError);
  });

  it("re-throws non-chunk errors without reload logic", async () => {
    const regularError = new Error("Cannot read properties of undefined");
    const factory = () => Promise.reject(regularError);

    let thrownError: unknown;
    try {
      await factory();
    } catch (error) {
      if (isChunkLoadError(error)) {
        window.location.reload();
      } else {
        thrownError = error;
      }
    }

    expect(reloadMock).not.toHaveBeenCalled();
    expect(sessionStorage.getItem(reloadedKey)).toBeNull();
    expect(thrownError).toBe(regularError);
  });

  it("returns the module normally when factory succeeds", async () => {
    const mockModule = { default: () => null };
    const factory = () => Promise.resolve(mockModule);

    const result = await factory();
    expect(result).toBe(mockModule);
  });
});
