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
    /// Creates an icon from raw SVG markup.
    /// </summary>
    public static PluginIcon Svg(string svg) => new() { Kind = PluginIconKind.Svg, Value = svg };

    /// <summary>
    /// Creates an icon from a URL (e.g. PNG, SVG, or any image URL).
    /// </summary>
    public static PluginIcon Url(string url) => new() { Kind = PluginIconKind.Url, Value = url };

    /// <summary>
    /// Creates an icon from a file bundled with the plugin (relative path within the plugin directory).
    /// </summary>
    public static PluginIcon File(string relativePath) => new() { Kind = PluginIconKind.File, Value = relativePath };
}

public enum PluginIconKind
{
    Named,
    Svg,
    Url,
    File
}
