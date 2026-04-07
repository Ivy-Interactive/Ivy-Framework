// Feature detection for File System Access API
export const hasFileSystemAccess = typeof window !== "undefined" && "showOpenFilePicker" in window;

export const hasSaveFilePicker = typeof window !== "undefined" && "showSaveFilePicker" in window;

export const hasDirectoryPicker = typeof window !== "undefined" && "showDirectoryPicker" in window;

export type PickDirectoryResult =
  | { kind: "selected"; name: string; path?: string }
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

export async function pickDirectoryFullPath(): Promise<PickDirectoryResult> {
  if (typeof window !== "undefined" && (window as any).__ivy_desktop?.showDirectoryPicker) {
    try {
      const result = await (window as any).__ivy_desktop.showDirectoryPicker();
      if (result) {
        return { kind: "selected", name: result.name, path: result.path };
      }
      return { kind: "cancelled" };
    } catch (err) {
      return { kind: "error", error: err };
    }
  }
  return pickDirectory();
}
