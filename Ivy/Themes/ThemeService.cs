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
        sb.AppendLine(":root {");

        // Apply color overrides
        if (_currentTheme.Colors != null)
        {
            AppendColorVariable(sb, "--primary", _currentTheme.Colors.Primary);
            AppendColorVariable(sb, "--primary-foreground", _currentTheme.Colors.PrimaryForeground);
            AppendColorVariable(sb, "--secondary", _currentTheme.Colors.Secondary);
            AppendColorVariable(sb, "--secondary-foreground", _currentTheme.Colors.SecondaryForeground);
            AppendColorVariable(sb, "--background", _currentTheme.Colors.Background);
            AppendColorVariable(sb, "--foreground", _currentTheme.Colors.Foreground);
            AppendColorVariable(sb, "--destructive", _currentTheme.Colors.Destructive);
            AppendColorVariable(sb, "--destructive-foreground", _currentTheme.Colors.DestructiveForeground);
            AppendColorVariable(sb, "--success", _currentTheme.Colors.Success);
            AppendColorVariable(sb, "--success-foreground", _currentTheme.Colors.SuccessForeground);
            AppendColorVariable(sb, "--warning", _currentTheme.Colors.Warning);
            AppendColorVariable(sb, "--warning-foreground", _currentTheme.Colors.WarningForeground);
            AppendColorVariable(sb, "--info", _currentTheme.Colors.Info);
            AppendColorVariable(sb, "--info-foreground", _currentTheme.Colors.InfoForeground);
            AppendColorVariable(sb, "--border", _currentTheme.Colors.Border);
            AppendColorVariable(sb, "--input", _currentTheme.Colors.Input);
            AppendColorVariable(sb, "--ring", _currentTheme.Colors.Ring);
            AppendColorVariable(sb, "--muted", _currentTheme.Colors.Muted);
            AppendColorVariable(sb, "--muted-foreground", _currentTheme.Colors.MutedForeground);
            AppendColorVariable(sb, "--accent", _currentTheme.Colors.Accent);
            AppendColorVariable(sb, "--accent-foreground", _currentTheme.Colors.AccentForeground);
            AppendColorVariable(sb, "--card", _currentTheme.Colors.Card);
            AppendColorVariable(sb, "--card-foreground", _currentTheme.Colors.CardForeground);
        }

        // Apply other theme properties
        if (!string.IsNullOrEmpty(_currentTheme.FontFamily))
            sb.AppendLine($"  --font-sans: {_currentTheme.FontFamily};");

        if (!string.IsNullOrEmpty(_currentTheme.FontSize))
            sb.AppendLine($"  --text-body: {_currentTheme.FontSize};");

        if (!string.IsNullOrEmpty(_currentTheme.BorderRadius))
            sb.AppendLine($"  --radius: {_currentTheme.BorderRadius};");

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

    private void AppendColorVariable(StringBuilder sb, string variableName, string? colorValue)
    {
        if (!string.IsNullOrEmpty(colorValue))
        {
            sb.AppendLine($"  {variableName}: {colorValue};");
        }
    }
}
