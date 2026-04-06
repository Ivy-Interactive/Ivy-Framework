// Feature detection for File System Access API
export const hasFileSystemAccess = typeof window !== "undefined" && "showOpenFilePicker" in window;

export const hasSaveFilePicker = typeof window !== "undefined" && "showSaveFilePicker" in window;

export const hasDirectoryPicker = typeof window !== "undefined" && "showDirectoryPicker" in window;

export type PickDirectoryResult =
  | { kind: "selected"; name: string }
  | { kind: "cancelled" }
  | { kind: "error"; error: unknown };

/**
 * Opens the native directory picker (modern API) and returns the selected directory name.
 * Returns a discriminated union so callers can handle selection, cancellation, and errors uniformly.
 */
export async function pickDirectory(): Promise<PickDirectoryResult> {
  try {
    const dirHandle = await showDirectoryPicker();
    return { kind: "selected", name: dirHandle.name };
  } catch (err: unknown) {
    if (err instanceof DOMException && err.name === "AbortError") {
      return { kind: "cancelled" };
    }
    return { kind: "error", error: err };
  }
}
