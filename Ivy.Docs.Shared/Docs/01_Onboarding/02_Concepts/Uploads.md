---
searchHints:
  - files
  - upload
  - file-input
  - drag-drop
  - attachments
  - images
---

# Uploads

<Ingress>
Handle file uploads with automatic state management, progress tracking, and support for single/multiple files with built-in validation.
</Ingress>

## Basic Usage

The upload system uses three key components working together:

1. **State for Files**: Holds the uploaded file(s) data in memory
2. **UseUpload Hook**: Creates an upload endpoint and returns an upload context
3. **MemoryStreamUploadHandler**: Automatically manages file data in state

Here's a simple example:

```csharp demo-below
public class SingleFileUpload : ViewBase
{
    public override object? Build()
    {
        var uploadState = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(uploadState));
        return uploadState.ToFileInput(upload).Placeholder("Choose a file to upload");
    }
}
```

## How It Works

The upload flow is automatic:

```csharp
// 1. Create state to hold file data
var uploadState = UseState<FileUpload<byte[]>?>();

// 2. Create upload context with handler - the handler automatically:
//    - Receives the file stream
//    - Reads it into memory
//    - Updates the state with file data
//    - Tracks upload progress
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(uploadState))
    .Accept("image/*")              // Configure accepted file types
    .MaxFileSize(5 * 1024 * 1024);  // Configure max file size (5 MB)

// 3. Connect to a file input - this creates a widget that:
//    - Shows a file picker
//    - Handles file selection
//    - Uploads to the server
//    - Updates the state automatically
uploadState.ToFileInput(upload).Placeholder("Choose an image");

// 4. Access uploaded file data
if (uploadState.Value != null)
{
    var fileName = uploadState.Value.FileName;
    var fileSize = uploadState.Value.Length;
    var fileData = uploadState.Value.Content; // byte[] containing file data
    var progress = uploadState.Value.Progress; // 0.0 to 1.0
}
```

## Multiple File Uploads

Use `ImmutableArray<FileUpload<byte[]>>` for multiple files:

```csharp demo-below
public class MultipleFilesUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(selectedFiles))
            .Accept("*/*")
            .MaxFileSize(10 * 1024 * 1024);

        return Layout.Vertical()
               | selectedFiles.ToFileInput(upload).Placeholder("Choose files to upload")
               | selectedFiles.Value.ToTable()
                   .Width(Size.Full())
                   .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                   .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                   .Remove(e => e.Id);
    }
}
```

## File Validation

Configure validation directly on the upload context:

```csharp demo-below
public class FileUploadValidation : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(selectedFiles))
            .Accept("image/*")                    // Only images
            .MaxFileSize(5 * 1024 * 1024)        // 5 MB per file
            .MaxFiles(3);                         // Maximum 3 files total

        return Layout.Vertical()
               | selectedFiles.ToFileInput(upload).Placeholder("Choose up to 3 images (max 5 MB each)")
               | selectedFiles.Value.ToTable()
                   .Width(Size.Full())
                   .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                   .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                   .Remove(e => e.Id);
    }
}
```

Validation errors are automatically shown to the user via toast notifications.

## File Content Types

The upload handler supports both binary and text content:

```csharp
// Binary content (default)
var binaryState = UseState<FileUpload<byte[]>?>();
var binaryUpload = this.UseUpload(MemoryStreamUploadHandler.Create(binaryState));

// Text content
var textState = UseState<FileUpload<string>?>();
var textUpload = this.UseUpload(MemoryStreamUploadHandler.Create(textState, Encoding.UTF8));

// Multiple binary files
var filesState = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
var filesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(filesState));

// Multiple text files
var textFilesState = UseState(ImmutableArray.Create<FileUpload<string>>());
var textFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(textFilesState, Encoding.UTF8));
```

## Integration Examples

### Dialog Integration

Use ephemeral state for temporary file selection in dialogs:

