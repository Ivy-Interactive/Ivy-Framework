# FolderInput Mode Removal (API Change)

## The Change
`FolderInputMode` enum has been removed. `FolderInput` and `FolderInputWidget` no longer expose a `Mode` property. 
The widget now always attempts to operate in "FullPath" mode (which natively falls back to folder names when rendered in browser environments due to File System Access API restrictions, but successfully returns absolute folder paths in desktop contexts).

## Reason
This ensures that users always receive fully qualified paths from directory pickers in supported environments out-of-the-box, without needing to explicitly state they want the full path. The name-only property was a source of confusion and unexpected bugs for file system logic.

## How to Fix and Verify
1. If your codebase explicitly references `FolderInputMode.Name` or `FolderInputMode.FullPath` (e.g. `.ToFolderInput(mode: FolderInputMode.Name)`), remove the `mode` argument.
2. If your codebase expected `FolderInput` to only return the trailing directory *name* even under Desktop environments, you must now handle splitting/extracting the name yourself (e.g., using `Path.GetFileName(folder.Value)` in .NET).
3. Confirm the fix by validating that the selected value from `FolderInput` now returns the absolute file system path when used in a Desktop (Electron/Tauri/Local API) environment, without any explicit configuration.
