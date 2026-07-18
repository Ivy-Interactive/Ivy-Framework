namespace Ivy.Plugins;

public record PluginIcon
{
    public PluginIconKind Kind { get; init; }
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Creates an icon from a named icon (matching the Icons enum).
    /// </summary>
    public static PluginIcon Named(string name) => new() { Kind = PluginIconKind.Named, Value = name };

    /// <summary>
    /// Creates an icon from a URL (e.g. PNG, SVG, or any image URL).
    /// </summary>
    public static PluginIcon Url(string url) => new() { Kind = PluginIconKind.Url, Value = url };
}

public enum PluginIconKind
{
    Named = 0,
    Url = 1,
}
