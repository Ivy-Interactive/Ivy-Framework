---
searchHints:
  - file-dialog
  - file-picker
  - open-file
  - save-file
  - folder-picker
  - native-dialog
  - usefiledialog
  - usesavedialog
  - usefolderdialog
  - browse
  - upload-dialog
---

# File Dialog Hooks

<Ingress>
Open native OS file, save, and folder picker dialogs from code — without rendering any visible UI. Three hooks follow the UseAlert callback pattern for maximum flexibility.
</Ingress>

## Overview

The file dialog hooks provide programmatic access to the browser's native file picker dialogs:

- **UseFileDialog** — Open-file picker with upload or metadata-only modes
- **UseSaveDialog** — Save-file picker for downloading content to a user-chosen location
- **UseFolderDialog** — Folder picker to browse directory contents

Each hook returns a `(dialogView, showDialog)` tuple following the same pattern as `UseAlert`. The `dialogView` must be placed in the view tree (it renders no visible UI), and the `showDialog` delegate opens the dialog when called, accepting a callback that receives the result.

### Browser Support

All three hooks use **smart browser API detection**:

- **Chromium** (Chrome, Edge, Opera): Full support via File System Access API — true native dialogs with save location, folder browsing, and type filters
- **Firefox/Safari**: Automatic fallback to `<input type="file">` for open/folder, programmatic download for save — functional but slightly less polished UX

## UseFileDialog

### Upload Mode

Opens the native file dialog and uploads the selected file(s) to the server. Accepts an `IUploadHandler`, same as `FileInput`.

```csharp demo-below
public class UseFileDialogUploadDemo : ViewBase
{
    public override object? Build()
    {
        var imageState = UseState<FileUpload<byte[]>?>();

        var (fileDialogView, showFileDialog) = UseFileDialog(
            MemoryStreamUploadHandler.Create(imageState),
            accept: "image/*");

        object? imageView = null;
        if (imageState.Value is { Status: FileUploadStatus.Finished } upload)
        {
            var base64 = Convert.ToBase64String(upload.Content);
            var dataUrl = $"data:{upload.ContentType};base64,{base64}";
            imageView = new Image(dataUrl) { Alt = upload.FileName }
                .Width(Size.Units(80))
                .Height(Size.Units(80));
        }

        return Layout.Vertical()
               | fileDialogView
               | new Button("Pick Image", _ => showFileDialog(files =>
               {
                   // Upload is handled automatically by the handler
               }), icon: Icons.Image)
               | (imageView ?? (object)Text.Muted("No image selected"));
    }
}
```

### PathOnly Mode

Opens the dialog and returns file metadata (name, content type, size) without uploading. Useful when you only need to know what the user selected.

```csharp demo-below
public class UseFileDialogPathOnlyDemo : ViewBase
{
    public override object? Build()
    {
        var selectedFile = UseState<string?>();

        var (fileDialogView, showFileDialog) = UseFileDialog(accept: "image/*");

        return Layout.Vertical()
               | fileDialogView
               | new Button("Select File", _ => showFileDialog(files =>
               {
                   var file = files.FirstOrDefault();
                   if (file != null)
                       selectedFile.Set($"{file.FileName} ({file.ContentType}, {file.Size} bytes)");
               }))
               | Text.P(selectedFile.Value ?? "No file selected");
    }
}
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `handler` | `IUploadHandler` | Upload handler (upload mode only). Use `MemoryStreamUploadHandler.Create(state)` |
| `accept` | `string?` | File type filter (e.g., `"image/*"`, `".pdf,.docx"`) |
| `multiple` | `bool` | Allow selecting multiple files (default: `false`) |
| `maxFileSize` | `long?` | Maximum file size in bytes |
| `minFileSize` | `long?` | Minimum file size in bytes |

## UseSaveDialog

Opens the native save dialog to save generated content to a user-chosen location.

```csharp demo-below
public class UseSaveDialogDemo : ViewBase
{
    public override object? Build()
    {
        var saveResult = UseState<string?>();

        var (saveDialogView, showSaveDialog) = UseSaveDialog(
            contentFactory: async () => System.Text.Encoding.UTF8.GetBytes("Hello, World!"),
            mimeType: "text/plain",
            suggestedName: "hello.txt");

        return Layout.Vertical()
               | saveDialogView
               | new Button("Save File", _ => showSaveDialog(result =>
               {
                   saveResult.Set(result.Success
                       ? $"Saved as {result.FileName}"
                       : "Save cancelled");
               }), icon: Icons.Download)
               | Text.P(saveResult.Value ?? "");
    }
}
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `contentFactory` | `Func<Task<byte[]>>` | Async factory that provides the file content |
| `mimeType` | `string` | MIME type of the file |
| `suggestedName` | `string` | Default file name for the save dialog |

## UseFolderDialog

Opens the native folder picker and returns the list of entries (files and subdirectories) in the selected folder.

```csharp demo-below
public class UseFolderDialogDemo : ViewBase
{
    public override object? Build()
    {
        var entries = UseState<FolderDialogEntry[]?>();

        var (folderDialogView, showFolderDialog) = UseFolderDialog();

        return Layout.Vertical()
               | folderDialogView
               | new Button("Browse Folder", _ => showFolderDialog(selected =>
               {
                   entries.Set(selected);
               }), icon: Icons.Folder)
               | (entries.Value != null && entries.Value.Length > 0
                   ? (object)Layout.Vertical(
                       entries.Value.Take(20).Select(e =>
                           Text.P($"[{e.Kind}] {e.Name}").Small()
                       ).ToArray())
                   : Text.Muted("No folder selected"));
    }
}
```

## Callback Pattern

All three hooks follow the **UseAlert callback pattern** — the show function accepts a callback with the result, rather than passing callbacks at hook creation time. This means each invocation can have a different callback:

```csharp
var (fileDialogView, showFileDialog) = UseFileDialog(accept: "image/*");

// Different callbacks for different contexts
showFileDialog(files => HandleProfilePhoto(files));
showFileDialog(files => HandleCoverImage(files));
```

Cancel is implicit — the callback simply isn't called when the user dismisses the dialog.

## Integration with DropDownMenu

A common pattern is to trigger file dialogs from dropdown menu items:

```csharp
var trigger = new Button("Actions", icon: Icons.Image, variant: ButtonVariant.Outline);

var menu = new DropDownMenu(DropDownMenu.DefaultSelectHandler(), trigger)
           | MenuItem.Default("Select File…")
               .Icon(Icons.Upload)
               .OnSelect(() => showFileDialog(files => { /* handle files */ }));
```
