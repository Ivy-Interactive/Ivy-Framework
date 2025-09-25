# Volume

Ivy provides a standardized way to manage application data storage through the `IVolume` interface and `FolderVolume` implementation. This ensures consistent file path handling and proper directory structure for your application's data files.

## Basic Usage

Configure a volume for your application during server startup:

```csharp
using Ivy.Services;

var server = new Server();

// Configure a volume for your application
var volume = new FolderVolume("/data/myapp");
server.UseVolume(volume);

await server.RunAsync();
```

### IVolume Interface

The `IVolume` interface defines the contract for volume implementations:

The `GetAbsolutePath` method combines the volume root with the specified path parts and returns the absolute path. It automatically creates parent directories if they don't exist.

```csharp
public interface IVolume
{
    public string GetAbsolutePath(params string[] parts);
}
```

### Overview

The Volume Management system in Ivy provides:

- **Automatic directory creation**: Parent directories are created automatically when you request a path
- **Namespace isolation**: Files are organized under `Ivy/{YourAppName}/` to prevent conflicts between applications
- **Fallback to local app data**: If the configured root directory doesn't exist, it falls back to the system's local application data folder
- **Clean path composition**: Use params array for path parts instead of manual string concatenation
- **Dependency injection support**: Volumes are registered as services and can be injected into your application components

## FolderVolume Implementation

The `FolderVolume` class provides the standard implementation for file system volumes:

### Constructor

```csharp
public class FolderVolume(string root) : IVolume
```

- **root**: The root directory path for the volume. If this directory doesn't exist, the volume will fall back to the system's local application data folder.

### Path Structure

The `FolderVolume` creates paths in the following structure:

```text
{root}/Ivy/{YourAppName}/{pathParts}
```

Where:

- `{root}` is the configured root directory or fallback location
- `Ivy` is a fixed namespace prefix
- `{YourAppName}` is automatically derived from your application's assembly name
- `{pathParts}` are the path parts you provide to `GetAbsolutePath`

## Examples

<Details>
<Summary>
Using Volumes in Services
</Summary>
<Body>
Inject and use the volume in your services:

```csharp
using Ivy.Services;

public class FileService(IVolume volume)
{
    public void SaveUserData(string userId, byte[] data)
    {
        // Automatically creates the full path: /data/myapp/Ivy/YourAppName/users/123/profile.json
        var path = volume.GetAbsolutePath("users", userId, "profile.json");
        File.WriteAllBytes(path, data);
    }
    
    public byte[] LoadUserData(string userId)
    {
        var path = volume.GetAbsolutePath("users", userId, "profile.json");
        return File.ReadAllBytes(path);
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Using Volumes in Views
</Summary>
<Body>
Access volumes through dependency injection in your views:

```csharp
public class DataManagementView : ViewBase
{
    public override object? Build()
    {
        var volume = this.UseService<IVolume>();
        
        return new Column
        {
            Children = [
                new Button("Save Data")
                {
                    OnClick = () => SaveData(volume)
                },
                new Button("Load Data")
                {
                    OnClick = () => LoadData(volume)
                }
            ]
        };
    }
    
    private void SaveData(IVolume volume)
    {
        var data = Encoding.UTF8.GetBytes("Sample data");
        var path = volume.GetAbsolutePath("cache", "sample.txt");
        File.WriteAllBytes(path, data);
    }
    
    private void LoadData(IVolume volume)
    {
        var path = volume.GetAbsolutePath("cache", "sample.txt");
        if (File.Exists(path))
        {
            var data = File.ReadAllBytes(path);
            // Process data...
        }
    }
}
```

</Body>
</Details>
