namespace Ivy.Themes;

/// <summary>Represents a complete theme configuration that can be applied to the frontend.</summary>
public class Theme
{
    public string Name { get; set; } = "Default";

    public ThemeColorScheme Colors { get; set; } = new();

    /// <summary>
    /// Should be a valid CSS font-family value (e.g., "Inter, sans-serif").
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Should be a valid CSS font-size value (e.g., "14px", "1rem").
    /// </summary>
    public string? FontSize { get; set; }

    /// <summary>
    /// Should be a valid CSS border-radius value (e.g., "8px", "0.5rem").
    /// </summary>
    public string? BorderRadius { get; set; }

    public static Theme Default => new()
    {
        Name = "Default",
        Colors = ThemeColorScheme.Default
    };
}

/// <summary>Color scheme supporting both light and dark variants.</summary>
public class ThemeColorScheme
{
    public ThemeColors Light { get; set; } = new();
    public ThemeColors Dark { get; set; } = new();

    public static ThemeColorScheme Default => new()
    {
        Light = ThemeColors.DefaultLight,
        Dark = ThemeColors.DefaultDark
    };
}

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

    // Popover colors
    public string? Popover { get; set; }
    public string? PopoverForeground { get; set; }

    public static ThemeColors DefaultLight => new()
    {
        // Semantic colors from Ivy Design System
        Primary = IvyFrameworkTokens.Color.ColorSemanticPrimaryBase,
        PrimaryForeground = IvyFrameworkTokens.Color.ColorSemanticPrimaryForeground,
        Secondary = IvyFrameworkTokens.Color.ColorSemanticSecondaryBase,
        SecondaryForeground = IvyFrameworkTokens.Color.ColorSemanticSecondaryForeground,
        Background = IvyFrameworkTokens.Color.ColorUiBackgroundBase,
        Foreground = IvyFrameworkTokens.Color.ColorUiBackgroundForeground,
        Destructive = IvyFrameworkTokens.Color.ColorSemanticDestructiveBase,
        DestructiveForeground = IvyFrameworkTokens.Color.ColorSemanticDestructiveForeground,
        Success = IvyFrameworkTokens.Color.ColorSemanticSuccessBase,
        SuccessForeground = IvyFrameworkTokens.Color.ColorSemanticSuccessForeground,
        Warning = IvyFrameworkTokens.Color.ColorSemanticWarningBase,
        WarningForeground = IvyFrameworkTokens.Color.ColorSemanticWarningForeground,
        Info = IvyFrameworkTokens.Color.ColorSemanticInfoBase,
        InfoForeground = IvyFrameworkTokens.Color.ColorSemanticInfoForeground,

        // UI element colors from Ivy Design System
        Border = IvyFrameworkTokens.Color.ColorUiBorder,
        Input = IvyFrameworkTokens.Color.ColorUiInput,
        Ring = IvyFrameworkTokens.Color.ColorUiRing,
        Muted = IvyFrameworkTokens.Color.ColorUiMutedBase,
        MutedForeground = IvyFrameworkTokens.Color.ColorUiMutedForeground,
        Accent = IvyFrameworkTokens.Color.ColorUiAccentBase,
        AccentForeground = IvyFrameworkTokens.Color.ColorUiAccentForeground,
        Card = IvyFrameworkTokens.Color.ColorUiCardBase,
        CardForeground = IvyFrameworkTokens.Color.ColorUiCardForeground,
        Popover = IvyFrameworkTokens.Color.ColorUiPopoverBase,
        PopoverForeground = IvyFrameworkTokens.Color.ColorUiPopoverForeground
    };

    public static ThemeColors DefaultDark => new()
    {
        // Semantic colors from Ivy Design System (Dark Theme)
        Primary = DarkThemeTokens.Theme.ThemeDarkPrimaryBase,
        PrimaryForeground = DarkThemeTokens.Theme.ThemeDarkPrimaryForeground,
        Secondary = DarkThemeTokens.Theme.ThemeDarkSecondaryBase,
        SecondaryForeground = DarkThemeTokens.Theme.ThemeDarkSecondaryForeground,
        Background = DarkThemeTokens.Theme.ThemeDarkBackgroundBase,
        Foreground = DarkThemeTokens.Theme.ThemeDarkBackgroundForeground,
        Destructive = DarkThemeTokens.Theme.ThemeDarkDestructiveBase,
        DestructiveForeground = DarkThemeTokens.Theme.ThemeDarkDestructiveForeground,
        Success = DarkThemeTokens.Theme.ThemeDarkSuccessBase,
        SuccessForeground = DarkThemeTokens.Theme.ThemeDarkSuccessForeground,
        Warning = DarkThemeTokens.Theme.ThemeDarkWarningBase,
        WarningForeground = DarkThemeTokens.Theme.ThemeDarkWarningForeground,
        Info = DarkThemeTokens.Theme.ThemeDarkInfoBase,
        InfoForeground = DarkThemeTokens.Theme.ThemeDarkInfoForeground,

        // UI element colors from Ivy Design System (Dark Theme)
        Border = DarkThemeTokens.Theme.ThemeDarkUiBorder,
        Input = DarkThemeTokens.Theme.ThemeDarkUiInput,
        Ring = DarkThemeTokens.Theme.ThemeDarkUiRing,
        Muted = DarkThemeTokens.Theme.ThemeDarkUiMutedBase,
        MutedForeground = DarkThemeTokens.Theme.ThemeDarkUiMutedForeground,
        Accent = DarkThemeTokens.Theme.ThemeDarkUiAccentBase,
        AccentForeground = DarkThemeTokens.Theme.ThemeDarkUiAccentForeground,
        Card = DarkThemeTokens.Theme.ThemeDarkUiCardBase,
        CardForeground = DarkThemeTokens.Theme.ThemeDarkUiCardForeground,
        Popover = DarkThemeTokens.Theme.ThemeDarkUiPopoverBase,
        PopoverForeground = DarkThemeTokens.Theme.ThemeDarkUiPopoverForeground,
    };
}
