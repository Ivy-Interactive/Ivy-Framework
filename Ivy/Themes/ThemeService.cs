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
        AppendAllTokens(sb, isLightTheme: true);
        AppendOtherThemeProperties(sb);
        sb.AppendLine("}");

        // Generate .dark theme variables
        sb.AppendLine(".dark {");
        AppendAllTokens(sb, isLightTheme: false);
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

    private void AppendAllTokens(StringBuilder sb, bool isLightTheme)
    {
        // Get all tokens from design system
        var themeTokens = isLightTheme
            ? IvyFrameworkLightThemeTokens.GetAllTokens()
            : IvyFrameworkDarkThemeTokens.GetAllTokens();
        var neutralTokens = IvyFrameworkNeutralTokens.GetAllTokens();
        var chromaticTokens = IvyFrameworkChromaticTokens.GetAllTokens();

        // Get custom overrides
        var customColors = isLightTheme ? _currentTheme.Colors.Light : _currentTheme.Colors.Dark;

        // Append theme tokens (primary, secondary, background, etc.)
        // Custom overrides take precedence over design system defaults
        foreach (var (name, value) in themeTokens)
        {
            var cssVarName = ToCssVariableName(name);
            var customValue = GetCustomColorValue(customColors, name);
            sb.AppendLine($"  {cssVarName}: {customValue ?? value};");
        }

        // Append neutral tokens (black, white, slate, gray, etc.)
        foreach (var (name, value) in neutralTokens)
        {
            var cssVarName = ToCssVariableName(name);
            sb.AppendLine($"  {cssVarName}: {value};");
        }

        // Append chromatic tokens (red, orange, amber, etc.)
        foreach (var (name, value) in chromaticTokens)
        {
            var cssVarName = ToCssVariableName(name);
            sb.AppendLine($"  {cssVarName}: {value};");
        }
    }

    private static string? GetCustomColorValue(ThemeColors colors, string tokenName)
    {
        return tokenName switch
        {
            "Primary" => colors.Primary,
            "PrimaryForeground" => colors.PrimaryForeground,
            "Secondary" => colors.Secondary,
            "SecondaryForeground" => colors.SecondaryForeground,
            "Background" => colors.Background,
            "Foreground" => colors.Foreground,
            "Destructive" => colors.Destructive,
            "DestructiveForeground" => colors.DestructiveForeground,
            "Success" => colors.Success,
            "SuccessForeground" => colors.SuccessForeground,
            "Warning" => colors.Warning,
            "WarningForeground" => colors.WarningForeground,
            "Info" => colors.Info,
            "InfoForeground" => colors.InfoForeground,
            "Border" => colors.Border,
            "Input" => colors.Input,
            "Ring" => colors.Ring,
            "Muted" => colors.Muted,
            "MutedForeground" => colors.MutedForeground,
            "Accent" => colors.Accent,
            "AccentForeground" => colors.AccentForeground,
            "Card" => colors.Card,
            "CardForeground" => colors.CardForeground,
            "Popover" => colors.Popover,
            "PopoverForeground" => colors.PopoverForeground,
            _ => null
        };
    }

    private static string ToCssVariableName(string tokenName)
    {
        // Convert PascalCase token names to kebab-case CSS variable names
        // e.g., "Primary" -> "--primary", "PrimaryForeground" -> "--primary-foreground"
        var sb = new StringBuilder("--");
        for (int i = 0; i < tokenName.Length; i++)
        {
            var c = tokenName[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
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
}
