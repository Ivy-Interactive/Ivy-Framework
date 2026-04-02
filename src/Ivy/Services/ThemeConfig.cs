using System.Text.Json.Serialization;
using Ivy.Themes;

// Resharper disable once CheckNamespace
namespace Ivy;

[JsonSerializable(typeof(Theme))]
internal partial class ThemeJsonContext : JsonSerializerContext;

public class Theme
{
    public string Name { get; set; } = "Default";

    public ThemeColorScheme Colors { get; set; } = new();

    public string? FontFamily { get; set; }

    public string? FontSize { get; set; }

    public string? BorderRadiusBoxes { get; set; }

    public string? BorderRadiusFields { get; set; }

    public string? BorderRadiusSelectors { get; set; }

    public static Theme Default => new()
    {
        Name = "Default",
        Colors = ThemeColorScheme.Default,
        FontFamily = "Geist",
        FontSize = "16px",
        BorderRadiusBoxes = IvyFrameworkBorderRadiusTokens.BorderRadius.Md,
        BorderRadiusFields = IvyFrameworkBorderRadiusTokens.BorderRadius.Md,
        BorderRadiusSelectors = IvyFrameworkBorderRadiusTokens.BorderRadius.Md
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
        Primary = IvyFrameworkLightThemeTokens.Color.Primary,
        PrimaryForeground = IvyFrameworkLightThemeTokens.Color.PrimaryForeground,
        Secondary = IvyFrameworkLightThemeTokens.Color.Secondary,
        SecondaryForeground = IvyFrameworkLightThemeTokens.Color.SecondaryForeground,
        Background = IvyFrameworkLightThemeTokens.Color.Background,
        Foreground = IvyFrameworkLightThemeTokens.Color.Foreground,
        Destructive = IvyFrameworkLightThemeTokens.Color.Destructive,
        DestructiveForeground = IvyFrameworkLightThemeTokens.Color.DestructiveForeground,
        Success = IvyFrameworkLightThemeTokens.Color.Success,
        SuccessForeground = IvyFrameworkLightThemeTokens.Color.SuccessForeground,
        Warning = IvyFrameworkLightThemeTokens.Color.Warning,
        WarningForeground = IvyFrameworkLightThemeTokens.Color.WarningForeground,
        Info = IvyFrameworkLightThemeTokens.Color.Info,
        InfoForeground = IvyFrameworkLightThemeTokens.Color.InfoForeground,
        Border = IvyFrameworkLightThemeTokens.Color.Border,
        Input = IvyFrameworkLightThemeTokens.Color.Input,
        Ring = IvyFrameworkLightThemeTokens.Color.Ring,
        Muted = IvyFrameworkLightThemeTokens.Color.Muted,
        MutedForeground = IvyFrameworkLightThemeTokens.Color.MutedForeground,
        Accent = IvyFrameworkLightThemeTokens.Color.Accent,
        AccentForeground = IvyFrameworkLightThemeTokens.Color.AccentForeground,
        Card = IvyFrameworkLightThemeTokens.Color.Card,
        CardForeground = IvyFrameworkLightThemeTokens.Color.CardForeground,
        Popover = IvyFrameworkLightThemeTokens.Color.Popover,
        PopoverForeground = IvyFrameworkLightThemeTokens.Color.PopoverForeground,
    };

    public static ThemeColors DefaultDark => new()
    {
        Primary = IvyFrameworkDarkThemeTokens.Color.Primary,
        PrimaryForeground = IvyFrameworkDarkThemeTokens.Color.PrimaryForeground,
        Secondary = IvyFrameworkDarkThemeTokens.Color.Secondary,
        SecondaryForeground = IvyFrameworkDarkThemeTokens.Color.SecondaryForeground,
        Background = IvyFrameworkDarkThemeTokens.Color.Background,
        Foreground = IvyFrameworkDarkThemeTokens.Color.Foreground,
        Destructive = IvyFrameworkDarkThemeTokens.Color.Destructive,
        DestructiveForeground = IvyFrameworkDarkThemeTokens.Color.DestructiveForeground,
        Success = IvyFrameworkDarkThemeTokens.Color.Success,
        SuccessForeground = IvyFrameworkDarkThemeTokens.Color.SuccessForeground,
        Warning = IvyFrameworkDarkThemeTokens.Color.Warning,
        WarningForeground = IvyFrameworkDarkThemeTokens.Color.WarningForeground,
        Info = IvyFrameworkDarkThemeTokens.Color.Info,
        InfoForeground = IvyFrameworkDarkThemeTokens.Color.InfoForeground,
        Border = IvyFrameworkDarkThemeTokens.Color.Border,
        Input = IvyFrameworkDarkThemeTokens.Color.Input,
        Ring = IvyFrameworkDarkThemeTokens.Color.Ring,
        Muted = IvyFrameworkDarkThemeTokens.Color.Muted,
        MutedForeground = IvyFrameworkDarkThemeTokens.Color.MutedForeground,
        Accent = IvyFrameworkDarkThemeTokens.Color.Accent,
        AccentForeground = IvyFrameworkDarkThemeTokens.Color.AccentForeground,
        Card = IvyFrameworkDarkThemeTokens.Color.Card,
        CardForeground = IvyFrameworkDarkThemeTokens.Color.CardForeground,
        Popover = IvyFrameworkDarkThemeTokens.Color.Popover,
        PopoverForeground = IvyFrameworkDarkThemeTokens.Color.PopoverForeground,
    };
}
