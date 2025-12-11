namespace Ivy.Themes;

public class Theme
{
    public string Name { get; set; } = "Default";

    public ThemeColorScheme Colors { get; set; } = new();

    public string? FontFamily { get; set; }

    public string? FontSize { get; set; }

    public string? BorderRadius { get; set; }

    public static Theme Default => new() { Name = "Default" };
}

public class ThemeColorScheme
{
    public ThemeColors Light { get; set; } = new();
    public ThemeColors Dark { get; set; } = new();
}

public class ThemeColors
{
    // Semantic theme colors
    public string? Primary { get; set; }
    public string? PrimaryForeground { get; set; }
    public string? Secondary { get; set; }
    public string? SecondaryForeground { get; set; }
    public string? Background { get; set; }
    public string? Foreground { get; set; }
    public string? Destructive { get; set; }
    public string? DestructiveForeground { get; set; }
    public string? Success { get; set; }
    public string? SuccessForeground { get; set; }
    public string? Warning { get; set; }
    public string? WarningForeground { get; set; }
    public string? Info { get; set; }
    public string? InfoForeground { get; set; }
    public string? Border { get; set; }
    public string? Input { get; set; }
    public string? Ring { get; set; }
    public string? Muted { get; set; }
    public string? MutedForeground { get; set; }
    public string? Accent { get; set; }
    public string? AccentForeground { get; set; }
    public string? Card { get; set; }
    public string? CardForeground { get; set; }
    public string? Popover { get; set; }
    public string? PopoverForeground { get; set; }
}