```csharp demo-tabs
public class DialogFileUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFile = UseState<FileUpload<byte[]>?>();

        // Ephemeral state used inside the dialog while picking a file
        var dialogFile = UseState<FileUpload<byte[]>?>();
        var uploadContext = this.UseUpload(MemoryStreamUploadHandler.Create(dialogFile))
            .Accept("*/*")
            .MaxFileSize(10 * 1024 * 1024);

        var isOpen = UseState(false);

        var dialog = isOpen.Value
            ? new Dialog(
                _ => { isOpen.Value = false; dialogFile.Reset(); return ValueTask.CompletedTask; },
                new DialogHeader("Select File"),
                new DialogBody(
                    dialogFile.ToFileInput(uploadContext).Placeholder("Choose a file to upload")
                ),
                new DialogFooter(
                    new Button("Cancel", _ => { isOpen.Value = false; dialogFile.Reset(); }, variant: ButtonVariant.Outline),
                    new Button("Ok", _ =>
                    {
                        if (dialogFile.Value != null)
                            selectedFile.Set(dialogFile.Value);
                        isOpen.Value = false;
                        dialogFile.Reset();
                    })
                )
            )
            : null;

        return Layout.Vertical()
               | new Button("Open Dialog", _ => { dialogFile.Reset(); isOpen.Value = true; })
               | (selectedFile.Value != null
                   ? selectedFile.ToDetails()
                   : Text.P("No file selected"))
               | dialog;
    }
}
```

### Form Integration

Integrate file uploads in forms using the context-aware `.Builder()` overload:

```csharp demo-tabs
public record FormFileUploadModel
{
    [Required]
    public FileUpload<byte[]>? Attachment1 { get; set; }

    public FileUpload<byte[]>? Attachment2 { get; set; }
}

public class FormFileUpload : ViewBase
{
    public override object? Build()
    {
        var model = UseState(() => new FormFileUploadModel());

        var form = model.ToForm()
            .Builder(e => e.Attachment1, (state, view) =>
            {
                var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                    .Accept("image/jpeg")
                    .MaxFileSize(1 * 1024 * 1024);
                return state.ToFileInput(uploadContext);
            })
            .Label(x => x.Attachment1, "Attachment1 image/jpeg (Required)")
            .Builder(e => e.Attachment2, (state, view) =>
            {
                var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                    .Accept("application/pdf")
                    .MaxFileSize(5 * 1024 * 1024);
                return state.ToFileInput(uploadContext);
            })
            .Label(x => x.Attachment2, "Attachment2 application/pdf (Optional)");

        return Layout.Vertical()
               | form
               | model.Value.Attachment1?.ToDetails()
               | model.Value.Attachment2?.ToDetails();
    }
}
```

## Upload Handlers Overview

Choose a handler that matches your scenario:

- `MemoryStreamUploadHandler`: Reads the entire upload into memory and updates state automatically.
- `ChunkedMemoryStreamUploadHandler`: Accumulates chunked uploads when data arrives in segments, such as audio capture.

<Callout Type="info">
In the docs we also use `SlowMemoryStreamUploadHandler` to simulate ~1 MB/s uploads for demos and progress tracking. It is documentation-only and not meant for production use.
</Callout>

## FileUpload Record

The `FileUpload<TContent>` record contains all file information:

```csharp
public record FileUpload<TContent>
{
    public Guid Id { get; init; }           // Unique identifier
    public string FileName { get; init; }   // Original file name
    public string ContentType { get; init; } // MIME type
    public long Length { get; init; }        // File size in bytes
    public float Progress { get; init; }     // Upload progress (0.0 to 1.0)
    public TContent Content { get; init; }   // File content (byte[] or string)
    public FileUploadStatus Status { get; init; } // Pending, Uploading, Completed, Failed, Aborted
}
```

## Best Practices

1. **Choose the Right Content Type**: Use `byte[]` for binary files, `string` for text files
2. **Set Validation Rules**: Always configure `Accept()` and `MaxFileSize()` to guide users
3. **Limit Multiple Uploads**: Use `MaxFiles()` when accepting multiple files
4. **Progress Feedback**: The `Progress` property automatically updates during upload
5. **State Reset**: Use `state.Reset()` to clear uploaded files
6. **Form Integration**: Use the context-aware `.Builder()` overload for proper hook access

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>
