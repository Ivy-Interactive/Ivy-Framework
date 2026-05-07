# File Input

An input field to select single or multiple files. Supports file type filtering, size validation, drag-and-drop, and progress tracking.

## Retool

```toolscript
fileInput1.value        // array of uploaded files
fileInput1.files        // metadata for selected files (name, size, type, etc.)
fileInput1.clearValue() // clear all selected files
```

## Ivy

```csharp
public class FileInputDemo : ViewBase
{
    public override object? Build()
    {
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState));

        return fileState
            .ToFileInput(upload)
            .Accept("image/*,.pdf")
            .MaxFileSize(FileSize.FromMegabytes(10))
            .MaxFiles(5)
            .Placeholder("Drop files here");
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Accepted file types | `accept` — array of MIME types or extensions | `Accept` — string MIME type or extension filter (e.g. `"image/*"`, `".pdf"`) |
| Append new selection | `appendNewSelection` — appends new files to existing selection | Not supported |
| Disabled | `disabled` — prevents interaction | `Disabled` — `bool`, disables input |
| Files metadata | `files` — read-only array of file objects with name, size, type, etc. | `FileUpload<TContent>` record with `FileName`, `ContentType`, `Length`, `Progress`, `Status` |
| Icon after | `iconAfter` — suffix icon identifier | Not supported |
| Icon before | `iconBefore` — prefix icon identifier | Not supported |
| Upload to storage | `isPublic` / `uploadToRetoolStorage` — uploads to Retool Storage | `UploadUrl` — custom upload endpoint via `UseUpload` hook |
| Label position | `labelPosition` — `"top"` or `"left"` | Not supported (layout handled externally) |
| Loading indicator | `loading` — shows loading state | `Progress` on `FileUpload` record tracks upload progress |
| Max file count | `maxCount` — maximum selectable files | `MaxFiles` — `int?`, maximum number of files |
| Max file size | `maxSize` — max size in bytes | `MaxFileSize` — `long?`, max size in bytes (helper: `FileSize.FromMegabytes()`) |
| Min file count | `minCount` — minimum required files | Not supported |
| Min file size | `minSize` — minimum file size in bytes | Not supported |
| Parse files | `parseFiles` — parses plain-text content | Not supported |
| Parsing indicator | `parsing` — read-only, indicates parsing in progress | Not supported |
| Placeholder | `placeholder` — default display text | `Placeholder` — `string` |
| Selection type | `selectionType` — `"single"`, `"multiple"`, or `"directory"` | `Multiple` — auto-determined by state type (single vs list) |
| Overwrite on collision | `shouldOverwriteOnNameCollision` — replaces files with same name | Not supported |
| Value | `value` — read-only array of uploaded files | `Value` — `TValue`, current file value bound to state |
| Validation | `clearValidation()` method | `Invalid` — `string`, set validation error message |
| Visibility | `setHidden(hidden)` method | `Visible` — `bool` |
| Focus | `focus()` method | Not supported |
| Scroll into view | `scrollIntoView(options)` method | Not supported |
| Variant | N/A | `Variant` — `FileInputs` enum (e.g. `Drop`) |
| Nullable | N/A | `Nullable` — `bool`, allow null values |
| Cancel upload | N/A | `OnCancel` event handler |
| Drag-and-drop | Built into component | Built-in with `FileInputs.Drop` variant |
