using System.Text;

namespace Ivy.Themes;

/// <summary>
/// Service for managing and applying themes
/// </summary>
public interface IThemeService
{
    ThemeConfig CurrentTheme { get; }
    void SetTheme(ThemeConfig theme);
    string GenerateThemeCss();
    string GenerateThemeMetaTag();
}

public class ThemeService : IThemeService
{
    private ThemeConfig _currentTheme = ThemeConfig.Default;

    public ThemeConfig CurrentTheme => _currentTheme;

    public void SetTheme(ThemeConfig theme)
    {
        _currentTheme = theme ?? ThemeConfig.Default;
    }

    /// <summary>
    /// Generates CSS variables for the theme that will be injected into the page
    /// </summary>
    public string GenerateThemeCss()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<style id=\"ivy-custom-theme\">");

        // Generate :root (light theme) variables
        sb.AppendLine(":root {");
        AppendThemeColors(sb, _currentTheme.Colors.Light);
        AppendOtherThemeProperties(sb);
        sb.AppendLine("}");

        // Generate .dark theme variables
        sb.AppendLine(".dark {");
        AppendThemeColors(sb, _currentTheme.Colors.Dark);
        sb.AppendLine("}");

        sb.AppendLine("</style>");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a meta tag containing the theme configuration for frontend access
    /// </summary>
    public string GenerateThemeMetaTag()
    {
        var themeJson = System.Text.Json.JsonSerializer.Serialize(_currentTheme);
        var encodedTheme = System.Web.HttpUtility.HtmlEncode(themeJson);
        return $"<meta name=\"ivy-theme\" content=\"{encodedTheme}\" />";
    }

    private void AppendThemeColors(StringBuilder sb, ThemeColors colors)
    {
        // Main theme colors
        AppendColorVariable(sb, "--primary", colors.Primary);
        AppendColorVariable(sb, "--primary-foreground", colors.PrimaryForeground);
        AppendColorVariable(sb, "--secondary", colors.Secondary);
        AppendColorVariable(sb, "--secondary-foreground", colors.SecondaryForeground);
        AppendColorVariable(sb, "--background", colors.Background);
        AppendColorVariable(sb, "--foreground", colors.Foreground);

        // Semantic colors
        AppendColorVariable(sb, "--destructive", colors.Destructive);
        AppendColorVariable(sb, "--destructive-foreground", colors.DestructiveForeground);
        AppendColorVariable(sb, "--success", colors.Success);
        AppendColorVariable(sb, "--success-foreground", colors.SuccessForeground);
        AppendColorVariable(sb, "--warning", colors.Warning);
        AppendColorVariable(sb, "--warning-foreground", colors.WarningForeground);
        AppendColorVariable(sb, "--info", colors.Info);
        AppendColorVariable(sb, "--info-foreground", colors.InfoForeground);

        // UI element colors
        AppendColorVariable(sb, "--border", colors.Border);
        AppendColorVariable(sb, "--input", colors.Input);
        AppendColorVariable(sb, "--ring", colors.Ring);
        AppendColorVariable(sb, "--muted", colors.Muted);
        AppendColorVariable(sb, "--muted-foreground", colors.MutedForeground);
        AppendColorVariable(sb, "--accent", colors.Accent);
        AppendColorVariable(sb, "--accent-foreground", colors.AccentForeground);
        AppendColorVariable(sb, "--card", colors.Card);
        AppendColorVariable(sb, "--card-foreground", colors.CardForeground);

        // Popover colors
        AppendColorVariable(sb, "--popover", colors.Popover);
        AppendColorVariable(sb, "--popover-foreground", colors.PopoverForeground);

        // Chart colors
        AppendColorVariable(sb, "--chart-1", colors.Chart1);
        AppendColorVariable(sb, "--chart-2", colors.Chart2);
        AppendColorVariable(sb, "--chart-3", colors.Chart3);
        AppendColorVariable(sb, "--chart-4", colors.Chart4);
        AppendColorVariable(sb, "--chart-5", colors.Chart5);

        // Sidebar colors
        AppendColorVariable(sb, "--sidebar", colors.Sidebar);
        AppendColorVariable(sb, "--sidebar-foreground", colors.SidebarForeground);
        AppendColorVariable(sb, "--sidebar-primary", colors.SidebarPrimary);
        AppendColorVariable(sb, "--sidebar-primary-foreground", colors.SidebarPrimaryForeground);
        AppendColorVariable(sb, "--sidebar-accent", colors.SidebarAccent);
        AppendColorVariable(sb, "--sidebar-accent-foreground", colors.SidebarAccentForeground);
        AppendColorVariable(sb, "--sidebar-border", colors.SidebarBorder);
        AppendColorVariable(sb, "--sidebar-ring", colors.SidebarRing);
    }

    private void AppendOtherThemeProperties(StringBuilder sb)
    {
        // Apply other theme properties only to :root
        if (!string.IsNullOrEmpty(_currentTheme.FontFamily))
            sb.AppendLine($"  --font-sans: {_currentTheme.FontFamily};");

        if (!string.IsNullOrEmpty(_currentTheme.FontSize))
            sb.AppendLine($"  --text-body: {_currentTheme.FontSize};");

        if (!string.IsNullOrEmpty(_currentTheme.BorderRadius))
            sb.AppendLine($"  --radius: {_currentTheme.BorderRadius};");
    }

    private void AppendColorVariable(StringBuilder sb, string variableName, string? colorValue)
    {
        if (!string.IsNullOrEmpty(colorValue))
        {
            sb.AppendLine($"  {variableName}: {colorValue};");
        }
    }
}
