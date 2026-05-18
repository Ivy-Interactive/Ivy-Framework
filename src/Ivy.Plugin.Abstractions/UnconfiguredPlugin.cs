namespace Ivy.Plugins;

public record UnconfiguredPlugin(
    string Id,
    string Name,
    string Directory,
    PluginConfigurationSchema Schema,
    IReadOnlyList<string> ValidationErrors);
