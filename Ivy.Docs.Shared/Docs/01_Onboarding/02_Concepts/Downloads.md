# Downloads

<Ingress>
Enable seamless file downloads for reports, documents, and user content.
</Ingress>

## Overview

The download system in Ivy supports:

- Direct file downloads
- Generated content downloads
- Large file transfers
- Progress tracking
- Error handling

## Basic Usage

Here's a simple example of downloading a file:

```csharp
var client = this.UseService<IClientProvider>();
client.DownloadFile("example.csv", csvContent);
```

### Downloading Generated Content

You can download dynamically generated content:

```csharp
public class ReportView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Download Report",
            onClick: async _ => {
                var report = await GenerateReport();
                client.DownloadFile(
                    "report.pdf",
                    report,
                    contentType: "application/pdf"
                );
            }
        );
    }
}
```

### Progress Tracking

For large files, you can track download progress:

```csharp
var client = this.UseService<IClientProvider>();
var progress = UseState(0.0);

client.DownloadFile(
    "large-file.zip",
    fileContent,
    onProgress: p => progress.Set(p)
);
```

### Error Handling

Proper error handling is essential for downloads:

```csharp demo-below 
public class DownloadView : ViewBase
{
        public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var error = UseState<string?>(() => null);
        var downloadUrl = this.UseDownload(
            () => Task.FromResult(System.Text.Encoding.UTF8.GetBytes("Hello World")),
            "text/plain",
            "file.txt"
        );

        if (downloadUrl.Value == null) return null;

        return Layout.Vertical(
            error.Value != null
                ? Callout.Error(error.Value)
                : null,
            new Button("Download").Url(downloadUrl.Value)
        );
    }
}
```

### Best Practices

1. **File Naming**: Use clear, descriptive filenames
2. **Content Types**: Always specify the correct content type
3. **Error Handling**: Implement proper error handling for failed downloads
4. **Progress Feedback**: Show progress for large files
5. **Security**: Validate file types and sizes before download
6. **Caching**: Consider implementing caching for frequently downloaded files

<WidgetDocs Type="Ivy.Hooks.UseDownloadExtensions" ExtensionTypes="" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Hooks/UseDownload.cs"/>

## Examples

### CSV Export

```csharp demo-below 
public class DataExportView : ViewBase
{
        public override object? Build()
    {
        var downloadUrl = this.UseDownload(
            () => Task.FromResult(System.Text.Encoding.UTF8.GetBytes("Name,Email,Age\nJohn,john@example.com,30\nJane,jane@example.com,25")),
            "text/csv",
            $"export-{DateTime.Now:yyyy-MM-dd}.csv"
        );

        if (downloadUrl.Value == null) return null;

        return new Button("Export to CSV").Url(downloadUrl.Value);
    }


}
```

### Large File Download with Progress

```csharp demo-below 
public class LargeFileView : ViewBase
{
    public override object? Build()
    {
        var downloadUrl = this.UseDownload(
            () => Task.FromResult(GenerateLargeFile()),
            "application/zip",
            "large-file.zip"
        );

        if (downloadUrl.Value == null) return null;

        return new Button("Download Large File").Url(downloadUrl.Value);
    }

    // Helper method
    private byte[] GenerateLargeFile()
    {
        // Simulate large file generation
        return System.Text.Encoding.UTF8.GetBytes("Large file content...");
    }
}
```
