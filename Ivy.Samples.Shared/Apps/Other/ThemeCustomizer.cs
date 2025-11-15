using Ivy.Client;
using Ivy.Core;
using Ivy.Shared;
using Ivy.Themes;
using Ivy.Views;

namespace Ivy.Samples.Shared.Apps.Other;

[App(icon: Icons.Palette, path: ["Other", "UI"], searchHints: ["theming", "customization", "branding", "styling", "appearance", "design"])]
public class ThemeCustomizer : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedPreset = UseState("default");
        var currentTheme = UseState(Theme.Default);
        var showJson = UseState(false);
        var showCode = UseState(false);
        var client = UseService<IClientProvider>();
        var themeService = UseService<IThemeService>();

        var presets = new Dictionary<string, Theme>
        {
            ["default"] = Theme.Default,
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
                client.ApplyTheme(css);

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
            | Layout.Horizontal(
                // Light theme colors
                new Card(
                    Layout.Grid().Columns(1)
                        | RenderColorPreview("Primary", currentTheme.Value.Colors.Light.Primary, currentTheme.Value.Colors.Light.PrimaryForeground)
                        | RenderColorPreview("Secondary", currentTheme.Value.Colors.Light.Secondary, currentTheme.Value.Colors.Light.SecondaryForeground)
                        | RenderColorPreview("Success", currentTheme.Value.Colors.Light.Success, currentTheme.Value.Colors.Light.SuccessForeground)
                        | RenderColorPreview("Destructive", currentTheme.Value.Colors.Light.Destructive, currentTheme.Value.Colors.Light.DestructiveForeground)
                        | RenderColorPreview("Warning", currentTheme.Value.Colors.Light.Warning, currentTheme.Value.Colors.Light.WarningForeground)
                        | RenderColorPreview("Info", currentTheme.Value.Colors.Light.Info, currentTheme.Value.Colors.Light.InfoForeground)
                        | RenderColorPreview("Muted", currentTheme.Value.Colors.Light.Muted, currentTheme.Value.Colors.Light.MutedForeground)
                        | RenderColorPreview("Accent", currentTheme.Value.Colors.Light.Accent, currentTheme.Value.Colors.Light.AccentForeground)
                ).Title("Light Theme"),
                // Dark theme colors  
                new Card(
                    Layout.Grid().Columns(1)
                        | RenderColorPreview("Primary", currentTheme.Value.Colors.Dark.Primary, currentTheme.Value.Colors.Dark.PrimaryForeground)
                        | RenderColorPreview("Secondary", currentTheme.Value.Colors.Dark.Secondary, currentTheme.Value.Colors.Dark.SecondaryForeground)
                        | RenderColorPreview("Success", currentTheme.Value.Colors.Dark.Success, currentTheme.Value.Colors.Dark.SuccessForeground)
                        | RenderColorPreview("Destructive", currentTheme.Value.Colors.Dark.Destructive, currentTheme.Value.Colors.Dark.DestructiveForeground)
                        | RenderColorPreview("Warning", currentTheme.Value.Colors.Dark.Warning, currentTheme.Value.Colors.Dark.WarningForeground)
                        | RenderColorPreview("Info", currentTheme.Value.Colors.Dark.Info, currentTheme.Value.Colors.Dark.InfoForeground)
                        | RenderColorPreview("Muted", currentTheme.Value.Colors.Dark.Muted, currentTheme.Value.Colors.Dark.MutedForeground)
                        | RenderColorPreview("Accent", currentTheme.Value.Colors.Dark.Accent, currentTheme.Value.Colors.Dark.AccentForeground)
                ).Title("Dark Theme")
            )

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

        // Map label to appropriate predefined color
        var previewColor = label switch
        {
            "Primary" => Colors.Primary,
            "Secondary" => Colors.Secondary,
            "Success" => Colors.Green,
            "Destructive" => Colors.Red,
            "Warning" => Colors.Orange,
            "Info" => Colors.Blue,
            "Muted" => Colors.Gray,
            "Accent" => Colors.Purple,
            _ => Colors.Primary
        };

        return Layout.Vertical()
            | Text.Small(label)
            | Layout.Horizontal(
                // Color preview box using appropriate predefined color
                new Box("Preview")
                    .Width(Size.Px(100))
                    .Height(Size.Px(60))
                    .Color(previewColor)
                    .BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
                    .ContentAlign(Align.Center),
                Layout.Vertical()
                    | Text.Small("Background:")
                    | UseState(bg).ToColorInput().Variant(ColorInputs.TextAndPicker).Disabled()
                    | Text.Small("Foreground:")
                    | UseState(fg).ToColorInput().Variant(ColorInputs.TextAndPicker).Disabled()
            );
    }

    private string GenerateCSharpCode(Theme theme)
    {
        var lightColors = theme.Colors.Light;
        var darkColors = theme.Colors.Dark;
        return $@"// Add this to your server configuration:
var server = new Server()
    .UseTheme(theme => {{
        theme.Name = ""{theme.Name}"";
        theme.Colors = new ThemeColorScheme
        {{
            Light = new ThemeColors
            {{
                Primary = ""{lightColors.Primary}"",
                PrimaryForeground = ""{lightColors.PrimaryForeground}"",
                Secondary = ""{lightColors.Secondary}"",
                SecondaryForeground = ""{lightColors.SecondaryForeground}"",
                Background = ""{lightColors.Background}"",
                Foreground = ""{lightColors.Foreground}"",
                Destructive = ""{lightColors.Destructive}"",
                DestructiveForeground = ""{lightColors.DestructiveForeground}"",
                Success = ""{lightColors.Success}"",
                SuccessForeground = ""{lightColors.SuccessForeground}"",
                Warning = ""{lightColors.Warning}"",
                WarningForeground = ""{lightColors.WarningForeground}"",
                Info = ""{lightColors.Info}"",
                InfoForeground = ""{lightColors.InfoForeground}"",
                Border = ""{lightColors.Border}"",
                Input = ""{lightColors.Input}"",
                Ring = ""{lightColors.Ring}"",
                Muted = ""{lightColors.Muted}"",
                MutedForeground = ""{lightColors.MutedForeground}"",
                Accent = ""{lightColors.Accent}"",
                AccentForeground = ""{lightColors.AccentForeground}"",
                Card = ""{lightColors.Card}"",
                CardForeground = ""{lightColors.CardForeground}"",
                Popover = ""{lightColors.Popover}"",
                PopoverForeground = ""{lightColors.PopoverForeground}""
            }},
            Dark = new ThemeColors
            {{
                Primary = ""{darkColors.Primary}"",
                PrimaryForeground = ""{darkColors.PrimaryForeground}"",
                Secondary = ""{darkColors.Secondary}"",
                SecondaryForeground = ""{darkColors.SecondaryForeground}"",
                Background = ""{darkColors.Background}"",
                Foreground = ""{darkColors.Foreground}"",
                Destructive = ""{darkColors.Destructive}"",
                DestructiveForeground = ""{darkColors.DestructiveForeground}"",
                Success = ""{darkColors.Success}"",
                SuccessForeground = ""{darkColors.SuccessForeground}"",
                Warning = ""{darkColors.Warning}"",
                WarningForeground = ""{darkColors.WarningForeground}"",
                Info = ""{darkColors.Info}"",
                InfoForeground = ""{darkColors.InfoForeground}"",
                Border = ""{darkColors.Border}"",
                Input = ""{darkColors.Input}"",
                Ring = ""{darkColors.Ring}"",
                Muted = ""{darkColors.Muted}"",
                MutedForeground = ""{darkColors.MutedForeground}"",
                Accent = ""{darkColors.Accent}"",
                AccentForeground = ""{darkColors.AccentForeground}"",
                Card = ""{darkColors.Card}"",
                CardForeground = ""{darkColors.CardForeground}"",
                Popover = ""{darkColors.Popover}"",
                PopoverForeground = ""{darkColors.PopoverForeground}""
            }}
        }};
    }});";
    }

    // Theme presets
    private static Theme GetOceanTheme() => new()
    {
        Name = "Ocean",
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
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
            },
            Dark = new ThemeColors
            {
                Primary = "#4A9EFF",
                PrimaryForeground = "#001122",
                Secondary = "#2D4F70",
                SecondaryForeground = "#E8F4FD",
                Background = "#001122",
                Foreground = "#E8F4FD",
                Destructive = "#FF6B7D",
                DestructiveForeground = "#FFFFFF",
                Success = "#4ECDC4",
                SuccessForeground = "#001122",
                Warning = "#FFE066",
                WarningForeground = "#001122",
                Info = "#87CEEB",
                InfoForeground = "#001122",
                Border = "#1A3A5C",
                Input = "#0F2A4A",
                Ring = "#4A9EFF",
                Muted = "#0F2A4A",
                MutedForeground = "#8BB3D9",
                Accent = "#1A3A5C",
                AccentForeground = "#E8F4FD",
                Card = "#0F2A4A",
                CardForeground = "#E8F4FD"
            }
        }
    };

    private static Theme GetForestTheme() => new()
    {
        Name = "Forest",
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
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
            },
            Dark = new ThemeColors
            {
                Primary = "#4AFF4A",
                PrimaryForeground = "#001100",
                Secondary = "#2D4A2D",
                SecondaryForeground = "#E8FFE8",
                Background = "#001100",
                Foreground = "#E8FFE8",
                Destructive = "#FF4444",
                DestructiveForeground = "#FFFFFF",
                Success = "#66FF66",
                SuccessForeground = "#001100",
                Warning = "#FFB84D",
                WarningForeground = "#001100",
                Info = "#6A9BFF",
                InfoForeground = "#001100",
                Border = "#1A3A1A",
                Input = "#0F2A0F",
                Ring = "#4AFF4A",
                Muted = "#0F2A0F",
                MutedForeground = "#8BC98B",
                Accent = "#1A3A1A",
                AccentForeground = "#E8FFE8",
                Card = "#0F2A0F",
                CardForeground = "#E8FFE8"
            }
        }
    };

    private static Theme GetSunsetTheme() => new()
    {
        Name = "Sunset",
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
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
            },
            Dark = new ThemeColors
            {
                Primary = "#FF8A65",
                PrimaryForeground = "#2A1100",
                Secondary = "#8D4A47",
                SecondaryForeground = "#FFE8E1",
                Background = "#2A1100",
                Foreground = "#FFE8E1",
                Destructive = "#FF5252",
                DestructiveForeground = "#FFFFFF",
                Success = "#81C784",
                SuccessForeground = "#2A1100",
                Warning = "#FFB74D",
                WarningForeground = "#2A1100",
                Info = "#64B5F6",
                InfoForeground = "#2A1100",
                Border = "#5D2A1A",
                Input = "#3D1F0F",
                Ring = "#FF8A65",
                Muted = "#3D1F0F",
                MutedForeground = "#C19A8A",
                Accent = "#5D2A1A",
                AccentForeground = "#FFE8E1",
                Card = "#3D1F0F",
                CardForeground = "#FFE8E1"
            }
        }
    };

    private static Theme GetMidnightTheme() => new()
    {
        Name = "Midnight",
        Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#7C3AED",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#DDD6FE",
                SecondaryForeground = "#1A1A1A",
                Background = "#FAFAFA",
                Foreground = "#1A1A1A",
                Destructive = "#EF4444",
                DestructiveForeground = "#FFFFFF",
                Success = "#10B981",
                SuccessForeground = "#FFFFFF",
                Warning = "#F59E0B",
                WarningForeground = "#000000",
                Info = "#3B82F6",
                InfoForeground = "#FFFFFF",
                Border = "#E5E7EB",
                Input = "#F3F4F6",
                Ring = "#7C3AED",
                Muted = "#F9FAFB",
                MutedForeground = "#6B7280",
                Accent = "#F3F0FF",
                AccentForeground = "#1A1A1A",
                Card = "#FFFFFF",
                CardForeground = "#1A1A1A"
            },
            Dark = new ThemeColors
            {
                Primary = "#A78BFA",
                PrimaryForeground = "#1A1A2E",
                Secondary = "#4C1D95",
                SecondaryForeground = "#E5E5E5",
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
                Ring = "#A78BFA",
                Muted = "#1F2937",
                MutedForeground = "#9CA3AF",
                Accent = "#6366F1",
                AccentForeground = "#FFFFFF",
                Card = "#1A1A2E",
                CardForeground = "#E5E5E5"
            }
        }
    };


}