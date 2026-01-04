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

You can show [toasts](./23_Alerts.md):

```csharp
// Simple toast
client.Toast("Operation successful!");

// Toast with title
client.Toast("Data saved", "Success");
```

You can [navigate](./14_Navigation.md) to different pages within the app:

```csharp
// Navigate to different pages within the app
client.Navigate("/dashboard");

// Redirect to external site (replaces current page)
client.Redirect("https://example.com");

// Open URL in new tab (keeps current page open)
client.OpenUrl("https://github.com");
client.OpenUrl(new Uri("https://stackoverflow.com"));
```

You can [download](./24_Downloads.md) and [upload](../../02_Widgets/04_Inputs/10_File.md) files:

```csharp
// Download CSV data
var csvData = Encoding.UTF8.GetBytes("Name,Age\nJohn,30\nJane,25");
client.DownloadFile("users.csv", csvData, "text/csv");

// Download with progress tracking
var progress = UseState(0.0);
client.DownloadFile("large-file.zip", fileData, onProgress: p => progress.Value = p);
```

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
```

You can copy text to the clipboard:

```csharp
// Copy text to clipboard
client.CopyToClipboard("Copied to clipboard!");
client.Toast("Text copied!");
```

You can set the [theme mode](./17_Theming.md) and apply custom CSS:

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
4. **State Management**: Use clients in combination with [state management](./05_State.md) for reactive updates.

## UI Refresh & State Management

Ivy automatically handles UI refreshes in most cases. You typically **don't need** to manually refresh the UI:

- **[Form Submissions](./13_Forms.md)**: When forms are submitted successfully, the UI automatically updates
- **State Changes**: When [state](./05_State.md) values change, the UI automatically re-renders
- **Sheet Dismissal**: [Sheets](../../02_Widgets/07_Advanced/02_Sheet.md) are automatically closed by the framework when forms are submitted successfully
- **Navigation**: Page [navigation](./14_Navigation.md) automatically refreshes the UI

**Do this instead** - Let the framework handle it:

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
[Navigation](./14_Navigation.md) and URL Management
</Summary>
<Body>

```csharp demo-tabs
public class ClientNavigationApp : ViewBase
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

- [Forms](./13_Forms.md)
- [State Management](./05_State.md)
- [Effects](./09_Effects.md)
- [Alerts & Notifications](./23_Alerts.md)
