---
searchHints:
  - files
  - upload
  - file-input
  - drag-drop
  - attachments
  - images
imports:
  - Ivy.Services
---

# Uploads

<Ingress>
Handle file uploads robustly with support for single/multiple files, drag-and-drop interfaces, and status feedback for various file types.
</Ingress>

## Basic Usage

The most common way to handle uploads is using the FileInput component:

```csharp demo-below
public class FileUploadView : ViewBase
{
    public override object? Build()
    {
        var files = UseState<FileUpload?>(() => null);
        var uploadUrl = this.UseUpload((fileUpload, stream, cancellationToken) => Task.CompletedTask);

        return files.ToFileInput(uploadUrl, "Choose a file");
    }
}
```

## How It Works

The upload system connects three key pieces:

1. **UseUpload Hook**: Creates a server-side upload endpoint and returns a state containing the upload URL
2. **State for Files**: A state variable that holds the selected file(s) information
3. **ToFileInput Extension**: Connects the file state to the upload URL, creating a file input widget

Here's how they work together:

```csharp
var client = UseService<IClientProvider>();

// 1. Create upload handler - returns state with
// URL like "/upload/{connectionId}/{uploadId}"
var uploadUrl = this.UseUpload(
    (fileUpload, stream, cancellationToken) => {
        // This handler is called when a file is uploaded
        // Access file metadata: fileUpload.FileName, fileUpload.ContentType, fileUpload.Length
        // Access file content via the stream parameter
        client.Toast($"Received {fileUpload.Length} bytes", "File Uploaded");
        return Task.CompletedTask;
    }
    // mimeType and fileName are optional parameters with defaults from the uploaded file
);

// 2. Create state to hold file information
var files = UseState<FileUpload?>(() => null);

// 3. Connect them with ToFileInput - creates a widget that:
//    - Updates the files state when user selects files
//    - Automatically uploads to the uploadUrl
//    - Calls your handler with the FileUpload record and stream
files.ToFileInput(uploadUrl, "Choose Files")
```

### Upload Status Feedback

Provide feedback during file upload using toasts:

```csharp demo-below
public class UploadWithStatusView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var files = UseState<FileUpload?>(() => null);
        var uploadUrl = this.UseUpload(
            (fileUpload, stream, cancellationToken) => {
                try {
                    client.Toast($"Successfully uploaded {fileUpload.Length} bytes", "Upload Complete");
                } catch (Exception ex) {
                    client.Toast(ex);
                }
                return Task.CompletedTask;
            }
        );

        return files.ToFileInput(uploadUrl, "Upload File");
    }
}
```

### File Validation

Validate files before upload:

```csharp demo-below
public class ValidatedUploadView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var error = UseState<string?>(() => null);
        var files = UseState<FileUpload?>(() => null);
        var uploadUrl = this.UseUpload(
            (fileUpload, stream, cancellationToken) => {
                if (fileUpload.Length > 2 * 1024 * 1024) // 2MB limit
                {
                    error.Set("File size must be less than 2MB");
                    return Task.CompletedTask;
                }
                error.Set((string?)null);
                // Process uploaded file
                client.Toast($"Image uploaded successfully ({fileUpload.Length} bytes)", "Success");
                return Task.CompletedTask;
            },
            "image/jpeg" // Optional: specify expected MIME type
        );

        return Layout.Vertical(
            files.ToFileInput(uploadUrl, "Upload Image").Accept(".jpg,.jpeg,.png"),
            error.Value != null
                ? new Callout(error.Value, variant: CalloutVariant.Error)
                : null
        );
    }
}
```

### Best Practices

1. **File Validation**: Validate file types and sizes using `Accept()` and custom validation
2. **Status Feedback**: Provide clear feedback about upload status (processing, success, errors)
3. **Error Handling**: Implement proper error handling in your upload handler
4. **Security**: Always validate files on the server side
5. **User Experience**: Show file information (name, size) after selection and clear status messages

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>

## Examples

<Details>
<Summary>
Image Upload with Preview
</Summary>
<Body>

```csharp demo-below
public class ImageUploadView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var preview = UseState<string?>(() => null);
        var files = UseState<FileUpload?>(() => null);
        var uploadUrl = this.UseUpload(
            async (fileUpload, stream, cancellationToken) => {
                // Convert stream to bytes for preview
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken);
                var fileBytes = memoryStream.ToArray();

                // Create preview URL from uploaded bytes
                preview.Set($"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}");
                // Process uploaded file
                client.Toast($"Image uploaded successfully ({fileUpload.Length} bytes)", "Success");
            },
            "image/jpeg"
        );

        return Layout.Vertical(
            files.ToFileInput(uploadUrl, "Upload Image").Accept("image/*"),
            preview.Value != null
                ? new Image(preview.Value)
                : null
        );
    }
}

```

</Body>
</Details>

<Details>
<Summary>
Multiple File Upload with List
</Summary>
<Body>

```csharp demo-below
public class MultiFileUploadView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var uploadedFiles = UseState(() => new List<string>());
        var newFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var uploadUrl = this.UseUpload(
            (fileUpload, stream, cancellationToken) => {
                // Process uploaded file
                client.Toast($"File uploaded ({fileUpload.Length} bytes)", "Upload Complete");
                // Add to list of uploaded files
                uploadedFiles.Set(uploadedFiles.Value.Append($"File {uploadedFiles.Value.Count + 1}").ToList());
                return Task.CompletedTask;
            }
        );

        return Layout.Vertical(
            newFiles.ToFileInput(uploadUrl, "Upload Files"),
            uploadedFiles.Value.Any()
                ? new List(uploadedFiles.Value.Select(f => Text.Inline(f)))
                : null
        );
    }
}
```

</Body>
</Details>
