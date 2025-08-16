# Uploads

<Ingress>
Handle file uploads robustly with support for single/multiple files, drag-and-drop interfaces, and progress tracking for various file types.
</Ingress>

## Overview

The upload system in Ivy supports:

- Single and multiple file uploads
- Drag and drop interfaces
- Progress tracking
- File validation
- Error handling
- Preview capabilities

## Basic Usage

Here's a simple example of handling file uploads:

```csharp
var uploadUrl = this.UseUpload(
    fileBytes => {
        // Process uploaded file bytes
        // For example: save to database, process image, etc.
        Console.WriteLine($"Received {fileBytes.Length} bytes");
    },
    "application/octet-stream",
    "uploaded-file"
);
```

### File Input Component

The most common way to handle uploads is using the FileInput component:

```csharp demo-below
public class UploadView : ViewBase
{
    public override object? Build()
    {
        var isUploading = UseState(() => false);
        var files = UseState<FileInput?>(() => null);
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
            "uploaded-file"
        );

        return Layout.Vertical(
            uploadUrl.Value != null
                ? Text.Inline($"Upload URL: {uploadUrl.Value}")
                : null,
            files.ToFileInput("Choose Files").Accept(".pdf,.doc,.docx")
        );
    }
}
```

### Progress Tracking

Track upload progress for better user feedback:

```csharp demo-below
public class UploadWithProgressView : ViewBase
{
    public override object? Build()
    {
        var progress = UseState(() => 0);
        var isUploading = UseState(() => false);
        var files = UseState<FileInput?>(() => null);
        var uploadUrl = this.UseUpload(
            fileBytes => {
                isUploading.Set(true);
                try {
                    // Process uploaded file bytes
                    Console.WriteLine($"Received {fileBytes.Length} bytes");
                } finally {
                    isUploading.Set(false);
                    progress.Set(0);
                }
            },
            "application/octet-stream",
            "uploaded-file"
        );

        return Layout.Vertical(
            isUploading.Value
                ? new Progress(progress.Value)
                : null,
            files.ToFileInput("Upload Files")
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
            files.ToFileInput("Upload Image").Accept(".jpg,.jpeg,.png")
        );
    }
}
```

### Best Practices

1. **File Validation**: Validate file types and sizes before upload
2. **Progress Feedback**: Show upload progress for better UX
3. **Error Handling**: Implement proper error handling
4. **Security**: Validate files on the server side
5. **User Feedback**: Provide clear feedback about upload status
6. **Accessibility**: Ensure upload interfaces are accessible

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>

## Examples

### Image Upload with Preview

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
            files.ToFileInput("Upload Image").Accept("image/*")
        );
    }
}
```

### Multiple File Upload with List

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
            newFiles.ToFileInput("Upload Files"),
            new List(
                files.Value.Select(f => Text.Inline(f.Name))
            )
        );
    }
}
```
