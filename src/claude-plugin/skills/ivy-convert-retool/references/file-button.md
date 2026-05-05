# File Button

A button/input that allows users to select single or multiple files from their system, with configurable constraints like accepted file types, size limits, and optional upload handling.

## Retool

```toolscript
fileButton1.files        // array of selected file metadata
fileButton1.value        // array of uploaded files
fileButton1.parsedValue  // parsed plain-text content

// Configuration
fileButton1.accept = [".pdf", ".docx"]
fileButton1.maxSize = 5242880          // 5 MB in bytes
fileButton1.maxCount = 3
fileButton1.selectionType = "multiple"
fileButton1.parseFiles = true
fileButton1.uploadToRetoolStorage = true

// Methods
fileButton1.clearValue()
fileButton1.resetValue()
fileButton1.setDisabled(true)
fileButton1.validate()
```

## Ivy

```csharp
public class FileButtonComparison : ViewBase
{
    public override object? Build()
    {
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files))
            .Accept(".pdf,.docx")
            .MaxFileSize(FileSize.FromMegabytes(5))
            .MaxFiles(3);

        return files.ToFileInput(upload)
            .Placeholder("Select files");
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Accepted file types | `accept` — array of extensions or MIME types | `.Accept(string)` — comma-separated extensions or MIME types (e.g. `"image/*"`, `".pdf,.doc"`) |
| Max file size | `maxSize` — max size in bytes | `.MaxFileSize(FileSize)` — via `FileSize.FromKilobytes()`, `.FromMegabytes()`, `.FromGigabytes()` |
| Max file count | `maxCount` — max number of files | `.MaxFiles(int)` — max number of files |
| Min file count | `minCount` — min number of files | Not supported |
| Min file size | `minSize` — min size in bytes | Not supported |
| Multiple selection | `selectionType` — `"single"`, `"multiple"`, or `"directory"` | Automatic — use `ImmutableArray<FileUpload<T>>` for multiple, `FileUpload<T>?` for single |
| Directory selection | `selectionType = "directory"` | Not supported |
| Disabled | `disabled` — boolean | `.Disabled()` or constructor parameter `disabled: true` |
| Placeholder text | `text` — button label | `.Placeholder(string)` |
| Display variant | `styleVariant` — `"solid"` or `"outline"` | `.Variant(FileInputs)` — `FileInputs.Drop` (default) |
| Loading indicator | `loading` / `loaderPosition` | Not supported (progress is per-file via `FileUpload.Progress`) |
| Icon before/after | `iconBefore` / `iconAfter` | Not supported |
| Parse file content | `parseFiles` / `parsedValue` — auto-parse plain text | Not supported (use `FileUpload<string>` with `Encoding.UTF8` for text content) |
| Upload to storage | `uploadToRetoolStorage` / `isPublic` / `shouldOverwriteOnNameCollision` | `UseUpload(handler)` — uploads handled via `MemoryStreamUploadHandler`; no built-in cloud storage integration |
| Append new selection | `appendNewSelection` — add files instead of replacing | Not supported (multiple files accumulate by default with `ImmutableArray`) |
| Visibility | `setHidden(bool)` method | `.Visible` property |
| Custom styling | `style` object | `.Width(Size)`, `.Height(Size)`, `.Scale(Scale)` |
| Validation | `validate()` / `clearValidation()` methods | `.Invalid(string)` — set validation message; form-level `[Required]` attribute |
| File metadata | `files[].name`, `.size`, `.type`, `.lastModified` | `FileUpload.FileName`, `.Length`, `.ContentType` (no `LastModified`) |
| Upload progress | Not available (binary uploaded/not) | `FileUpload.Progress` — float 0.0–1.0 with real-time updates |
| Upload status | `files[].uploading` — boolean | `FileUpload.Status` — `Pending`, `Uploading`, `Completed`, `Failed`, `Aborted` |
| Cancel upload | Not supported | `UploadContext.Cancel(Guid fileId)` / `.HandleCancel()` event |
| On change event | Change event handler | `.HandleChange()` / `OnChange` event |
| On parse event | Parse event handler | Not supported |
| On blur event | Not supported | `.HandleBlur()` / `OnBlur` event |
| Focus method | `focus()` | Not supported |
| Scroll into view | `scrollIntoView(options)` | Not supported |
| Clear/reset value | `clearValue()` / `resetValue()` | `state.Reset()` |
| Nullable | Not applicable | `.Nullable(bool)` |
