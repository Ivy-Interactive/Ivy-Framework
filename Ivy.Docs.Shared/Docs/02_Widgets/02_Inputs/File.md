---
searchHints:
  - upload
  - file
  - attachment
  - drag-drop
  - browse
  - files
---

# FileInput

<Ingress>
Enable file uploads with automatic state management, progress tracking, type filtering, size limits, and support for single or multiple file selections.
</Ingress>

The `FileInput` widget provides a file upload interface with built-in validation, progress tracking, and drag-and-drop support. It works seamlessly with the upload system to automatically manage file data in state.

## Basic Usage

To create a file input, use `ToFileInput()` with an upload context from `UseUpload()`:

```csharp demo-below
public class BasicFileInputDemo : ViewBase
{
    public override object? Build()
    {
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(fileState));
        return fileState.ToFileInput(upload);
   }
}
```

## Single vs Multiple Files

The type of state determines whether single or multiple files can be selected:

```csharp demo-below
public class SingleVsMultipleDemo : ViewBase
{
    public override object? Build()
    {
        // Single file - use nullable FileUpload<byte[]>
        var singleFile = UseState<FileUpload<byte[]>?>();
        var singleUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleFile));

        // Multiple files - use ImmutableArray<FileUpload<byte[]>>
        var multipleFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var multipleUpload = this.UseUpload(MemoryStreamUploadHandler.Create(multipleFiles));

        return Layout.Vertical()
                | Text.H2("Single File")
                | singleFile.ToFileInput(singleUpload).Placeholder("Choose one file")
                | Text.H2("Multiple Files")
                | multipleFiles.ToFileInput(multipleUpload).Placeholder("Choose multiple files");
    }
}
```

Multiple file selection is automatically enabled when you use `ImmutableArray&lt;FileUpload&lt;T&gt;&gt;` as your state type. You do **not** need to explicitly set a `.Multiple()` property.

## File Type Filtering

Use `.Accept()` on the upload context to filter file types:

```csharp demo-below
public class FileTypeFilteringDemo : ViewBase
{
    public override object? Build()
    {
        var imageFile = UseState<FileUpload<byte[]>?>();
        var imageUpload = this.UseUpload(MemoryStreamUploadHandler.Create(imageFile))
            .Accept("image/*");  // Only images

        var documentFile = UseState<FileUpload<byte[]>?>();
        var documentUpload = this.UseUpload(MemoryStreamUploadHandler.Create(documentFile))
            .Accept(".pdf,.doc,.docx");  // Specific file extensions

        return Layout.Vertical()
                | Text.H2("Images Only")
                | imageFile.ToFileInput(imageUpload).Placeholder("Choose an image")
                | Text.H2("Documents Only")
                | documentFile.ToFileInput(documentUpload).Placeholder("Choose a document");
    }
}
```

## File Size Limits

Configure maximum file size with `.MaxFileSize()`:

```csharp demo-below
public class FileSizeLimitDemo : ViewBase
{
    public override object? Build()
    {
        var file = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file))
            .MaxFileSize(FileSize.FromMegabytes(2));

        return Layout.Vertical()
                | Text.H2("2 MB Size Limit")
                | file.ToFileInput(upload).Placeholder("Max 2 MB")
                | (file.Value != null
                    ? Text.P($"Selected: {file.Value.FileName} ({Utils.FormatBytes(file.Value.Length)})")
                    : null);
    }
}
```

<Callout Tip="Info">
You can set size limits clearly with helper methods:
`.FromKilobytes()`, `.FromMegabytes()`, or `.FromGigabytes()` provide clear, self-documenting limits.
</Callout>

## Multiple Files Limit

When accepting multiple files, use `.MaxFiles()` to set a maximum count:

```csharp demo-below
public class MaxFilesDemo : ViewBase
{
    public override object? Build()
    {
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(files))
            .MaxFiles(3)  // Maximum 3 files
            .MaxFileSize(FileSize.FromMegabytes(5));

        return Layout.Vertical()
                | Text.H2("Maximum 3 Files")
                | files.ToFileInput(upload).Placeholder("Choose up to 3 files")
                | Text.P($"{files.Value.Length} file(s) selected");
    }
}
```

## Upload Progress

The `FileUpload` record automatically tracks upload progress:

```csharp demo-below
public class UploadProgressDemo : ViewBase
{
    public override object? Build()
    {
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical()
                | files.ToFileInput(upload).Placeholder("Choose files")
                | files.Value.ToTable()
                    .Width(Size.Full())
                    .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                    .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                    .Remove(e => e.Id);
    }
}
```

### Disabled State

Disable the file input:

```csharp demo-below
public class FileInputDisabledDemo : ViewBase
{
    public override object? Build()
    {
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(fileState));

        return fileState.ToFileInput(upload)
                    .Placeholder("This file input is disabled")
                    .Accept(".jpg,.png")
                    .Disabled();
    }
}
```

## MemoryStreamUploadHandler

`MemoryStreamUploadHandler` automatically manages file uploads by reading the file stream into memory and updating your state. It handles progress tracking, cancellation, and error states automatically.

### Configuration Options

`MemoryStreamUploadHandler.Create()` supports optional configuration parameters:

```csharp
// Default configuration (binary file)
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(fileState));

// Text file with encoding (encoding parameter only available for FileUpload<string>)
var textState = UseState<FileUpload<string>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(textState, System.Text.Encoding.UTF8));

// Binary file with custom chunk size (default: 8192 bytes)
// Larger chunks = fewer progress updates but potentially better performance
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(fileState, chunkSize: 16384));

// Binary file with custom chunk size and progress threshold
// Progress threshold (default: 0.05 = 5%) - only reports progress when it changes by this amount
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(
    fileState, 
    chunkSize: 16384, 
    progressThreshold: 0.1f
));

// Text file with all options
var textState = UseState<FileUpload<string>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(
    textState, 
    encoding: System.Text.Encoding.UTF8,
    chunkSize: 16384,
    progressThreshold: 0.1f
));
```

`MemoryStreamUploadHandler` automatically detects the state type and configures itself accordingly. For binary files, use `FileUpload&lt;byte[]&gt;`. For text files, use `FileUpload&lt;string&gt;` and optionally specify the encoding (defaults to UTF-8).

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>
