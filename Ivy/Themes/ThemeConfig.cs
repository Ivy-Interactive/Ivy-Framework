namespace Ivy.Themes;

/// <summary>
/// Represents a theme configuration that can be applied to the frontend
/// </summary>
public class ThemeConfig
{
    public string Name { get; set; } = "Default";
    public ThemeColorScheme Colors { get; set; } = new();
    public string? FontFamily { get; set; }
    public string? FontSize { get; set; }
    public string? BorderRadius { get; set; }

    public static ThemeConfig Default => new()
    {
        Name = "Default",
        Colors = ThemeColorScheme.Default
    };
}

/// <summary>
/// Represents the color scheme for a theme, supporting both light and dark variants
/// </summary>
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

    // Popover colors
    public string? Popover { get; set; }
    public string? PopoverForeground { get; set; }

    // Chart colors
    public string? Chart1 { get; set; }
    public string? Chart2 { get; set; }
    public string? Chart3 { get; set; }
    public string? Chart4 { get; set; }
    public string? Chart5 { get; set; }

    // Sidebar colors
    public string? Sidebar { get; set; }
    public string? SidebarForeground { get; set; }
    public string? SidebarPrimary { get; set; }
    public string? SidebarPrimaryForeground { get; set; }
    public string? SidebarAccent { get; set; }
    public string? SidebarAccentForeground { get; set; }
    public string? SidebarBorder { get; set; }
    public string? SidebarRing { get; set; }

    public static ThemeColors DefaultLight => new()
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
        CardForeground = "#262626",

        // Extended colors to match frontend CSS variables
        Popover = "#ffffff",
        PopoverForeground = "#000000",
        Chart1 = "#0077BE",
        Chart2 = "#DC143C",
        Chart3 = "#70AD47",
        Chart4 = "#FFC000",
        Chart5 = "#5B9BD5",
        Sidebar = "#f8f8f8",
        SidebarForeground = "#000000",
        SidebarPrimary = "#00cc92",
        SidebarPrimaryForeground = "#ffffff",
        SidebarAccent = "#f4f6f6",
        SidebarAccentForeground = "#333333",
        SidebarBorder = "#d1d5db",
        SidebarRing = "#777777"
    };

    public static ThemeColors DefaultDark => new()
    {
        Primary = "#00cc92",
        PrimaryForeground = "#18181d",
        Secondary = "#26262b",
        SecondaryForeground = "#f8f8f8",
        Background = "#000000",
        Foreground = "#f8f8f8",
        Destructive = "#dd5860",
        DestructiveForeground = "#f8f8f8",
        Success = "#86d26f",
        SuccessForeground = "#000000",
        Warning = "#deb145",
        WarningForeground = "#000000",
        Info = "#4469c0",
        InfoForeground = "#ffffff",
        Border = "#262626",
        Input = "#262626",
        Ring = "#777777",
        Muted = "#1a1a1a",
        MutedForeground = "#8f8f8f",
        Accent = "#1a1a1a",
        AccentForeground = "#f8f8f8",
        Card = "#171717",
        CardForeground = "#f8f8f8",

        // Extended colors to match frontend CSS variables
        Popover = "#000000",
        PopoverForeground = "#f8f8f8",
        Chart1 = "#0077BE",
        Chart2 = "#DC143C",
        Chart3 = "#70AD47",
        Chart4 = "#FFC000",
        Chart5 = "#5B9BD5",
        Sidebar = "#0a0a0a",
        SidebarForeground = "#f8f8f8",
        SidebarPrimary = "#00cc92",
        SidebarPrimaryForeground = "#000000",
        SidebarAccent = "#1a1a1a",
        SidebarAccentForeground = "#f8f8f8",
        SidebarBorder = "#262626",
        SidebarRing = "#777777"
    };
}
