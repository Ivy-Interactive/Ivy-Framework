---
searchHints:
  - upload
  - file
  - attachment
  - drag-drop
  - browse
  - files
imports:
  - Ivy.Services
  - Ivy.Core.Helpers
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
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(fileState))
            .Accept(".txt,.pdf,.cs")
            .MaxFileSize(5 * 1024 * 1024); // 5 MB

        var selected = fileState.Value?.FileName ?? "No file selected";

        return Layout.Vertical()
                | fileState.ToFileInput(upload).Placeholder("Select a file")
                | Text.Large(selected);
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

<Callout Type="tip">
Multiple file selection is automatically enabled when you use `ImmutableArray&lt;FileUpload&lt;T&gt;&gt;` as your state type. You do **not** need to explicitly set a `.Multiple()` property.
</Callout>

## Variants

The `FileInput` widget supports different visual variants:

```csharp demo-below
public class FileInputVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var defaultFile = UseState<FileUpload<byte[]>?>();
        var defaultUpload = this.UseUpload(MemoryStreamUploadHandler.Create(defaultFile));

        var dropFile = UseState<FileUpload<byte[]>?>();
        var dropUpload = this.UseUpload(MemoryStreamUploadHandler.Create(dropFile));

        var dropFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var dropFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(dropFiles));

        return Layout.Vertical()
                | Text.H2("Default Variant")
                | defaultFile.ToFileInput(defaultUpload).Placeholder("Browse for file")
                | Text.H2("Drop Variant (Single)")
                | dropFile.ToFileInput(dropUpload).Variant(FileInputs.Drop)
                | Text.H2("Drop Variant (Multiple)")
                | dropFiles.ToFileInput(dropFilesUpload).Variant(FileInputs.Drop);
    }
}
```

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
            .MaxFileSize(2 * 1024 * 1024); // 2 MB limit

        return Layout.Vertical()
                | Text.H2("2 MB Size Limit")
                | file.ToFileInput(upload).Placeholder("Max 2 MB")
                | (file.Value != null
                    ? Text.P($"Selected: {file.Value.FileName} ({Utils.FormatBytes(file.Value.Length)})")
                    : null);
    }
}
```

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
            .MaxFileSize(5 * 1024 * 1024);

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
                | Text.H2("Upload Progress")
                | files.ToFileInput(upload).Placeholder("Choose files")
                | files.Value.ToTable()
                    .Width(Size.Full())
                    .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                    .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                    .Remove(e => e.Id);
    }
}
```

## Styling

### Placeholder Text

Set custom placeholder text:

```csharp demo-below
public class PlaceholderDemo : ViewBase
{
    public override object? Build()
    {
        var file = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file));

        return file.ToFileInput(upload)
                   .Placeholder("Drag and drop your file here or click to browse");
    }
}
```

### Size Variants

Control the size of the file input:

```csharp demo-below
public class FileInputSizeVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var smallFile = UseState<FileUpload<byte[]>?>();
        var smallUpload = this.UseUpload(MemoryStreamUploadHandler.Create(smallFile));

        var mediumFile = UseState<FileUpload<byte[]>?>();
        var mediumUpload = this.UseUpload(MemoryStreamUploadHandler.Create(mediumFile));

        var largeFile = UseState<FileUpload<byte[]>?>();
        var largeUpload = this.UseUpload(MemoryStreamUploadHandler.Create(largeFile));

        return Layout.Vertical()
                | Text.H2("Small")
                | smallFile.ToFileInput(smallUpload).Small()
                | Text.H2("Medium (Default)")
                | mediumFile.ToFileInput(mediumUpload)
                | Text.H2("Large")
                | largeFile.ToFileInput(largeUpload).Large();
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

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>
