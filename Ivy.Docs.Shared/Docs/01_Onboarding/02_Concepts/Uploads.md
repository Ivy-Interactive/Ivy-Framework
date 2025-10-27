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
Handle file uploads robustly with support for single/multiple files, drag-and-drop interfaces, and status feedback for various file types.
</Ingress>

## Overview

The upload system in Ivy supports:

- Single and multiple file uploads
- Drag and drop interfaces
- Upload status feedback
- File validation
- Error handling
- Image preview capabilities

## How It Works

The upload system connects three key pieces:

1. **UseUpload Hook**: Creates a server-side upload endpoint and returns a state containing the upload URL
2. **State for Files**: A state variable that holds the selected file(s) information
3. **ToFileInput Extension**: Connects the file state to the upload URL, creating a file input widget

Here's how they work together:

```csharp
// 1. Create upload handler - returns state with URL like "/upload/{connectionId}/{uploadId}"
var uploadUrl = this.UseUpload(
    fileBytes => {
        // This handler is called when a file is uploaded
        Console.WriteLine($"Received {fileBytes.Length} bytes");
    },
    "application/octet-stream",  // Expected MIME type
    "uploaded-file"              // Default filename
);

// 2. Create state to hold file information
var files = UseState<FileInput?>(() => null);

// 3. Connect them with ToFileInput - creates a widget that:
//    - Updates the files state when user selects files
//    - Automatically uploads to the uploadUrl
//    - Calls your handler with the file bytes
files.ToFileInput(uploadUrl, "Choose Files")
```

## Basic Usage

### Single File Upload

The most common way to handle uploads is using the FileInput component:

```csharp demo-below
public class FileUploadView : ViewBase
{
    public override object? Build()
    {
        var files = UseState<FileInput?>(() => null);
        var uploadUrl = this.UseUpload(
            fileBytes => {
                // Process uploaded file bytes
                Console.WriteLine($"Received {fileBytes.Length} bytes");
            },
            "application/octet-stream",
            "uploaded-file"
        );

        return Layout.Vertical(
            files.Value != null
                ? Text.Inline($"Selected: {files.Value.Name} ({files.Value.Size} bytes)")
                : null,
            files.ToFileInput(uploadUrl, "Choose Files").Accept(".pdf,.doc,.docx")
        );
    }
}
```

### Upload Status Feedback

Provide feedback during file upload:

```csharp demo-below
public class UploadWithStatusView : ViewBase
{
    public override object? Build()
    {
        var status = UseState<string?>(() => null);
        var files = UseState<FileInput?>(() => null);
        var uploadUrl = this.UseUpload(
            fileBytes => {
                status.Set("Processing...");
                try {
                    // Process uploaded file bytes
                    Console.WriteLine($"Received {fileBytes.Length} bytes");
                    // Simulate processing
                    System.Threading.Thread.Sleep(1000);
                    status.Set($"✓ Uploaded {fileBytes.Length} bytes successfully");
                } catch (Exception ex) {
                    status.Set($"✗ Upload failed: {ex.Message}");
                }
            },
            "application/octet-stream",
            "uploaded-file"
        );

        return Layout.Vertical(
            status.Value != null
                ? Text.Inline(status.Value)
                : null,
            files.ToFileInput(uploadUrl, "Upload File")
        );
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
        var error = UseState<string?>(() => null);
        var files = UseState<FileInput?>(() => null);
        var uploadUrl = this.UseUpload(
            fileBytes => {
                if (fileBytes.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    error.Set("File size must be less than 5MB");
                    return;
                }
                error.Set((string?)null);
                // Process uploaded file bytes
                Console.WriteLine($"Received {fileBytes.Length} bytes");
            },
            "image/jpeg",
            "uploaded-image"
        );

        return Layout.Vertical(
            error.Value != null
                ? new Callout(error.Value, variant: CalloutVariant.Error)
                : null,
            files.ToFileInput(uploadUrl, "Upload Image").Accept(".jpg,.jpeg,.png")
        );
    }
}
```

### Best Practices

1. **File Validation**: Validate file types and sizes using `Accept()` and custom validation
2. **Status Feedback**: Provide clear feedback about upload status (processing, success, errors)
3. **Error Handling**: Implement proper error handling in your upload handler
4. **Security**: Always validate files on the server side, never trust client-side validation alone
5. **User Experience**: Show file information (name, size) after selection and clear status messages
6. **Accessibility**: Ensure upload interfaces are accessible with proper labels and keyboard support

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
        var preview = UseState<string?>(() => null);
        var isUploading = UseState(() => false);
        var files = UseState<FileInput?>(() => null);
        var uploadUrl = this.UseUpload(
            fileBytes => {
                // Create preview URL from uploaded bytes
                preview.Set($"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}");
                isUploading.Set(true);
                try {
                    // Process uploaded file bytes
                    Console.WriteLine($"Received {fileBytes.Length} bytes");
                } finally {
                    isUploading.Set(false);
                }
            },
            "image/jpeg",
            "uploaded-image"
        );

        return Layout.Vertical(
            preview.Value != null
                ? new Image(preview.Value)
                : null,
            files.ToFileInput(uploadUrl, "Upload Image").Accept("image/*")
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
        var files = UseState(() => new List<FileInput>());
        var isUploading = UseState(() => false);
        var newFiles = UseState<IEnumerable<FileInput>?>(() => null);
        var uploadUrl = this.UseUpload(
            fileBytes => {
                isUploading.Set(true);
                try {
                    // Process uploaded file bytes
                    Console.WriteLine($"Received {fileBytes.Length} bytes");
                } finally {
                    isUploading.Set(false);
                }
            },
            "application/octet-stream",
            "uploaded-files"
        );

        return Layout.Vertical(
            newFiles.ToFileInput(uploadUrl, "Upload Files"),
            new List(
                files.Value.Select(f => Text.Inline(f.Name))
            )
        );
    }
}
```

</Body>
</Details>
