namespace Ivy.IvyML;

/// <summary>
/// Turns the CLI's <c>--theme</c> option into a <see cref="Theme"/>, including the implicit case:
/// a <c>theme.yaml</c> sitting next to the IvyML file is picked up on its own. That is what lets a
/// wireframe library carry its own look without every command repeating the option.
/// </summary>
public static class ThemeResolver
{
    private static readonly string[] ConventionFileNames = ["theme.yaml", "theme.yml"];

    /// <summary>Value of <c>--theme</c> that suppresses the <c>theme.yaml</c> convention.</summary>
    public const string None = "none";

    /// <summary>
    /// Returns null when no theme applies, which leaves the framework default in place.
    /// Throws with a human-readable message when an explicitly requested theme cannot be loaded.
    /// </summary>
    /// <param name="disabled">
    /// Turns theming off outright: neither <paramref name="themeOption"/> nor a neighbouring
    /// <c>theme.yaml</c> is applied. This is what <c>--no-theme</c> sets, and it wins over an
    /// explicit theme so a themed command can be re-run unthemed without editing it.
    /// </param>
    public static Theme? Resolve(string? themeOption, string? ivymlFilePath, bool disabled = false)
    {
        if (disabled) return null;

        if (!string.IsNullOrWhiteSpace(themeOption))
        {
            return string.Equals(themeOption.Trim(), None, StringComparison.OrdinalIgnoreCase)
                ? null
                : ThemeLoader.Load(themeOption.Trim());
        }

        var conventionPath = FindConventionFile(ivymlFilePath);
        return conventionPath is null ? null : ThemeLoader.Load(conventionPath);
    }

    private static string? FindConventionFile(string? ivymlFilePath)
    {
        if (string.IsNullOrWhiteSpace(ivymlFilePath)) return null;

        var directory = Path.GetDirectoryName(Path.GetFullPath(ivymlFilePath));
        if (string.IsNullOrEmpty(directory)) return null;

        return ConventionFileNames
            .Select(name => Path.Combine(directory, name))
            .FirstOrDefault(File.Exists);
    }
}
