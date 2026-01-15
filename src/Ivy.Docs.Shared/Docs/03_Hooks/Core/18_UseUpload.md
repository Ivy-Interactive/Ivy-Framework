---
searchHints:
  - upload
  - useupload
  - file-upload
  - file-input
  - upload-handler
  - file-handling
---

# UseUpload

<Ingress>
The `UseUpload` [hook](../02_RulesOfHooks.md) creates an upload endpoint and returns an upload context that can be used with [FileInput](../../../02_Widgets/04_Inputs/10_File.md) to enable file uploads with automatic state management, progress tracking, and validation.
</Ingress>

## Basic Usage

The `UseUpload` hook takes an upload handler and returns a state containing an `UploadContext`. The context provides an upload URL and methods to configure validation:

```csharp demo-below
public class BasicUploadExample : ViewBase
{
    public override object? Build()
    {
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState));
        
        return fileState.ToFileInput(upload);
    }
}
```

## How It Works

The `UseUpload` hook:

1. **Registers an Upload Handler**: Takes an `IUploadHandler` (like `MemoryStreamUploadHandler`) that processes incoming file streams
2. **Creates an Upload Endpoint**: Registers the handler with the upload service and generates a unique upload URL
3. **Returns Upload Context**: Provides an `UploadContext` containing:

   - `UploadUrl`: The endpoint URL for file uploads
   - `Cancel`: Action to cancel an in-progress upload
   - Validation properties: `Accept`, `MaxFileSize`, `MaxFiles`

```mermaid
graph LR
    A[UseUpload Hook] --> B[Register Handler]
    B --> C[Create Upload Endpoint]
    C --> D[Return UploadContext]
    D --> E[Configure Validation]
    E --> F[Use with FileInput]
```

The upload context is returned as a state, allowing you to configure validation using extension methods:

```csharp
var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState))
    .Accept("image/*")                    // File type filter
    .MaxFileSize(FileSize.FromMegabytes(5)) // Size limit
    .MaxFiles(3);                          // File count limit
```

## Upload Handlers

The most common handler is `MemoryStreamUploadHandler`, which automatically:
- Reads the file stream into memory
- Updates your state with file data
- Tracks upload progress
- Handles single or multiple files based on state type

```csharp demo-below
public class ConfiguredUploadExample : ViewBase
{
    public override object? Build()
    {
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files))
            .Accept("image/*")
            .MaxFileSize(FileSize.FromMegabytes(5))
            .MaxFiles(3);
        
        return Layout.Vertical()
            | files.ToFileInput(upload).Placeholder("Choose up to 3 images")
            | files.Value.ToTable()
                .Builder(e => e.FileName, e => e.Func((string x) => x))
                .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")));
    }
}
```

## UploadContext Properties

| Property     | Type           | Description                                                      |
| ------------ | -------------- | ---------------------------------------------------------------- |
| `UploadUrl`  | `string`       | The endpoint URL for file uploads                                |
| `Cancel`     | `Action<Guid>` | Cancels an in-progress upload by file ID                         |
| `Accept`     | `string?`      | MIME type or file extension filter (e.g., `"image/*"`, `".pdf,.doc"`) |
| `MaxFileSize`| `long?`        | Maximum file size in bytes                                       |
| `MaxFiles`   | `int?`         | Maximum number of files (for multiple file uploads)              |

## See Also

For complete file upload documentation, including:

- File validation and filtering
- Progress tracking
- Single vs multiple file handling
- Form integration
- Custom upload handlers

See the [FileInput Widget](../../../02_Widgets/04_Inputs/10_File.md) documentation.
