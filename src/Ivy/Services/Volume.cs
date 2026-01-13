using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Services;

public interface IVolume
{
    public string GetAbsolutePath(params string[] parts);
}

public class FolderVolume(string? mountPath = null) : IVolume, IDescribableService
{
    public string GetAbsolutePath(params string[] parts)
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        var @namespace = assembly?.GetName().Name ?? "unknown";
        string rootPath = (mountPath != null && Directory.Exists(mountPath) ? mountPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                                                               ?? throw new Exception("Cannot resolve fallback path."));
        var fullPath = Path.Combine(rootPath, "Ivy", @namespace, Path.Combine(parts));
        var dir = Path.GetDirectoryName(fullPath);
        if (dir != null && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return fullPath;
    }
    public string ToYaml()
    {
        return "mountPath:" + mountPath;
    }
}

public static class VolumeExtensions
{
    public static Server UseVolume(this Server server, IVolume volume)
    {
        server.Services.AddSingleton(volume);
        return server;
    }
}