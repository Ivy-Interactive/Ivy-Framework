---
searchHints:
  - browser
  - client-side
  - javascript
  - interop
  - api
  - provider
---

# Clients

<Ingress>
Bridge server-side C# code with client-side React components for seamless browser interactions and client-side operations.
</Ingress>

## Overview

The `IClientProvider` interface is the main entry point for client-side interactions. It's available through dependency injection using the `UseService` hook:

```csharp
var client = this.UseService<IClientProvider>();
```

## Common Use Cases

### Showing Toasts

```csharp
// Simple toast
client.Toast("Operation successful!");

// Toast with title
client.Toast("Data saved", "Success");
```

### Navigation

```csharp
// Navigate to different pages within the app
client.Navigate("/dashboard");

// Redirect to external site (replaces current page)
client.Redirect("https://example.com");

// Open URL in new tab (keeps current page open)
client.OpenUrl("https://github.com");
client.OpenUrl(new Uri("https://stackoverflow.com"));
```

### Downloading Files

```csharp
// Download CSV data
var csvData = Encoding.UTF8.GetBytes("Name,Age\nJohn,30\nJane,25");
client.DownloadFile("users.csv", csvData, "text/csv");

// Download with progress tracking
var progress = UseState(0.0);
client.DownloadFile("large-file.zip", fileData, onProgress: p => progress.Value = p);
```

### Uploading Files

```csharp
// Handle single file upload
client.UploadFiles(async files => {
    var file = files.FirstOrDefault();
    if (file != null)
    {
        var content = await file.GetContentAsync();
        await ProcessFileAsync(file.FileName, content);
        client.Toast($"Uploaded {file.FileName}");
    }
});

// Handle multiple file uploads
client.UploadFiles(async files => {
    var uploadTasks = files.Select(async file => {
        var content = await file.GetContentAsync();
        await ProcessFileAsync(file.FileName, content);
        return file.FileName;
    });
    
    var uploadedFiles = await Task.WhenAll(uploadTasks);
    client.Toast($"Uploaded {uploadedFiles.Length} files");
});
```

### Clipboard Operations

```csharp
// Copy text to clipboard
client.CopyToClipboard("Copied to clipboard!");
client.Toast("Text copied!");
```

### Theme Customization

```csharp
// Set theme mode
client.SetThemeMode(ThemeMode.Dark);

// Apply custom CSS
var customCss = @"
:root {
    --primary: #ff6b6b;
    --secondary: #4ecdc4;
}";
client.ApplyTheme(customCss);
```

## Best Practices

1. **Dependency Injection**: Always use `UseService<IClientProvider>()` to get the client instance.
2. **Error Handling**: Wrap client operations in try-catch blocks when appropriate.
3. **Async Operations**: Use async/await for operations that might take time.
4. **State Management**: Use clients in combination with state management for reactive updates.

## UI Refresh & State Management

Ivy automatically handles UI refreshes in most cases. You typically **don't need** to manually refresh the UI:

- **Form Submissions**: When forms are submitted successfully, the UI automatically updates
- **State Changes**: When state values change, the UI automatically re-renders
- **Sheet Dismissal**: Sheets are automatically closed by the framework when forms are submitted successfully
- **Navigation**: Page navigation automatically refreshes the UI

❌ **Don't do this** - The framework handles it automatically:

```csharp
// Don't call Refresh() on IClientProvider
client.Refresh(); // This method doesn't exist and isn't needed
```

✅ **Do this instead** - Let the framework handle it:

```csharp
// Just update state, UI refreshes automatically
var isOpen = UseState(false);
var formData = UseState("");

// When form submits successfully, sheet closes automatically
if (formSubmitted.Value)
{
    formSubmitted.Value = false;
    isOpen.Value = false; // This triggers UI update
    client.Toast("Form saved successfully!");
}
```

## Examples

<Details>
<Summary>
Form Submission with Toast Feedback
</Summary>
<Body>

```csharp demo-tabs
public class FormSubmissionApp : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var nameState = UseState("");
        var submitTrigger = UseState(false);
        
        if (submitTrigger.Value)
        {
            submitTrigger.Value = false;
            var name = nameState.Value;
            if (string.IsNullOrEmpty(name))
            {
                client.Toast("Please enter a name", "Validation Error");
            }
            else
            {
                client.Toast($"Hello, {name}! Form submitted successfully.");
            }
        }
        
        return Layout.Vertical(
            new TextInput(nameState.Value, e => nameState.Value = e.Value) { Placeholder = "Your name" },
            new Button("Submit Form", _ => submitTrigger.Value = true)
        );
    }
}
```

</Body>
</Details>

<Details>
<Summary>
File Operations Simulation
</Summary>
<Body>

```csharp demo-tabs
public class FileOperationsApp : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var downloadTrigger = UseState(false);
        var uploadTrigger = UseState(false);
        var downloadComplete = UseState(false);
        var uploadComplete = UseState(false);
        
        if (downloadTrigger.Value)
        {
            downloadTrigger.Value = false;
            downloadComplete.Value = false;
            client.Toast("Downloading file...", "Download Started");
            
            // Simulate download completion after 2 seconds
            Task.Run(async () => {
                await Task.Delay(2000);
                downloadComplete.Value = true;
            });
        }
        
        if (uploadTrigger.Value)
        {
            uploadTrigger.Value = false;
            uploadComplete.Value = false;
            client.Toast("Uploading files...", "Upload Started");
            
            // Simulate upload completion after 3 seconds
            Task.Run(async () => {
                await Task.Delay(3000);
                uploadComplete.Value = true;
            });
        }
        
        // Show completion messages when state changes
        if (downloadComplete.Value)
        {
            downloadComplete.Value = false;
            client.Toast("File downloaded successfully!");
        }
        
        if (uploadComplete.Value)
        {
            uploadComplete.Value = false;
            client.Toast("Files uploaded successfully!");
        }
        
        return Layout.Vertical(
            new Button("Simulate File Download", _ => downloadTrigger.Value = true),
            new Button("Simulate File Upload", _ => uploadTrigger.Value = true)
        );
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Navigation and URL Management
</Summary>
<Body>

```csharp demo-tabs
public class NavigationApp : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var copyTrigger = UseState(false);
        var openTabsTrigger = UseState(false);
        
        if (copyTrigger.Value)
        {
            copyTrigger.Value = false;
            var appDescriptor = UseService<AppDescriptor>();
            var info = $"Current app: {appDescriptor.Title}";
            client.CopyToClipboard(info);
            client.Toast("Page info copied to clipboard!");
        }
        
        if (openTabsTrigger.Value)
        {
            openTabsTrigger.Value = false;
            client.OpenUrl("https://google.com");
            client.OpenUrl("https://github.com");
            client.Toast("Opened multiple tabs", "Navigation");
        }
        
        return Layout.Vertical(
            new Button("Copy Current Page Info", _ => copyTrigger.Value = true),
            new Button("Open Multiple Tabs", _ => openTabsTrigger.Value = true)
        );
    }
}
```

</Body>
</Details>

## See Also

- [Forms](./Forms.md)
- [State Management](./State.md)
- [Effects](./Effects.md)
- [Alerts & Notifications](./Alerts.md)
