namespace Ivy.Plugins;

/// <summary>
/// Metadata describing a plugin.
///
/// COMPATIBILITY NOTE: Only optional (nullable, non-required) properties may be added
/// to this record going forward. Adding new 'required' properties is a breaking change
/// for all existing plugins.
/// </summary>
public record PluginManifest
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public Version? MinimumHostVersion { get; init; }
    public PluginIcon? Icon { get; init; }
}
