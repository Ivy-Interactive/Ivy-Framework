namespace Ivy.Plugins;

public record PluginCandidate(
    string Id,
    string Directory,
    string? FailureReason = null,
    DateTime? FailedAt = null);

public interface IPluginManager
{
    IReadOnlyList<string> GetActivePluginIds();
    PluginManifest? GetPluginManifest(string pluginId);
    PluginConfigurationSchema? GetPluginSchema(string pluginId);
    object? BuildPluginConfigurationView(string pluginId, IIvyPluginConfig config);
    IReadOnlyList<PluginCandidate> GetUnloadedPlugins();
    IReadOnlyList<UnconfiguredPlugin> GetUnconfiguredPlugins();
    bool UnloadPlugin(string pluginId);
    bool LoadPlugin(string pluginPath);
    bool ReloadPlugin(string pluginId);
    bool ReconfigurePlugin(string pluginId);

    event Action<string>? PluginLoaded;
    event Action<string>? PluginUnloaded;
    event Action<string>? PluginReloaded;
    event Action<string>? PluginActivated;
    event Action<string>? PluginDeactivated;
}
