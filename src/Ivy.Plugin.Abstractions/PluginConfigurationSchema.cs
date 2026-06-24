namespace Ivy.Plugins;

public record PluginConfigurationSchema
{
    internal PluginConfigurationSchema() { }
    public IReadOnlyList<ConfigFieldDefinition> Fields { get; internal init; } = [];
}
