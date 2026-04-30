namespace Ivy.Plugins;

public record PluginCandidate(
    string Id,
    string Directory,
    string? FailureReason = null,
    DateTime? FailedAt = null);

public interface IPluginManager
{
    IReadOnlyList<string> GetLoadedPluginIds();
    IReadOnlyList<PluginCandidate> GetUnloadedPlugins();
    bool UnloadPlugin(string pluginId);
    bool LoadPlugin(string pluginPath);
    bool ReloadPlugin(string pluginId);

    event Action<string>? PluginLoaded;
    event Action<string>? PluginUnloaded;
    event Action<string>? PluginReloaded;
}
