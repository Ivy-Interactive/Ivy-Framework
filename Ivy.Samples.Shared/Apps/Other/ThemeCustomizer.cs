using Ivy.Client;
using Ivy.Core;
using Ivy.Shared;
using Ivy.Themes;
using Ivy.Views;

namespace Ivy.Samples.Shared.Apps.Other;

[App(icon: Icons.Palette, path: ["Other", "UI"])]
public class ThemeCustomizer : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedPreset = UseState("default");
        var currentTheme = UseState(ThemeConfig.Default);
        var showJson = UseState(false);
        var showCode = UseState(false);
        var client = UseService<IClientProvider>();
        var themeService = UseService<IThemeService>();

        var presets = new Dictionary<string, ThemeConfig>
        {
            ["default"] = ThemeConfig.Default,
            ["ocean"] = GetOceanTheme(),
            ["forest"] = GetForestTheme(),
            ["sunset"] = GetSunsetTheme(),
            ["midnight"] = GetMidnightTheme()
        };

        void ApplyTheme()
        {
            try
            {
                // Apply theme directly to the service
                themeService.SetTheme(currentTheme.Value);

                // Generate and apply the CSS to the frontend immediately
                var css = themeService.GenerateThemeCss();
                client.ApplyThemeCss(css);

                client.Toast("Theme applied successfully!", "Success");
            }
            catch (Exception ex)
            {
                client.Toast($"Error: {ex.Message}", "Error");
            }
        }

        return Layout.Vertical()
            | Text.H1("Theme Customizer")
            | Text.Block("Customize and apply themes dynamically to your Ivy application.")
            | new Card(
                Layout.Vertical()
                    | Text.Block("🎨 Live Theme Application")
                    | Text.Small("Select a preset and click 'Apply Selected Theme' to see changes instantly - no page refresh needed!")
            ).BorderColor(Colors.Primary)

            // Preset selector
            | Text.H2("Theme Presets")
            | Layout.Horizontal(
                Text.Block("Select Theme"),
                selectedPreset.ToSelectInput(
                    presets.Select(kv => new Option<string>(kv.Value.Name, kv.Key))
                )
            )

            // Apply button
            | new Button("Apply Selected Theme")
            {
                OnClick = _ =>
                {
                    // Update current theme from selected preset first
                    if (presets.TryGetValue(selectedPreset.Value, out var theme))
                    {
                        currentTheme.Set(theme);
                    }
                    ApplyTheme();
                    return ValueTask.CompletedTask;
                },
                Icon = Icons.Sparkles
            }

            // Theme preview with actual colors
            | Text.H2("Color Preview")
            | new Card(
                Layout.Grid().Columns(2)
                    | RenderColorPreview("Primary", currentTheme.Value.Colors.Primary, currentTheme.Value.Colors.PrimaryForeground)
                    | RenderColorPreview("Secondary", currentTheme.Value.Colors.Secondary, currentTheme.Value.Colors.SecondaryForeground)
                    | RenderColorPreview("Success", currentTheme.Value.Colors.Success, currentTheme.Value.Colors.SuccessForeground)
                    | RenderColorPreview("Destructive", currentTheme.Value.Colors.Destructive, currentTheme.Value.Colors.DestructiveForeground)
                    | RenderColorPreview("Warning", currentTheme.Value.Colors.Warning, currentTheme.Value.Colors.WarningForeground)
                    | RenderColorPreview("Info", currentTheme.Value.Colors.Info, currentTheme.Value.Colors.InfoForeground)
                    | RenderColorPreview("Muted", currentTheme.Value.Colors.Muted, currentTheme.Value.Colors.MutedForeground)
                    | RenderColorPreview("Accent", currentTheme.Value.Colors.Accent, currentTheme.Value.Colors.AccentForeground)
            ).Title("Color Palette")

            // Export options
            | Text.H2("Export Options")
            | Layout.Horizontal(
                new Button("Show C# Code")
                {
                    OnClick = _ =>
                    {
                        showCode.Set(!showCode.Value);
                        showJson.Set(false);
                        return ValueTask.CompletedTask;
                    }
                },
                new Button("Show JSON")
                {
                    OnClick = _ =>
                    {
                        showJson.Set(!showJson.Value);
                        showCode.Set(false);
                        return ValueTask.CompletedTask;
                    }
                }.Variant(ButtonVariant.Secondary),
                                new Button("Copy to Clipboard")
                                {
                                    OnClick = _ =>
                                    {
                                        var content = showCode.Value
                                            ? GenerateCSharpCode(currentTheme.Value)
                                            : System.Text.Json.JsonSerializer.Serialize(currentTheme.Value, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                                        client.CopyToClipboard(content);
                                        client.Toast("Theme configuration copied to clipboard!");
                                        return ValueTask.CompletedTask;
                                    }
                                }.Variant(ButtonVariant.Outline)
            )

            | (showCode.Value ? new Code(GenerateCSharpCode(currentTheme.Value), Languages.Csharp) : null)
            | (showJson.Value ? new Code(System.Text.Json.JsonSerializer.Serialize(currentTheme.Value, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }), Languages.Json) : null)

            // Usage instructions
            | Text.H2("Usage")
            | new Card(
                Layout.Vertical()
                    | Text.H3("Dynamic Theme Application")
                    | Text.Block("Click 'Apply Selected Theme' to instantly apply the theme with live CSS updates - no server restart required!")
                    | Text.H3("Server Configuration")
                    | Text.Block("You can also configure themes at server startup:")
                    | new Code(@"var server = new Server()
    .UseTheme(theme => 
    {
        // Your theme configuration here
    });", Languages.Csharp)
            )
        ;
    }

    private object RenderColorPreview(string label, string? bgColor, string? fgColor)
    {
        var bg = bgColor ?? "#000000";
        var fg = fgColor ?? "#FFFFFF";

        return Layout.Vertical()
            | Text.Small(label)
            | Layout.Horizontal(
                // Color swatch showing actual colors
                new Html($@"<div style='background-color: {bg}; color: {fg}; padding: 20px 30px; border-radius: 8px; text-align: center; font-weight: bold; font-size: 16px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); width: 80px;'>Aa</div>")
                    .Width(Size.Px(140)),
                Layout.Vertical()
                    | Text.InlineCode(bg)
                    | Text.InlineCode(fg)
            );
    }

    private string GenerateCSharpCode(ThemeConfig theme)
    {
        var colors = theme.Colors;
        return $@"// Add this to your server configuration:
var server = new Server()
    .UseTheme(theme => {{
        theme.Name = ""{theme.Name}"";
        theme.Colors = new ThemeColors
        {{
            Primary = ""{colors.Primary}"",
            PrimaryForeground = ""{colors.PrimaryForeground}"",
            Secondary = ""{colors.Secondary}"",
            SecondaryForeground = ""{colors.SecondaryForeground}"",
            Background = ""{colors.Background}"",
            Foreground = ""{colors.Foreground}"",
            Destructive = ""{colors.Destructive}"",
            DestructiveForeground = ""{colors.DestructiveForeground}"",
            Success = ""{colors.Success}"",
            SuccessForeground = ""{colors.SuccessForeground}"",
            Warning = ""{colors.Warning}"",
            WarningForeground = ""{colors.WarningForeground}"",
            Info = ""{colors.Info}"",
            InfoForeground = ""{colors.InfoForeground}"",
            Border = ""{colors.Border}"",
            Input = ""{colors.Input}"",
            Ring = ""{colors.Ring}"",
            Muted = ""{colors.Muted}"",
            MutedForeground = ""{colors.MutedForeground}"",
            Accent = ""{colors.Accent}"",
            AccentForeground = ""{colors.AccentForeground}"",
            Card = ""{colors.Card}"",
            CardForeground = ""{colors.CardForeground}"",
            Popover = ""{colors.Popover}"",
            PopoverForeground = ""{colors.PopoverForeground}"",
            Chart1 = ""{colors.Chart1}"",
            Chart2 = ""{colors.Chart2}"",
            Chart3 = ""{colors.Chart3}"",
            Chart4 = ""{colors.Chart4}"",
            Chart5 = ""{colors.Chart5}"",
            Sidebar = ""{colors.Sidebar}"",
            SidebarForeground = ""{colors.SidebarForeground}"",
            SidebarPrimary = ""{colors.SidebarPrimary}"",
            SidebarPrimaryForeground = ""{colors.SidebarPrimaryForeground}"",
            SidebarAccent = ""{colors.SidebarAccent}"",
            SidebarAccentForeground = ""{colors.SidebarAccentForeground}"",
            SidebarBorder = ""{colors.SidebarBorder}"",
            SidebarRing = ""{colors.SidebarRing}""
        }};
    }});";
    }

    // Theme presets
    private static ThemeConfig GetOceanTheme() => new()
    {
        Name = "Ocean",
        Colors = new ThemeColors
        {
            Primary = "#0077BE",
            PrimaryForeground = "#FFFFFF",
            Secondary = "#5B9BD5",
            SecondaryForeground = "#FFFFFF",
            Background = "#F0F8FF",
            Foreground = "#1A1A1A",
            Destructive = "#DC143C",
            DestructiveForeground = "#FFFFFF",
            Success = "#20B2AA",
            SuccessForeground = "#FFFFFF",
            Warning = "#FFD700",
            WarningForeground = "#1A1A1A",
            Info = "#4682B4",
            InfoForeground = "#FFFFFF",
            Border = "#B0C4DE",
            Input = "#E6F2FF",
            Ring = "#0077BE",
            Muted = "#E0E8F0",
            MutedForeground = "#5A6A7A",
            Accent = "#87CEEB",
            AccentForeground = "#1A1A1A",
            Card = "#FFFFFF",
            CardForeground = "#1A1A1A"
        }
    };

    private static ThemeConfig GetForestTheme() => new()
    {
        Name = "Forest",
        Colors = new ThemeColors
        {
            Primary = "#228B22",
            PrimaryForeground = "#FFFFFF",
            Secondary = "#8FBC8F",
            SecondaryForeground = "#1A1A1A",
            Background = "#F0FFF0",
            Foreground = "#1A1A1A",
            Destructive = "#B22222",
            DestructiveForeground = "#FFFFFF",
            Success = "#32CD32",
            SuccessForeground = "#FFFFFF",
            Warning = "#FFA500",
            WarningForeground = "#1A1A1A",
            Info = "#4169E1",
            InfoForeground = "#FFFFFF",
            Border = "#90EE90",
            Input = "#E8F5E8",
            Ring = "#228B22",
            Muted = "#E0F0E0",
            MutedForeground = "#4A5A4A",
            Accent = "#98FB98",
            AccentForeground = "#1A1A1A",
            Card = "#FFFFFF",
            CardForeground = "#1A1A1A"
        }
    };

    private static ThemeConfig GetSunsetTheme() => new()
    {
        Name = "Sunset",
        Colors = new ThemeColors
        {
            Primary = "#FF6347",
            PrimaryForeground = "#FFFFFF",
            Secondary = "#FFB6C1",
            SecondaryForeground = "#1A1A1A",
            Background = "#FFF5EE",
            Foreground = "#1A1A1A",
            Destructive = "#DC143C",
            DestructiveForeground = "#FFFFFF",
            Success = "#90EE90",
            SuccessForeground = "#1A1A1A",
            Warning = "#FFD700",
            WarningForeground = "#1A1A1A",
            Info = "#87CEEB",
            InfoForeground = "#1A1A1A",
            Border = "#FFE4E1",
            Input = "#FFF0E6",
            Ring = "#FF6347",
            Muted = "#FFDAB9",
            MutedForeground = "#8B4513",
            Accent = "#FFA07A",
            AccentForeground = "#1A1A1A",
            Card = "#FFFFFF",
            CardForeground = "#1A1A1A"
        }
    };

    private static ThemeConfig GetMidnightTheme() => new()
    {
        Name = "Midnight",
        Colors = new ThemeColors
        {
            Primary = "#7C3AED",
            PrimaryForeground = "#FFFFFF",
            Secondary = "#4C1D95",
            SecondaryForeground = "#FFFFFF",
            Background = "#0F0F23",
            Foreground = "#E5E5E5",
            Destructive = "#EF4444",
            DestructiveForeground = "#FFFFFF",
            Success = "#10B981",
            SuccessForeground = "#FFFFFF",
            Warning = "#F59E0B",
            WarningForeground = "#000000",
            Info = "#3B82F6",
            InfoForeground = "#FFFFFF",
            Border = "#374151",
            Input = "#1F2937",
            Ring = "#7C3AED",
            Muted = "#1F2937",
            MutedForeground = "#9CA3AF",
            Accent = "#6366F1",
            AccentForeground = "#FFFFFF",
            Card = "#1A1A2E",
            CardForeground = "#E5E5E5"
        }
    };


}