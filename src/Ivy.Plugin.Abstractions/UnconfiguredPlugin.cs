namespace Ivy.Plugins;

public record UnconfiguredPlugin(
    string Id,
    string Title,
    string Directory,
    PluginConfigurationSchema Schema,
    IReadOnlyList<string> ValidationErrors);
