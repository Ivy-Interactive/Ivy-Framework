# Volume Management for Application Data

Ivy provides a standardized way to manage application data storage through the `IVolume` interface and `FolderVolume` implementation. This ensures consistent file path handling and proper directory structure for your application's data files.

## Overview

The Volume Management system in Ivy provides:

- **Automatic directory creation**: Parent directories are created automatically when you request a path
- **Namespace isolation**: Files are organized under `Ivy/{YourAppName}/` to prevent conflicts between applications
- **Fallback to local app data**: If the configured root directory doesn't exist, it falls back to the system's local application data folder
- **Clean path composition**: Use params array for path parts instead of manual string concatenation
- **Dependency injection support**: Volumes are registered as services and can be injected into your application components

## Basic Usage

### Server Configuration

Configure a volume for your application during server startup:

```csharp
using Ivy.Services;

var server = new Server();

// Configure a volume for your application
var volume = new FolderVolume("/data/myapp");
server.UseVolume(volume);

await server.RunAsync();
```

### Using Volumes in Services

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

### Using Volumes in Views

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

## IVolume Interface

The `IVolume` interface defines the contract for volume implementations:

```csharp
public interface IVolume
{
    /// <summary>
    /// Gets the absolute path by combining the volume root with the specified path parts.
    /// Automatically creates parent directories if they don't exist.
    /// </summary>
    /// <param name="parts">Path parts to combine</param>
    /// <returns>Absolute path to the file or directory</returns>
    public string GetAbsolutePath(params string[] parts);
}
```

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

### Examples

```csharp
var volume = new FolderVolume("/data/myapp");

// Creates: /data/myapp/Ivy/MyApplication/users/123/profile.json
var userProfile = volume.GetAbsolutePath("users", "123", "profile.json");

// Creates: /data/myapp/Ivy/MyApplication/cache/temp.dat
var cacheFile = volume.GetAbsolutePath("cache", "temp.dat");

// Creates: /data/myapp/Ivy/MyApplication/logs/app.log
var logFile = volume.GetAbsolutePath("logs", "app.log");
```

## Fallback Behavior

If the configured root directory doesn't exist, `FolderVolume` automatically falls back to the system's local application data folder:

- **Windows**: `%LOCALAPPDATA%`
- **macOS**: `~/Library/Application Support`
- **Linux**: `~/.local/share`

This ensures your application can always store data even if the configured directory is unavailable.

## Best Practices

### 1. Organize Your Data

Use meaningful path structures to organize your application data:

```csharp
public class DataManager(IVolume volume)
{
    // User-specific data
    public string GetUserDataPath(string userId) => 
        volume.GetAbsolutePath("users", userId);
    
    // Application cache
    public string GetCachePath(string cacheKey) => 
        volume.GetAbsolutePath("cache", cacheKey);
    
    // Logs
    public string GetLogPath(string logName) => 
        volume.GetAbsolutePath("logs", logName);
    
    // Temporary files
    public string GetTempPath(string fileName) => 
        volume.GetAbsolutePath("temp", fileName);
}
```

### 2. Handle File Operations Safely

Always check for file existence and handle exceptions:

```csharp
public class SafeFileService(IVolume volume)
{
    public async Task<bool> SaveDataAsync(string fileName, byte[] data)
    {
        try
        {
            var path = volume.GetAbsolutePath("data", fileName);
            await File.WriteAllBytesAsync(path, data);
            return true;
        }
        catch (Exception ex)
        {
            // Log error and handle gracefully
            Console.WriteLine($"Failed to save data: {ex.Message}");
            return false;
        }
    }
    
    public async Task<byte[]?> LoadDataAsync(string fileName)
    {
        try
        {
            var path = volume.GetAbsolutePath("data", fileName);
            if (File.Exists(path))
            {
                return await File.ReadAllBytesAsync(path);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load data: {ex.Message}");
            return null;
        }
    }
}
```

