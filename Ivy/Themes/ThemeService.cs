using System.Text;

namespace Ivy.Themes;

public interface IThemeService
{
    Theme CurrentTheme { get; }

    void SetTheme(Theme theme);

    string GenerateThemeCss();

    string GenerateThemeMetaTag();
}

public class ThemeService : IThemeService
{
    private Theme _currentTheme = Theme.Default;

    public Theme CurrentTheme => _currentTheme;

    public void SetTheme(Theme theme)
    {
        _currentTheme = theme ?? Theme.Default;
    }

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
        else
        {
            // Log warning for missing color values (helps debug theme token issues)
            System.Diagnostics.Debug.WriteLine($"Warning: Theme color '{variableName}' is null or empty. CSS variable will not be set.");
        }
    }
}
