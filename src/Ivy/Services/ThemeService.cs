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
        AppendColorScaleVariables(sb);
        AppendOtherThemeProperties(sb);
        sb.AppendLine("}");

        // Generate .dark theme variables
        sb.AppendLine(".dark {");
        AppendThemeColors(sb, _currentTheme.Colors.Dark, ThemeColors.DefaultDark);
        AppendNeutralColors(sb);
        AppendChromaticColors(sb);
        AppendColorScaleVariables(sb);
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

    private void AppendColorScaleVariables(StringBuilder sb)
    {
        // Red scale
        AppendColorVariable(sb, "--red-50", IvyFrameworkSourceTokens.Color.Red_50);
        AppendColorVariable(sb, "--red-100", IvyFrameworkSourceTokens.Color.Red_100);
        AppendColorVariable(sb, "--red-200", IvyFrameworkSourceTokens.Color.Red_200);
        AppendColorVariable(sb, "--red-300", IvyFrameworkSourceTokens.Color.Red_300);
        AppendColorVariable(sb, "--red-400", IvyFrameworkSourceTokens.Color.Red_400);
        AppendColorVariable(sb, "--red-500", IvyFrameworkSourceTokens.Color.Red_500);
        AppendColorVariable(sb, "--red-600", IvyFrameworkSourceTokens.Color.Red_600);
        AppendColorVariable(sb, "--red-700", IvyFrameworkSourceTokens.Color.Red_700);
        AppendColorVariable(sb, "--red-800", IvyFrameworkSourceTokens.Color.Red_800);
        AppendColorVariable(sb, "--red-900", IvyFrameworkSourceTokens.Color.Red_900);
        AppendColorVariable(sb, "--red-950", IvyFrameworkSourceTokens.Color.Red_950);

        // Orange scale
        AppendColorVariable(sb, "--orange-50", IvyFrameworkSourceTokens.Color.Orange_50);
        AppendColorVariable(sb, "--orange-100", IvyFrameworkSourceTokens.Color.Orange_100);
        AppendColorVariable(sb, "--orange-200", IvyFrameworkSourceTokens.Color.Orange_200);
        AppendColorVariable(sb, "--orange-300", IvyFrameworkSourceTokens.Color.Orange_300);
        AppendColorVariable(sb, "--orange-400", IvyFrameworkSourceTokens.Color.Orange_400);
        AppendColorVariable(sb, "--orange-500", IvyFrameworkSourceTokens.Color.Orange_500);
        AppendColorVariable(sb, "--orange-600", IvyFrameworkSourceTokens.Color.Orange_600);
        AppendColorVariable(sb, "--orange-700", IvyFrameworkSourceTokens.Color.Orange_700);
        AppendColorVariable(sb, "--orange-800", IvyFrameworkSourceTokens.Color.Orange_800);
        AppendColorVariable(sb, "--orange-900", IvyFrameworkSourceTokens.Color.Orange_900);
        AppendColorVariable(sb, "--orange-950", IvyFrameworkSourceTokens.Color.Orange_950);

        // Amber scale
        AppendColorVariable(sb, "--amber-50", IvyFrameworkSourceTokens.Color.Amber_50);
        AppendColorVariable(sb, "--amber-100", IvyFrameworkSourceTokens.Color.Amber_100);
        AppendColorVariable(sb, "--amber-200", IvyFrameworkSourceTokens.Color.Amber_200);
        AppendColorVariable(sb, "--amber-300", IvyFrameworkSourceTokens.Color.Amber_300);
        AppendColorVariable(sb, "--amber-400", IvyFrameworkSourceTokens.Color.Amber_400);
        AppendColorVariable(sb, "--amber-500", IvyFrameworkSourceTokens.Color.Amber_500);
        AppendColorVariable(sb, "--amber-600", IvyFrameworkSourceTokens.Color.Amber_600);
        AppendColorVariable(sb, "--amber-700", IvyFrameworkSourceTokens.Color.Amber_700);
        AppendColorVariable(sb, "--amber-800", IvyFrameworkSourceTokens.Color.Amber_800);
        AppendColorVariable(sb, "--amber-900", IvyFrameworkSourceTokens.Color.Amber_900);
        AppendColorVariable(sb, "--amber-950", IvyFrameworkSourceTokens.Color.Amber_950);

        // Yellow scale
        AppendColorVariable(sb, "--yellow-50", IvyFrameworkSourceTokens.Color.Yellow_50);
        AppendColorVariable(sb, "--yellow-100", IvyFrameworkSourceTokens.Color.Yellow_100);
        AppendColorVariable(sb, "--yellow-200", IvyFrameworkSourceTokens.Color.Yellow_200);
        AppendColorVariable(sb, "--yellow-300", IvyFrameworkSourceTokens.Color.Yellow_300);
        AppendColorVariable(sb, "--yellow-400", IvyFrameworkSourceTokens.Color.Yellow_400);
        AppendColorVariable(sb, "--yellow-500", IvyFrameworkSourceTokens.Color.Yellow_500);
        AppendColorVariable(sb, "--yellow-600", IvyFrameworkSourceTokens.Color.Yellow_600);
        AppendColorVariable(sb, "--yellow-700", IvyFrameworkSourceTokens.Color.Yellow_700);
        AppendColorVariable(sb, "--yellow-800", IvyFrameworkSourceTokens.Color.Yellow_800);
        AppendColorVariable(sb, "--yellow-900", IvyFrameworkSourceTokens.Color.Yellow_900);
        AppendColorVariable(sb, "--yellow-950", IvyFrameworkSourceTokens.Color.Yellow_950);

        // Lime scale
        AppendColorVariable(sb, "--lime-50", IvyFrameworkSourceTokens.Color.Lime_50);
        AppendColorVariable(sb, "--lime-100", IvyFrameworkSourceTokens.Color.Lime_100);
        AppendColorVariable(sb, "--lime-200", IvyFrameworkSourceTokens.Color.Lime_200);
        AppendColorVariable(sb, "--lime-300", IvyFrameworkSourceTokens.Color.Lime_300);
        AppendColorVariable(sb, "--lime-400", IvyFrameworkSourceTokens.Color.Lime_400);
        AppendColorVariable(sb, "--lime-500", IvyFrameworkSourceTokens.Color.Lime_500);
        AppendColorVariable(sb, "--lime-600", IvyFrameworkSourceTokens.Color.Lime_600);
        AppendColorVariable(sb, "--lime-700", IvyFrameworkSourceTokens.Color.Lime_700);
        AppendColorVariable(sb, "--lime-800", IvyFrameworkSourceTokens.Color.Lime_800);
        AppendColorVariable(sb, "--lime-900", IvyFrameworkSourceTokens.Color.Lime_900);
        AppendColorVariable(sb, "--lime-950", IvyFrameworkSourceTokens.Color.Lime_950);

        // Green scale
        AppendColorVariable(sb, "--green-50", IvyFrameworkSourceTokens.Color.Green_50);
        AppendColorVariable(sb, "--green-100", IvyFrameworkSourceTokens.Color.Green_100);
        AppendColorVariable(sb, "--green-200", IvyFrameworkSourceTokens.Color.Green_200);
        AppendColorVariable(sb, "--green-300", IvyFrameworkSourceTokens.Color.Green_300);
        AppendColorVariable(sb, "--green-400", IvyFrameworkSourceTokens.Color.Green_400);
        AppendColorVariable(sb, "--green-500", IvyFrameworkSourceTokens.Color.Green_500);
        AppendColorVariable(sb, "--green-600", IvyFrameworkSourceTokens.Color.Green_600);
        AppendColorVariable(sb, "--green-700", IvyFrameworkSourceTokens.Color.Green_700);
        AppendColorVariable(sb, "--green-800", IvyFrameworkSourceTokens.Color.Green_800);
        AppendColorVariable(sb, "--green-900", IvyFrameworkSourceTokens.Color.Green_900);
        AppendColorVariable(sb, "--green-950", IvyFrameworkSourceTokens.Color.Green_950);

        // Emerald scale
        AppendColorVariable(sb, "--emerald-50", IvyFrameworkSourceTokens.Color.Emerald_50);
        AppendColorVariable(sb, "--emerald-100", IvyFrameworkSourceTokens.Color.Emerald_100);
        AppendColorVariable(sb, "--emerald-200", IvyFrameworkSourceTokens.Color.Emerald_200);
        AppendColorVariable(sb, "--emerald-300", IvyFrameworkSourceTokens.Color.Emerald_300);
        AppendColorVariable(sb, "--emerald-400", IvyFrameworkSourceTokens.Color.Emerald_400);
        AppendColorVariable(sb, "--emerald-500", IvyFrameworkSourceTokens.Color.Emerald_500);
        AppendColorVariable(sb, "--emerald-600", IvyFrameworkSourceTokens.Color.Emerald_600);
        AppendColorVariable(sb, "--emerald-700", IvyFrameworkSourceTokens.Color.Emerald_700);
        AppendColorVariable(sb, "--emerald-800", IvyFrameworkSourceTokens.Color.Emerald_800);
        AppendColorVariable(sb, "--emerald-900", IvyFrameworkSourceTokens.Color.Emerald_900);
        AppendColorVariable(sb, "--emerald-950", IvyFrameworkSourceTokens.Color.Emerald_950);

        // Teal scale
        AppendColorVariable(sb, "--teal-50", IvyFrameworkSourceTokens.Color.Teal_50);
        AppendColorVariable(sb, "--teal-100", IvyFrameworkSourceTokens.Color.Teal_100);
        AppendColorVariable(sb, "--teal-200", IvyFrameworkSourceTokens.Color.Teal_200);
        AppendColorVariable(sb, "--teal-300", IvyFrameworkSourceTokens.Color.Teal_300);
        AppendColorVariable(sb, "--teal-400", IvyFrameworkSourceTokens.Color.Teal_400);
        AppendColorVariable(sb, "--teal-500", IvyFrameworkSourceTokens.Color.Teal_500);
        AppendColorVariable(sb, "--teal-600", IvyFrameworkSourceTokens.Color.Teal_600);
        AppendColorVariable(sb, "--teal-700", IvyFrameworkSourceTokens.Color.Teal_700);
        AppendColorVariable(sb, "--teal-800", IvyFrameworkSourceTokens.Color.Teal_800);
        AppendColorVariable(sb, "--teal-900", IvyFrameworkSourceTokens.Color.Teal_900);
        AppendColorVariable(sb, "--teal-950", IvyFrameworkSourceTokens.Color.Teal_950);

        // Cyan scale
        AppendColorVariable(sb, "--cyan-50", IvyFrameworkSourceTokens.Color.Cyan_50);
        AppendColorVariable(sb, "--cyan-100", IvyFrameworkSourceTokens.Color.Cyan_100);
        AppendColorVariable(sb, "--cyan-200", IvyFrameworkSourceTokens.Color.Cyan_200);
        AppendColorVariable(sb, "--cyan-300", IvyFrameworkSourceTokens.Color.Cyan_300);
        AppendColorVariable(sb, "--cyan-400", IvyFrameworkSourceTokens.Color.Cyan_400);
        AppendColorVariable(sb, "--cyan-500", IvyFrameworkSourceTokens.Color.Cyan_500);
        AppendColorVariable(sb, "--cyan-600", IvyFrameworkSourceTokens.Color.Cyan_600);
        AppendColorVariable(sb, "--cyan-700", IvyFrameworkSourceTokens.Color.Cyan_700);
        AppendColorVariable(sb, "--cyan-800", IvyFrameworkSourceTokens.Color.Cyan_800);
        AppendColorVariable(sb, "--cyan-900", IvyFrameworkSourceTokens.Color.Cyan_900);
        AppendColorVariable(sb, "--cyan-950", IvyFrameworkSourceTokens.Color.Cyan_950);

        // Sky scale
        AppendColorVariable(sb, "--sky-50", IvyFrameworkSourceTokens.Color.Sky_50);
        AppendColorVariable(sb, "--sky-100", IvyFrameworkSourceTokens.Color.Sky_100);
        AppendColorVariable(sb, "--sky-200", IvyFrameworkSourceTokens.Color.Sky_200);
        AppendColorVariable(sb, "--sky-300", IvyFrameworkSourceTokens.Color.Sky_300);
        AppendColorVariable(sb, "--sky-400", IvyFrameworkSourceTokens.Color.Sky_400);
        AppendColorVariable(sb, "--sky-500", IvyFrameworkSourceTokens.Color.Sky_500);
        AppendColorVariable(sb, "--sky-600", IvyFrameworkSourceTokens.Color.Sky_600);
        AppendColorVariable(sb, "--sky-700", IvyFrameworkSourceTokens.Color.Sky_700);
        AppendColorVariable(sb, "--sky-800", IvyFrameworkSourceTokens.Color.Sky_800);
        AppendColorVariable(sb, "--sky-900", IvyFrameworkSourceTokens.Color.Sky_900);
        AppendColorVariable(sb, "--sky-950", IvyFrameworkSourceTokens.Color.Sky_950);

        // Blue scale
        AppendColorVariable(sb, "--blue-50", IvyFrameworkSourceTokens.Color.Blue_50);
        AppendColorVariable(sb, "--blue-100", IvyFrameworkSourceTokens.Color.Blue_100);
        AppendColorVariable(sb, "--blue-200", IvyFrameworkSourceTokens.Color.Blue_200);
        AppendColorVariable(sb, "--blue-300", IvyFrameworkSourceTokens.Color.Blue_300);
        AppendColorVariable(sb, "--blue-400", IvyFrameworkSourceTokens.Color.Blue_400);
        AppendColorVariable(sb, "--blue-500", IvyFrameworkSourceTokens.Color.Blue_500);
        AppendColorVariable(sb, "--blue-600", IvyFrameworkSourceTokens.Color.Blue_600);
        AppendColorVariable(sb, "--blue-700", IvyFrameworkSourceTokens.Color.Blue_700);
        AppendColorVariable(sb, "--blue-800", IvyFrameworkSourceTokens.Color.Blue_800);
        AppendColorVariable(sb, "--blue-900", IvyFrameworkSourceTokens.Color.Blue_900);
        AppendColorVariable(sb, "--blue-950", IvyFrameworkSourceTokens.Color.Blue_950);

        // Indigo scale
        AppendColorVariable(sb, "--indigo-50", IvyFrameworkSourceTokens.Color.Indigo_50);
        AppendColorVariable(sb, "--indigo-100", IvyFrameworkSourceTokens.Color.Indigo_100);
        AppendColorVariable(sb, "--indigo-200", IvyFrameworkSourceTokens.Color.Indigo_200);
        AppendColorVariable(sb, "--indigo-300", IvyFrameworkSourceTokens.Color.Indigo_300);
        AppendColorVariable(sb, "--indigo-400", IvyFrameworkSourceTokens.Color.Indigo_400);
        AppendColorVariable(sb, "--indigo-500", IvyFrameworkSourceTokens.Color.Indigo_500);
        AppendColorVariable(sb, "--indigo-600", IvyFrameworkSourceTokens.Color.Indigo_600);
        AppendColorVariable(sb, "--indigo-700", IvyFrameworkSourceTokens.Color.Indigo_700);
        AppendColorVariable(sb, "--indigo-800", IvyFrameworkSourceTokens.Color.Indigo_800);
        AppendColorVariable(sb, "--indigo-900", IvyFrameworkSourceTokens.Color.Indigo_900);
        AppendColorVariable(sb, "--indigo-950", IvyFrameworkSourceTokens.Color.Indigo_950);

        // Violet scale
        AppendColorVariable(sb, "--violet-50", IvyFrameworkSourceTokens.Color.Violet_50);
        AppendColorVariable(sb, "--violet-100", IvyFrameworkSourceTokens.Color.Violet_100);
        AppendColorVariable(sb, "--violet-200", IvyFrameworkSourceTokens.Color.Violet_200);
        AppendColorVariable(sb, "--violet-300", IvyFrameworkSourceTokens.Color.Violet_300);
        AppendColorVariable(sb, "--violet-400", IvyFrameworkSourceTokens.Color.Violet_400);
        AppendColorVariable(sb, "--violet-500", IvyFrameworkSourceTokens.Color.Violet_500);
        AppendColorVariable(sb, "--violet-600", IvyFrameworkSourceTokens.Color.Violet_600);
        AppendColorVariable(sb, "--violet-700", IvyFrameworkSourceTokens.Color.Violet_700);
        AppendColorVariable(sb, "--violet-800", IvyFrameworkSourceTokens.Color.Violet_800);
        AppendColorVariable(sb, "--violet-900", IvyFrameworkSourceTokens.Color.Violet_900);
        AppendColorVariable(sb, "--violet-950", IvyFrameworkSourceTokens.Color.Violet_950);

        // Purple scale
        AppendColorVariable(sb, "--purple-50", IvyFrameworkSourceTokens.Color.Purple_50);
        AppendColorVariable(sb, "--purple-100", IvyFrameworkSourceTokens.Color.Purple_100);
        AppendColorVariable(sb, "--purple-200", IvyFrameworkSourceTokens.Color.Purple_200);
        AppendColorVariable(sb, "--purple-300", IvyFrameworkSourceTokens.Color.Purple_300);
        AppendColorVariable(sb, "--purple-400", IvyFrameworkSourceTokens.Color.Purple_400);
        AppendColorVariable(sb, "--purple-500", IvyFrameworkSourceTokens.Color.Purple_500);
        AppendColorVariable(sb, "--purple-600", IvyFrameworkSourceTokens.Color.Purple_600);
        AppendColorVariable(sb, "--purple-700", IvyFrameworkSourceTokens.Color.Purple_700);
        AppendColorVariable(sb, "--purple-800", IvyFrameworkSourceTokens.Color.Purple_800);
        AppendColorVariable(sb, "--purple-900", IvyFrameworkSourceTokens.Color.Purple_900);
        AppendColorVariable(sb, "--purple-950", IvyFrameworkSourceTokens.Color.Purple_950);

        // Fuchsia scale
        AppendColorVariable(sb, "--fuchsia-50", IvyFrameworkSourceTokens.Color.Fuchsia_50);
        AppendColorVariable(sb, "--fuchsia-100", IvyFrameworkSourceTokens.Color.Fuchsia_100);
        AppendColorVariable(sb, "--fuchsia-200", IvyFrameworkSourceTokens.Color.Fuchsia_200);
        AppendColorVariable(sb, "--fuchsia-300", IvyFrameworkSourceTokens.Color.Fuchsia_300);
        AppendColorVariable(sb, "--fuchsia-400", IvyFrameworkSourceTokens.Color.Fuchsia_400);
        AppendColorVariable(sb, "--fuchsia-500", IvyFrameworkSourceTokens.Color.Fuchsia_500);
        AppendColorVariable(sb, "--fuchsia-600", IvyFrameworkSourceTokens.Color.Fuchsia_600);
        AppendColorVariable(sb, "--fuchsia-700", IvyFrameworkSourceTokens.Color.Fuchsia_700);
        AppendColorVariable(sb, "--fuchsia-800", IvyFrameworkSourceTokens.Color.Fuchsia_800);
        AppendColorVariable(sb, "--fuchsia-900", IvyFrameworkSourceTokens.Color.Fuchsia_900);
        AppendColorVariable(sb, "--fuchsia-950", IvyFrameworkSourceTokens.Color.Fuchsia_950);

        // Pink scale
        AppendColorVariable(sb, "--pink-50", IvyFrameworkSourceTokens.Color.Pink_50);
        AppendColorVariable(sb, "--pink-100", IvyFrameworkSourceTokens.Color.Pink_100);
        AppendColorVariable(sb, "--pink-200", IvyFrameworkSourceTokens.Color.Pink_200);
        AppendColorVariable(sb, "--pink-300", IvyFrameworkSourceTokens.Color.Pink_300);
        AppendColorVariable(sb, "--pink-400", IvyFrameworkSourceTokens.Color.Pink_400);
        AppendColorVariable(sb, "--pink-500", IvyFrameworkSourceTokens.Color.Pink_500);
        AppendColorVariable(sb, "--pink-600", IvyFrameworkSourceTokens.Color.Pink_600);
        AppendColorVariable(sb, "--pink-700", IvyFrameworkSourceTokens.Color.Pink_700);
        AppendColorVariable(sb, "--pink-800", IvyFrameworkSourceTokens.Color.Pink_800);
        AppendColorVariable(sb, "--pink-900", IvyFrameworkSourceTokens.Color.Pink_900);
        AppendColorVariable(sb, "--pink-950", IvyFrameworkSourceTokens.Color.Pink_950);

        // Rose scale
        AppendColorVariable(sb, "--rose-50", IvyFrameworkSourceTokens.Color.Rose_50);
        AppendColorVariable(sb, "--rose-100", IvyFrameworkSourceTokens.Color.Rose_100);
        AppendColorVariable(sb, "--rose-200", IvyFrameworkSourceTokens.Color.Rose_200);
        AppendColorVariable(sb, "--rose-300", IvyFrameworkSourceTokens.Color.Rose_300);
        AppendColorVariable(sb, "--rose-400", IvyFrameworkSourceTokens.Color.Rose_400);
        AppendColorVariable(sb, "--rose-500", IvyFrameworkSourceTokens.Color.Rose_500);
        AppendColorVariable(sb, "--rose-600", IvyFrameworkSourceTokens.Color.Rose_600);
        AppendColorVariable(sb, "--rose-700", IvyFrameworkSourceTokens.Color.Rose_700);
        AppendColorVariable(sb, "--rose-800", IvyFrameworkSourceTokens.Color.Rose_800);
        AppendColorVariable(sb, "--rose-900", IvyFrameworkSourceTokens.Color.Rose_900);
        AppendColorVariable(sb, "--rose-950", IvyFrameworkSourceTokens.Color.Rose_950);

        // Slate scale
        AppendColorVariable(sb, "--slate-50", IvyFrameworkSourceTokens.Color.Slate_50);
        AppendColorVariable(sb, "--slate-100", IvyFrameworkSourceTokens.Color.Slate_100);
        AppendColorVariable(sb, "--slate-200", IvyFrameworkSourceTokens.Color.Slate_200);
        AppendColorVariable(sb, "--slate-300", IvyFrameworkSourceTokens.Color.Slate_300);
        AppendColorVariable(sb, "--slate-400", IvyFrameworkSourceTokens.Color.Slate_400);
        AppendColorVariable(sb, "--slate-500", IvyFrameworkSourceTokens.Color.Slate_500);
        AppendColorVariable(sb, "--slate-600", IvyFrameworkSourceTokens.Color.Slate_600);
        AppendColorVariable(sb, "--slate-700", IvyFrameworkSourceTokens.Color.Slate_700);
        AppendColorVariable(sb, "--slate-800", IvyFrameworkSourceTokens.Color.Slate_800);
        AppendColorVariable(sb, "--slate-900", IvyFrameworkSourceTokens.Color.Slate_900);
        AppendColorVariable(sb, "--slate-950", IvyFrameworkSourceTokens.Color.Slate_950);

        // Gray scale
        AppendColorVariable(sb, "--gray-50", IvyFrameworkSourceTokens.Color.Gray_50);
        AppendColorVariable(sb, "--gray-100", IvyFrameworkSourceTokens.Color.Gray_100);
        AppendColorVariable(sb, "--gray-200", IvyFrameworkSourceTokens.Color.Gray_200);
        AppendColorVariable(sb, "--gray-300", IvyFrameworkSourceTokens.Color.Gray_300);
        AppendColorVariable(sb, "--gray-400", IvyFrameworkSourceTokens.Color.Gray_400);
        AppendColorVariable(sb, "--gray-500", IvyFrameworkSourceTokens.Color.Gray_500);
        AppendColorVariable(sb, "--gray-600", IvyFrameworkSourceTokens.Color.Gray_600);
        AppendColorVariable(sb, "--gray-700", IvyFrameworkSourceTokens.Color.Gray_700);
        AppendColorVariable(sb, "--gray-800", IvyFrameworkSourceTokens.Color.Gray_800);
        AppendColorVariable(sb, "--gray-900", IvyFrameworkSourceTokens.Color.Gray_900);
        AppendColorVariable(sb, "--gray-950", IvyFrameworkSourceTokens.Color.Gray_950);

        // Zinc scale
        AppendColorVariable(sb, "--zinc-50", IvyFrameworkSourceTokens.Color.Zinc_50);
        AppendColorVariable(sb, "--zinc-100", IvyFrameworkSourceTokens.Color.Zinc_100);
        AppendColorVariable(sb, "--zinc-200", IvyFrameworkSourceTokens.Color.Zinc_200);
        AppendColorVariable(sb, "--zinc-300", IvyFrameworkSourceTokens.Color.Zinc_300);
        AppendColorVariable(sb, "--zinc-400", IvyFrameworkSourceTokens.Color.Zinc_400);
        AppendColorVariable(sb, "--zinc-500", IvyFrameworkSourceTokens.Color.Zinc_500);
        AppendColorVariable(sb, "--zinc-600", IvyFrameworkSourceTokens.Color.Zinc_600);
        AppendColorVariable(sb, "--zinc-700", IvyFrameworkSourceTokens.Color.Zinc_700);
        AppendColorVariable(sb, "--zinc-800", IvyFrameworkSourceTokens.Color.Zinc_800);
        AppendColorVariable(sb, "--zinc-900", IvyFrameworkSourceTokens.Color.Zinc_900);
        AppendColorVariable(sb, "--zinc-950", IvyFrameworkSourceTokens.Color.Zinc_950);

        // Neutral scale
        AppendColorVariable(sb, "--neutral-50", IvyFrameworkSourceTokens.Color.Neutral_50);
        AppendColorVariable(sb, "--neutral-100", IvyFrameworkSourceTokens.Color.Neutral_100);
        AppendColorVariable(sb, "--neutral-200", IvyFrameworkSourceTokens.Color.Neutral_200);
        AppendColorVariable(sb, "--neutral-300", IvyFrameworkSourceTokens.Color.Neutral_300);
        AppendColorVariable(sb, "--neutral-400", IvyFrameworkSourceTokens.Color.Neutral_400);
        AppendColorVariable(sb, "--neutral-500", IvyFrameworkSourceTokens.Color.Neutral_500);
        AppendColorVariable(sb, "--neutral-600", IvyFrameworkSourceTokens.Color.Neutral_600);
        AppendColorVariable(sb, "--neutral-700", IvyFrameworkSourceTokens.Color.Neutral_700);
        AppendColorVariable(sb, "--neutral-800", IvyFrameworkSourceTokens.Color.Neutral_800);
        AppendColorVariable(sb, "--neutral-900", IvyFrameworkSourceTokens.Color.Neutral_900);
        AppendColorVariable(sb, "--neutral-950", IvyFrameworkSourceTokens.Color.Neutral_950);

        // Stone scale
        AppendColorVariable(sb, "--stone-50", IvyFrameworkSourceTokens.Color.Stone_50);
        AppendColorVariable(sb, "--stone-100", IvyFrameworkSourceTokens.Color.Stone_100);
        AppendColorVariable(sb, "--stone-200", IvyFrameworkSourceTokens.Color.Stone_200);
        AppendColorVariable(sb, "--stone-300", IvyFrameworkSourceTokens.Color.Stone_300);
        AppendColorVariable(sb, "--stone-400", IvyFrameworkSourceTokens.Color.Stone_400);
        AppendColorVariable(sb, "--stone-500", IvyFrameworkSourceTokens.Color.Stone_500);
        AppendColorVariable(sb, "--stone-600", IvyFrameworkSourceTokens.Color.Stone_600);
        AppendColorVariable(sb, "--stone-700", IvyFrameworkSourceTokens.Color.Stone_700);
        AppendColorVariable(sb, "--stone-800", IvyFrameworkSourceTokens.Color.Stone_800);
        AppendColorVariable(sb, "--stone-900", IvyFrameworkSourceTokens.Color.Stone_900);
        AppendColorVariable(sb, "--stone-950", IvyFrameworkSourceTokens.Color.Stone_950);
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

        // Shadow tokens from design system, controlled per element group
        sb.AppendLine($"  --shadow-boxes: {(_currentTheme.ShadowBoxes ? IvyFrameworkShadowTokens.Shadow.Sm : "none")};");
        sb.AppendLine($"  --shadow-fields: {(_currentTheme.ShadowFields ? IvyFrameworkShadowTokens.Shadow.Sm : "none")};");
        sb.AppendLine($"  --shadow-selectors: {(_currentTheme.ShadowSelectors ? IvyFrameworkShadowTokens.Shadow.Sm : "none")};");

        // Emit full shadow scale from design system (replacing hardcoded values in index.css)
        sb.AppendLine($"  --shadow-sm: {IvyFrameworkShadowTokens.Shadow.Sm};");
        sb.AppendLine($"  --shadow-md: {IvyFrameworkShadowTokens.Shadow.Md};");
        sb.AppendLine($"  --shadow-lg: {IvyFrameworkShadowTokens.Shadow.Lg};");
    }

    private void AppendColorVariable(StringBuilder sb, string variableName, string? colorValue)
    {
        if (!string.IsNullOrEmpty(colorValue))
            sb.AppendLine($"  {variableName}: {colorValue};");
    }
}
