using Ivy.Plugins;

namespace Ivy.Core.Plugins;

public static class PluginLoaderExtensions
{
    public static IReadOnlyDictionary<string, PluginConfigurationSchema> GetPluginConfigurationSchemas(
        this PluginLoader loader)
    {
        return loader.Plugins
            .Where(p => p.Instance.ConfigurationSchema is not null)
            .ToDictionary(
                p => p.Instance.Manifest.Id,
                p => p.Instance.ConfigurationSchema!);
    }
}
