namespace Ivy.Plugins;

public record PluginConfigurationSchema
{
    public ConfigFieldDefinition[] Fields { get; init; } = [];
}
