using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ivy.Themes;

[ApiController]
[Route("api/[controller]")]
public class ThemeController : ControllerBase
{
    private readonly IThemeService _themeService;

    public ThemeController(IThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpGet("current")]
    public IActionResult GetCurrentTheme()
    {
        return Ok(_themeService.CurrentTheme);
    }

    [HttpPost("apply")]
    public IActionResult ApplyTheme([FromBody] ThemeConfig theme)
    {
        if (theme == null)
        {
            return BadRequest("Theme configuration is required");
        }

        _themeService.SetTheme(theme);

        // Generate CSS for dynamic application
        var css = GenerateDynamicCss(theme.Colors);

        return Ok(new { success = true, css });
    }

    [HttpGet("presets")]
    public IActionResult GetPresets()
    {
        var presets = new[]
        {
            new { id = "default", name = "Default", theme = ThemeConfig.Default },
            new { id = "ocean", name = "Ocean", theme = GetOceanTheme() },
            new { id = "forest", name = "Forest", theme = GetForestTheme() },
            new { id = "sunset", name = "Sunset", theme = GetSunsetTheme() },
            new { id = "midnight", name = "Midnight", theme = GetMidnightTheme() }
        };

        return Ok(presets);
    }

    private string GenerateDynamicCss(ThemeColors colors)
    {
        var css = ":root {";

        if (!string.IsNullOrEmpty(colors.Primary)) css += $" --primary: {colors.Primary};";
        if (!string.IsNullOrEmpty(colors.PrimaryForeground)) css += $" --primary-foreground: {colors.PrimaryForeground};";
        if (!string.IsNullOrEmpty(colors.Secondary)) css += $" --secondary: {colors.Secondary};";
        if (!string.IsNullOrEmpty(colors.SecondaryForeground)) css += $" --secondary-foreground: {colors.SecondaryForeground};";
        if (!string.IsNullOrEmpty(colors.Background)) css += $" --background: {colors.Background};";
        if (!string.IsNullOrEmpty(colors.Foreground)) css += $" --foreground: {colors.Foreground};";
        if (!string.IsNullOrEmpty(colors.Destructive)) css += $" --destructive: {colors.Destructive};";
        if (!string.IsNullOrEmpty(colors.DestructiveForeground)) css += $" --destructive-foreground: {colors.DestructiveForeground};";
        if (!string.IsNullOrEmpty(colors.Success)) css += $" --success: {colors.Success};";
        if (!string.IsNullOrEmpty(colors.SuccessForeground)) css += $" --success-foreground: {colors.SuccessForeground};";
        if (!string.IsNullOrEmpty(colors.Warning)) css += $" --warning: {colors.Warning};";
        if (!string.IsNullOrEmpty(colors.WarningForeground)) css += $" --warning-foreground: {colors.WarningForeground};";
        if (!string.IsNullOrEmpty(colors.Info)) css += $" --info: {colors.Info};";
        if (!string.IsNullOrEmpty(colors.InfoForeground)) css += $" --info-foreground: {colors.InfoForeground};";
        if (!string.IsNullOrEmpty(colors.Border)) css += $" --border: {colors.Border};";
        if (!string.IsNullOrEmpty(colors.Input)) css += $" --input: {colors.Input};";
        if (!string.IsNullOrEmpty(colors.Ring)) css += $" --ring: {colors.Ring};";
        if (!string.IsNullOrEmpty(colors.Muted)) css += $" --muted: {colors.Muted};";
        if (!string.IsNullOrEmpty(colors.MutedForeground)) css += $" --muted-foreground: {colors.MutedForeground};";
        if (!string.IsNullOrEmpty(colors.Accent)) css += $" --accent: {colors.Accent};";
        if (!string.IsNullOrEmpty(colors.AccentForeground)) css += $" --accent-foreground: {colors.AccentForeground};";
        if (!string.IsNullOrEmpty(colors.Card)) css += $" --card: {colors.Card};";
        if (!string.IsNullOrEmpty(colors.CardForeground)) css += $" --card-foreground: {colors.CardForeground};";

        css += " }";

        return css;
    }

    private static ThemeConfig GetOceanTheme()
    {
        return new ThemeConfig
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
    }

    private static ThemeConfig GetForestTheme()
    {
        return new ThemeConfig
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
    }

    private static ThemeConfig GetSunsetTheme()
    {
        return new ThemeConfig
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
    }

    private static ThemeConfig GetMidnightTheme()
    {
        return new ThemeConfig
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
}
