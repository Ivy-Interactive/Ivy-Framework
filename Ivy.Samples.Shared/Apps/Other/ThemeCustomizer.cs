using System.Text.Json;
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
        var showJson = UseState(false);
        var showCode = UseState(false);
        var client = UseService<IClientProvider>();

        var presets = new Dictionary<string, ThemeConfig>
        {
            ["default"] = ThemeConfig.Default,
            ["ocean"] = GetOceanTheme(),
            ["forest"] = GetForestTheme(),
            ["sunset"] = GetSunsetTheme(),
            ["midnight"] = GetMidnightTheme()
        };

        var currentTheme = presets[selectedPreset.Value];

        return Layout.Vertical()
            | Text.H1("Theme Customizer")
            | Text.Block("Explore different theme configurations for your Ivy application.")
            | Text.Small("Note: To apply a theme, configure it in your server startup code and restart the application.")

            // Preset selector
            | Text.H2("Theme Presets")
            | Text.Block("Select Theme")
            | selectedPreset.ToSelectInput(
                presets.Select(kv => new Option<string>(kv.Value.Name, kv.Key))
            )

            // Theme preview
            | Text.H2("Theme Configuration")
            | new Card(
                Layout.Grid().Columns(2)
                    | RenderColorPreview("Primary", currentTheme.Colors.Primary, currentTheme.Colors.PrimaryForeground)
                    | RenderColorPreview("Secondary", currentTheme.Colors.Secondary, currentTheme.Colors.SecondaryForeground)
                    | RenderColorPreview("Success", currentTheme.Colors.Success, currentTheme.Colors.SuccessForeground)
                    | RenderColorPreview("Destructive", currentTheme.Colors.Destructive, currentTheme.Colors.DestructiveForeground)
                    | RenderColorPreview("Warning", currentTheme.Colors.Warning, currentTheme.Colors.WarningForeground)
                    | RenderColorPreview("Info", currentTheme.Colors.Info, currentTheme.Colors.InfoForeground)
                    | RenderColorPreview("Muted", currentTheme.Colors.Muted, currentTheme.Colors.MutedForeground)
                    | RenderColorPreview("Accent", currentTheme.Colors.Accent, currentTheme.Colors.AccentForeground)
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
                            ? GenerateCSharpCode(currentTheme)
                            : JsonSerializer.Serialize(currentTheme, new JsonSerializerOptions { WriteIndented = true });
                        client.CopyToClipboard(content);
                        client.Toast("Theme configuration copied to clipboard!");
                        return ValueTask.CompletedTask;
                    }
                }.Variant(ButtonVariant.Outline)
            )

            | (showCode.Value ? new Code(GenerateCSharpCode(currentTheme), Languages.Csharp) : null)
            | (showJson.Value ? new Code(JsonSerializer.Serialize(currentTheme, new JsonSerializerOptions { WriteIndented = true }), Languages.Json) : null)

            // Usage instructions
            | Text.H2("How to Use")
            | Text.Block("To apply a theme to your Ivy application:")
            | Layout.Vertical()
                | Text.Block("1. Copy the configuration code above")
                | Text.Block("2. Add it to your server startup:")
                | new Code(@"var server = new Server()
    .UseTheme(theme => 
    {
        // Your theme configuration here
    });", Languages.Csharp)
                | Text.Block("3. Restart your application to see the changes")
        ;
    }

    private object RenderColorPreview(string label, string? bgColor, string? fgColor)
    {
        return Layout.Vertical()
            | Text.Small(label)
            | Layout.Horizontal(
                // Color swatch - using DemoBox as a simple container
                new DemoBox(Text.Block("Aa"))
                {
                    Padding = new Thickness(15, 10),
                    BorderRadius = BorderRadius.Rounded,
                    BorderThickness = new Thickness(1)
                },
                Layout.Vertical()
                    | Text.InlineCode(bgColor ?? "#000000")
                    | Text.InlineCode(fgColor ?? "#FFFFFF")
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
            CardForeground = ""{colors.CardForeground}""
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
