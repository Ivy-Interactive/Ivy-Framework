namespace Ivy.Themes;

/// <summary>
/// Represents a theme configuration that can be applied to the frontend
/// </summary>
public class ThemeConfig
{
    public string Name { get; set; } = "Default";
    public ThemeColors Colors { get; set; } = new();
    public string? FontFamily { get; set; }
    public string? FontSize { get; set; }
    public string? BorderRadius { get; set; }

    public static ThemeConfig Default => new()
    {
        Name = "Default",
        Colors = ThemeColors.Default
    };
}

/// <summary>
/// Represents the color palette for a theme
/// </summary>
public class ThemeColors
{
    // Main theme colors
    public string? Primary { get; set; }
    public string? PrimaryForeground { get; set; }
    public string? Secondary { get; set; }
    public string? SecondaryForeground { get; set; }
    public string? Background { get; set; }
    public string? Foreground { get; set; }

    // Semantic colors
    public string? Destructive { get; set; }
    public string? DestructiveForeground { get; set; }
    public string? Success { get; set; }
    public string? SuccessForeground { get; set; }
    public string? Warning { get; set; }
    public string? WarningForeground { get; set; }
    public string? Info { get; set; }
    public string? InfoForeground { get; set; }

    // UI element colors
    public string? Border { get; set; }
    public string? Input { get; set; }
    public string? Ring { get; set; }
    public string? Muted { get; set; }
    public string? MutedForeground { get; set; }
    public string? Accent { get; set; }
    public string? AccentForeground { get; set; }
    public string? Card { get; set; }
    public string? CardForeground { get; set; }

    public static ThemeColors Default => new()
    {
        Primary = "#00cc92",
        PrimaryForeground = "#000000",
        Secondary = "#dfe7e3",
        SecondaryForeground = "#000000",
        Background = "#ffffff",
        Foreground = "#000000",
        Destructive = "#dd5860",
        DestructiveForeground = "#000000",
        Success = "#86d26f",
        SuccessForeground = "#000000",
        Warning = "#deb145",
        WarningForeground = "#000000",
        Info = "#4469c0",
        InfoForeground = "#ffffff",
        Border = "#d1d5db",
        Input = "#d1d5db",
        Ring = "#777777",
        Muted = "#f8f8f8",
        MutedForeground = "#8f8f8f",
        Accent = "#f8f8f8",
        AccentForeground = "#333333",
        Card = "#ffffff",
        CardForeground = "#262626"
    };
}