### 3. Use Dependency Injection

Register your volume as a singleton service to ensure consistency across your application:

```csharp
public class Program
{
    public static void Main()
    {
        var server = new Server();
        
        // Configure volume
        var volume = new FolderVolume("/data/myapp");
        server.UseVolume(volume);
        
        // Register your services
        server.Services.AddSingleton<IDataService, DataService>();
        
        await server.RunAsync();
    }
}
```

## Advanced Usage

### Custom Volume Implementation

You can create custom volume implementations for different storage backends:

```csharp
public class CloudVolume : IVolume
{
    private readonly string _bucketName;
    private readonly ICloudStorageClient _client;
    
    public CloudVolume(string bucketName, ICloudStorageClient client)
    {
        _bucketName = bucketName;
        _client = client;
    }
    
    public string GetAbsolutePath(params string[] parts)
    {
        // Implement cloud storage path logic
        var path = string.Join("/", parts);
        return $"{_bucketName}/{path}";
    }
}

// Usage
var cloudVolume = new CloudVolume("my-bucket", cloudClient);
server.UseVolume(cloudVolume);
```

### Multiple Volumes

You can register multiple volumes for different purposes:

```csharp
public class MultiVolumeService
{
    private readonly IVolume _dataVolume;
    private readonly IVolume _cacheVolume;
    
    public MultiVolumeService(IServiceProvider services)
    {
        _dataVolume = services.GetRequiredService<IVolume>("data");
        _cacheVolume = services.GetRequiredService<IVolume>("cache");
    }
    
    public void SaveData(string key, byte[] data)
    {
        var path = _dataVolume.GetAbsolutePath("persistent", key);
        File.WriteAllBytes(path, data);
    }
    
    public void SaveCache(string key, byte[] data)
    {
        var path = _cacheVolume.GetAbsolutePath("temp", key);
        File.WriteAllBytes(path, data);
    }
}
```

## Integration with Other Services

### Upload Service Integration

Volumes work seamlessly with Ivy's upload service:

```csharp
public class FileUploadHandler(IVolume volume, IUploadService uploadService)
{
    public void SetupFileUpload()
    {
        var (cleanup, url) = uploadService.AddUpload(
            async (fileData) =>
            {
                var path = volume.GetAbsolutePath("uploads", "user-file.dat");
                await File.WriteAllBytesAsync(path, fileData);
            },
            "application/octet-stream",
            "uploaded-file.dat"
        );
        
        // Use the URL in your UI
    }
}
```

### Download Service Integration

Similarly, volumes integrate with the download service:

```csharp
public class FileDownloadHandler(IVolume volume, IDownloadService downloadService)
{
    public void SetupFileDownload(string fileName)
    {
        var (cleanup, url) = downloadService.AddDownload(
            async () =>
            {
                var path = volume.GetAbsolutePath("exports", fileName);
                return await File.ReadAllBytesAsync(path);
            },
            "application/octet-stream",
            fileName
        );
        
        // Use the URL in your UI
    }
}
```

## Troubleshooting

### Common Issues

1. **Permission Denied**: Ensure your application has write permissions to the configured directory
2. **Path Too Long**: Avoid creating deeply nested directory structures
3. **Disk Space**: Monitor available disk space, especially for applications that store large amounts of data

### Debugging

Enable verbose logging to see volume operations:

```csharp
var server = new Server(new ServerArgs { Verbose = true });
```

This will help you track file operations and identify any issues with volume configuration.

## Summary

Volume Management in Ivy provides a robust, standardized approach to file system operations in your applications. By using the `IVolume` interface and `FolderVolume` implementation, you get:

- Automatic directory creation
- Namespace isolation between applications
- Fallback to system directories
- Clean dependency injection integration
- Consistent path handling across your application

This system ensures your application data is properly organized and accessible while maintaining clean separation between different applications running on the same system.
