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
client.Toast("Operation successful!");
```

### Opening Dialogs

```csharp
client.Dialog(new DialogContent("Are you sure?", "This action cannot be undone."));
```

### Navigation

```csharp
client.Navigate("/some/path");
```

### File Operations

```csharp
// Download a file
client.DownloadFile("data.csv", csvContent);

// Upload files
client.UploadFiles(async files => {
    foreach (var file in files)
    {
        // Process uploaded file
    }
});
```

## Best Practices

1. **Dependency Injection**: Always use `UseService<IClientProvider>()` to get the client instance.
2. **Error Handling**: Wrap client operations in try-catch blocks when appropriate.
3. **Async Operations**: Use async/await for operations that might take time.
4. **State Management**: Use clients in combination with state management for reactive updates.

## Interactive Examples

### Form Submission with Toast Feedback

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

### File Operations Simulation

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

### Navigation and URL Management

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

## See Also

- [Forms](./Forms.md)
- [State Management](./State.md)
- [Effects](./Effects.md)
