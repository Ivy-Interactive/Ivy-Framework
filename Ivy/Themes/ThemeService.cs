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
        AppendNeutralColors(sb);
        AppendChromaticColors(sb);
        AppendOtherThemeProperties(sb);
        sb.AppendLine("}");

        // Generate .dark theme variables
        sb.AppendLine(".dark {");
        AppendThemeColors(sb, _currentTheme.Colors.Dark);
        AppendNeutralColors(sb);
        AppendChromaticColors(sb);
        sb.AppendLine("}");

        sb.AppendLine("</style>");
        return sb.ToString();
    }

    public string GenerateThemeMetaTag()
    {
        var themeJson = System.Text.Json.JsonSerializer.Serialize(_currentTheme, ThemeJsonContext.Default.Theme);
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

    private void AppendNeutralColors(StringBuilder sb)
    {
        // Neutral colors with foreground variants
        AppendColorVariable(sb, "--slate", IvyFrameworkNeutralTokens.Color.Slate);
        AppendColorVariable(sb, "--slate-foreground", IvyFrameworkNeutralTokens.Color.SlateForeground);
        AppendColorVariable(sb, "--gray", IvyFrameworkNeutralTokens.Color.Gray);
        AppendColorVariable(sb, "--gray-foreground", IvyFrameworkNeutralTokens.Color.GrayForeground);
        AppendColorVariable(sb, "--zinc", IvyFrameworkNeutralTokens.Color.Zinc);
        AppendColorVariable(sb, "--zinc-foreground", IvyFrameworkNeutralTokens.Color.ZincForeground);
        AppendColorVariable(sb, "--neutral", IvyFrameworkNeutralTokens.Color.Neutral);
        AppendColorVariable(sb, "--neutral-foreground", IvyFrameworkNeutralTokens.Color.NeutralForeground);
        AppendColorVariable(sb, "--stone", IvyFrameworkNeutralTokens.Color.Stone);
        AppendColorVariable(sb, "--stone-foreground", IvyFrameworkNeutralTokens.Color.StoneForeground);
        AppendColorVariable(sb, "--black", IvyFrameworkNeutralTokens.Color.Black);
        AppendColorVariable(sb, "--black-foreground", IvyFrameworkNeutralTokens.Color.BlackForeground);
        AppendColorVariable(sb, "--white", IvyFrameworkNeutralTokens.Color.White);
        AppendColorVariable(sb, "--white-foreground", IvyFrameworkNeutralTokens.Color.WhiteForeground);
    }

    private void AppendChromaticColors(StringBuilder sb)
    {
        // Chromatic colors with foreground variants
        AppendColorVariable(sb, "--red", IvyFrameworkChromaticTokens.Color.Red);
        AppendColorVariable(sb, "--red-foreground", IvyFrameworkChromaticTokens.Color.RedForeground);
        AppendColorVariable(sb, "--orange", IvyFrameworkChromaticTokens.Color.Orange);
        AppendColorVariable(sb, "--orange-foreground", IvyFrameworkChromaticTokens.Color.OrangeForeground);
        AppendColorVariable(sb, "--amber", IvyFrameworkChromaticTokens.Color.Amber);
        AppendColorVariable(sb, "--amber-foreground", IvyFrameworkChromaticTokens.Color.AmberForeground);
        AppendColorVariable(sb, "--yellow", IvyFrameworkChromaticTokens.Color.Yellow);
        AppendColorVariable(sb, "--yellow-foreground", IvyFrameworkChromaticTokens.Color.YellowForeground);
        AppendColorVariable(sb, "--lime", IvyFrameworkChromaticTokens.Color.Lime);
        AppendColorVariable(sb, "--lime-foreground", IvyFrameworkChromaticTokens.Color.LimeForeground);
        AppendColorVariable(sb, "--green", IvyFrameworkChromaticTokens.Color.Green);
        AppendColorVariable(sb, "--green-foreground", IvyFrameworkChromaticTokens.Color.GreenForeground);
        AppendColorVariable(sb, "--emerald", IvyFrameworkChromaticTokens.Color.Emerald);
        AppendColorVariable(sb, "--emerald-foreground", IvyFrameworkChromaticTokens.Color.EmeraldForeground);
        AppendColorVariable(sb, "--teal", IvyFrameworkChromaticTokens.Color.Teal);
        AppendColorVariable(sb, "--teal-foreground", IvyFrameworkChromaticTokens.Color.TealForeground);
        AppendColorVariable(sb, "--cyan", IvyFrameworkChromaticTokens.Color.Cyan);
        AppendColorVariable(sb, "--cyan-foreground", IvyFrameworkChromaticTokens.Color.CyanForeground);
        AppendColorVariable(sb, "--sky", IvyFrameworkChromaticTokens.Color.Sky);
        AppendColorVariable(sb, "--sky-foreground", IvyFrameworkChromaticTokens.Color.SkyForeground);
        AppendColorVariable(sb, "--blue", IvyFrameworkChromaticTokens.Color.Blue);
        AppendColorVariable(sb, "--blue-foreground", IvyFrameworkChromaticTokens.Color.BlueForeground);
        AppendColorVariable(sb, "--indigo", IvyFrameworkChromaticTokens.Color.Indigo);
        AppendColorVariable(sb, "--indigo-foreground", IvyFrameworkChromaticTokens.Color.IndigoForeground);
        AppendColorVariable(sb, "--violet", IvyFrameworkChromaticTokens.Color.Violet);
        AppendColorVariable(sb, "--violet-foreground", IvyFrameworkChromaticTokens.Color.VioletForeground);
        AppendColorVariable(sb, "--purple", IvyFrameworkChromaticTokens.Color.Purple);
        AppendColorVariable(sb, "--purple-foreground", IvyFrameworkChromaticTokens.Color.PurpleForeground);
        AppendColorVariable(sb, "--fuchsia", IvyFrameworkChromaticTokens.Color.Fuchsia);
        AppendColorVariable(sb, "--fuchsia-foreground", IvyFrameworkChromaticTokens.Color.FuchsiaForeground);
        AppendColorVariable(sb, "--pink", IvyFrameworkChromaticTokens.Color.Pink);
        AppendColorVariable(sb, "--pink-foreground", IvyFrameworkChromaticTokens.Color.PinkForeground);
        AppendColorVariable(sb, "--rose", IvyFrameworkChromaticTokens.Color.Rose);
        AppendColorVariable(sb, "--rose-foreground", IvyFrameworkChromaticTokens.Color.RoseForeground);
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
            sb.AppendLine($"  {variableName}: {colorValue};");
    }
}
