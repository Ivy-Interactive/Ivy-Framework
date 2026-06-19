namespace Ivy.Plugins;

public record PluginConfigurationSchema
{
    public IReadOnlyList<ConfigFieldDefinition> Fields { get; init; } = [];
}
