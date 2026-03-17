// Feature detection for File System Access API
export const hasFileSystemAccess =
  typeof window !== 'undefined' && 'showOpenFilePicker' in window;

export const hasSaveFilePicker =
  typeof window !== 'undefined' && 'showSaveFilePicker' in window;

export const hasDirectoryPicker =
  typeof window !== 'undefined' && 'showDirectoryPicker' in window;
