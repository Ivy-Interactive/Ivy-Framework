using System.Text;
using Ivy.Themes;

// Resharper disable once CheckNamespace
namespace Ivy;

public interface IThemeService
{
    Theme CurrentTheme { get; }

    Func<Theme>? ThemeFactory { get; }

    void SetTheme(Theme theme);

    void SetThemeFactory(Func<Theme> factory);

    void ReloadTheme();

    string GenerateThemeCss();

    string GenerateThemeMetaTag();
}

public class ThemeService : IThemeService
{
    private Theme _currentTheme = Theme.Default;
    private Func<Theme>? _themeFactory;

    public Theme CurrentTheme => _currentTheme;

    public Func<Theme>? ThemeFactory => _themeFactory;

    public void SetTheme(Theme theme)
    {
        _currentTheme = theme ?? Theme.Default;
    }

    public void SetThemeFactory(Func<Theme> factory)
    {
        _themeFactory = factory;
    }

    public void ReloadTheme()
    {
        if (_themeFactory == null) return;
        SetTheme(_themeFactory());
    }

    public string GenerateThemeCss()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<style id=\"ivy-custom-theme\">");

        // Generate :root (light theme) variables
        sb.AppendLine(":root {");
        AppendThemeColors(sb, _currentTheme.Colors.Light, ThemeColors.DefaultLight);
        AppendNeutralColors(sb);
        AppendChromaticColors(sb);
        AppendOtherThemeProperties(sb);
        sb.AppendLine("}");

        // Generate .dark theme variables
        sb.AppendLine(".dark {");
        AppendThemeColors(sb, _currentTheme.Colors.Dark, ThemeColors.DefaultDark);
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

    private void AppendThemeColors(StringBuilder sb, ThemeColors colors, ThemeColors defaults)
    {
        // Main theme colors
        AppendColorVariable(sb, "--primary", colors.Primary ?? defaults.Primary);
        AppendColorVariable(sb, "--primary-foreground", colors.PrimaryForeground ?? defaults.PrimaryForeground);
        AppendColorVariable(sb, "--secondary", colors.Secondary ?? defaults.Secondary);
        AppendColorVariable(sb, "--secondary-foreground", colors.SecondaryForeground ?? defaults.SecondaryForeground);
        AppendColorVariable(sb, "--background", colors.Background ?? defaults.Background);
        AppendColorVariable(sb, "--foreground", colors.Foreground ?? defaults.Foreground);

        // Semantic colors
        AppendColorVariable(sb, "--destructive", colors.Destructive ?? defaults.Destructive);
        AppendColorVariable(sb, "--destructive-foreground", colors.DestructiveForeground ?? defaults.DestructiveForeground);
        AppendColorVariable(sb, "--success", colors.Success ?? defaults.Success);
        AppendColorVariable(sb, "--success-foreground", colors.SuccessForeground ?? defaults.SuccessForeground);
        AppendColorVariable(sb, "--warning", colors.Warning ?? defaults.Warning);
        AppendColorVariable(sb, "--warning-foreground", colors.WarningForeground ?? defaults.WarningForeground);
        AppendColorVariable(sb, "--info", colors.Info ?? defaults.Info);
        AppendColorVariable(sb, "--info-foreground", colors.InfoForeground ?? defaults.InfoForeground);

        // UI element colors
        AppendColorVariable(sb, "--border", colors.Border ?? defaults.Border);
        AppendColorVariable(sb, "--input", colors.Input ?? defaults.Input);
        AppendColorVariable(sb, "--ring", colors.Ring ?? defaults.Ring);
        AppendColorVariable(sb, "--muted", colors.Muted ?? defaults.Muted);
        AppendColorVariable(sb, "--muted-foreground", colors.MutedForeground ?? defaults.MutedForeground);
        AppendColorVariable(sb, "--accent", colors.Accent ?? defaults.Accent);
        AppendColorVariable(sb, "--accent-foreground", colors.AccentForeground ?? defaults.AccentForeground);
        AppendColorVariable(sb, "--card", colors.Card ?? defaults.Card);
        AppendColorVariable(sb, "--card-foreground", colors.CardForeground ?? defaults.CardForeground);

        // Popover colors
        AppendColorVariable(sb, "--popover", colors.Popover ?? defaults.Popover);
        AppendColorVariable(sb, "--popover-foreground", colors.PopoverForeground ?? defaults.PopoverForeground);

        // Ivy green for logo and branding elements
        AppendColorVariable(sb, "--ivy-green", colors.IvyGreen ?? defaults.IvyGreen);
        AppendColorVariable(sb, "--ivy-green-foreground", colors.IvyGreenForeground ?? defaults.IvyGreenForeground);
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
        if (!string.IsNullOrEmpty(_currentTheme.FontFamily))
            sb.AppendLine($"  --font-sans: {_currentTheme.FontFamily};");

        if (!string.IsNullOrEmpty(_currentTheme.FontSize))
            sb.AppendLine($"  --text-body: {_currentTheme.FontSize};");

        if (!string.IsNullOrEmpty(_currentTheme.BorderRadiusBoxes))
            sb.AppendLine($"  --radius-boxes: {_currentTheme.BorderRadiusBoxes};");

        if (!string.IsNullOrEmpty(_currentTheme.BorderRadiusFields))
            sb.AppendLine($"  --radius-fields: {_currentTheme.BorderRadiusFields};");

        if (!string.IsNullOrEmpty(_currentTheme.BorderRadiusSelectors))
            sb.AppendLine($"  --radius-selectors: {_currentTheme.BorderRadiusSelectors};");
    }

    private void AppendColorVariable(StringBuilder sb, string variableName, string? colorValue)
    {
        if (!string.IsNullOrEmpty(colorValue))
            sb.AppendLine($"  {variableName}: {colorValue};");
    }
}
