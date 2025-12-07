namespace Ivy.Themes;

public class Theme
{
    public string Name { get; set; } = "Default";

    public ThemeColorScheme Colors { get; set; } = new();

    public string? FontFamily { get; set; }

    public string? FontSize { get; set; }

    public string? BorderRadius { get; set; }

    public static Theme Default => new()
    {
        Name = "Default",
        Colors = ThemeColorScheme.Default
    };
}

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

    public static ThemeColors DefaultLight => new()
    {
        Primary = LightThemeTokens.Color.Primary,
        PrimaryForeground = LightThemeTokens.Color.PrimaryForeground,
        Secondary = LightThemeTokens.Color.Secondary,
        SecondaryForeground = LightThemeTokens.Color.SecondaryForeground,
        Background = LightThemeTokens.Color.Background,
        Foreground = LightThemeTokens.Color.Foreground,
        Destructive = LightThemeTokens.Color.Destructive,
        DestructiveForeground = LightThemeTokens.Color.DestructiveForeground,
        Success = LightThemeTokens.Color.Success,
        SuccessForeground = LightThemeTokens.Color.SuccessForeground,
        Warning = LightThemeTokens.Color.Warning,
        WarningForeground = LightThemeTokens.Color.WarningForeground,
        Info = LightThemeTokens.Color.Info,
        InfoForeground = LightThemeTokens.Color.InfoForeground,
        Border = LightThemeTokens.Color.Border,
        Input = LightThemeTokens.Color.Input,
        Ring = LightThemeTokens.Color.Ring,
        Muted = LightThemeTokens.Color.Muted,
        MutedForeground = LightThemeTokens.Color.MutedForeground,
        Accent = LightThemeTokens.Color.Accent,
        AccentForeground = LightThemeTokens.Color.AccentForeground,
        Card = LightThemeTokens.Color.Card,
        CardForeground = LightThemeTokens.Color.CardForeground,
        Popover = LightThemeTokens.Color.Popover,
        PopoverForeground = LightThemeTokens.Color.PopoverForeground
    };

    public static ThemeColors DefaultDark => new()
    {
        Primary = DarkThemeTokens.Color.Primary,
        PrimaryForeground = DarkThemeTokens.Color.PrimaryForeground,
        Secondary = DarkThemeTokens.Color.Secondary,
        SecondaryForeground = DarkThemeTokens.Color.SecondaryForeground,
        Background = DarkThemeTokens.Color.Background,
        Foreground = DarkThemeTokens.Color.Foreground,
        Destructive = DarkThemeTokens.Color.Destructive,
        DestructiveForeground = DarkThemeTokens.Color.DestructiveForeground,
        Success = DarkThemeTokens.Color.Success,
        SuccessForeground = DarkThemeTokens.Color.SuccessForeground,
        Warning = DarkThemeTokens.Color.Warning,
        WarningForeground = DarkThemeTokens.Color.WarningForeground,
        Info = DarkThemeTokens.Color.Info,
        InfoForeground = DarkThemeTokens.Color.InfoForeground,
        Border = DarkThemeTokens.Color.Border,
        Input = DarkThemeTokens.Color.Input,
        Ring = DarkThemeTokens.Color.Ring,
        Muted = DarkThemeTokens.Color.Muted,
        MutedForeground = DarkThemeTokens.Color.MutedForeground,
        Accent = DarkThemeTokens.Color.Accent,
        AccentForeground = DarkThemeTokens.Color.AccentForeground,
        Card = DarkThemeTokens.Color.Card,
        CardForeground = DarkThemeTokens.Color.CardForeground,
        Popover = DarkThemeTokens.Color.Popover,
        PopoverForeground = DarkThemeTokens.Color.PopoverForeground
    };
}
