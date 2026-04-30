namespace Ivy.Plugins;

public record PluginManifest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ConfigSectionName { get; init; }
    public required Version Version { get; init; }
    public Version? MinimumHostVersion { get; init; }
    public string[] Dependencies { get; init; } = [];
}
