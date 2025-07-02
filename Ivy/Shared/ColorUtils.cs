using System.Text.RegularExpressions;

namespace Ivy.Shared;

/// <summary>
/// Utility class for color parsing and validation
/// Supports hex colors (#RRGGBB), RGB values (rgb(r,g,b)), and color names from the Colors enum
/// </summary>
public static class ColorUtils
{
    private static readonly Dictionary<string, string> ColorNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["black"] = "#000000",
        ["white"] = "#ffffff",
        ["slate"] = "#6a7489",
        ["gray"] = "#6e727f",
        ["zinc"] = "#717179",
        ["neutral"] = "#737373",
        ["stone"] = "#76716d",
        ["red"] = "#dd5860",
        ["orange"] = "#dc824d",
        ["amber"] = "#deb145",
        ["yellow"] = "#e5e04c",
        ["lime"] = "#afd953",
        ["green"] = "#86d26f",
        ["emerald"] = "#76cd94",
        ["teal"] = "#5b9ba8",
        ["cyan"] = "#4469c0",
        ["sky"] = "#373bda",
        ["blue"] = "#381ff4",
        ["indigo"] = "#4b28e2",
        ["violet"] = "#6637d1",
        ["purple"] = "#844cc0",
        ["fuchsia"] = "#a361af",
        ["pink"] = "#c377a0",
        ["rose"] = "#e48e91",
        ["primary"] = "#74c997",
        ["secondary"] = "#c2cbc7",
        ["destructive"] = "#dd5860"
    };

    /// <summary>
    /// Validates if a string is a valid hex color
    /// </summary>
    public static bool IsValidHexColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;

        var hexRegex = new Regex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
        return hexRegex.IsMatch(color);
    }

    /// <summary>
    /// Validates if a string is a valid RGB color
    /// </summary>
    public static bool IsValidRgbColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;

        var rgbRegex = new Regex(@"^rgb\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*\)$");
        if (!rgbRegex.IsMatch(color)) return false;

        // Extract RGB values and validate ranges
        var match = Regex.Match(color, @"rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)");
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[1].Value, out var red) ||
            !int.TryParse(match.Groups[2].Value, out var green) ||
            !int.TryParse(match.Groups[3].Value, out var blue))
            return false;

        return red >= 0 && red <= 255 &&
               green >= 0 && green <= 255 &&
               blue >= 0 && blue <= 255;
    }

    /// <summary>
    /// Validates if a string is a valid color name from the Colors enum
    /// </summary>
    public static bool IsValidColorName(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        return ColorNames.ContainsKey(color);
    }

    /// <summary>
    /// Validates if a string is a valid color (hex, RGB, or color name)
    /// </summary>
    public static bool IsValidColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;

        return IsValidHexColor(color) ||
               IsValidRgbColor(color) ||
               IsValidColorName(color);
    }

    /// <summary>
    /// Converts a color string to a standardized hex format
    /// Returns hex color for valid inputs, null for invalid
    /// </summary>
    public static string? NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return null;

        var trimmedColor = color.Trim();

        // Already a valid hex color
        if (IsValidHexColor(trimmedColor))
        {
            return trimmedColor;
        }

        // RGB color - convert to hex
        if (IsValidRgbColor(trimmedColor))
        {
            var match = Regex.Match(trimmedColor, @"rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)");
            if (match.Success)
            {
                var red = int.Parse(match.Groups[1].Value);
                var green = int.Parse(match.Groups[2].Value);
                var blue = int.Parse(match.Groups[3].Value);
                return $"#{red:x2}{green:x2}{blue:x2}";
            }
        }

        // Color name - convert to hex
        if (IsValidColorName(trimmedColor))
        {
            return ColorNames[trimmedColor];
        }

        return null;
    }

    /// <summary>
    /// Gets the hex value for a color name
    /// </summary>
    public static string? GetColorHex(string? colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName)) return null;
        return ColorNames.TryGetValue(colorName, out var hex) ? hex : null;
    }

    /// <summary>
    /// Gets all available color names from the Colors enum
    /// </summary>
    public static IEnumerable<string> GetAvailableColorNames()
    {
        return ColorNames.Keys;
    }

    /// <summary>
    /// Gets the display value for a color (for showing in input fields)
    /// Returns the original input if it's a valid color, otherwise returns the normalized hex
    /// </summary>
    public static string GetColorDisplayValue(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return string.Empty;

        var normalized = NormalizeColor(color);
        if (normalized == null) return color; // Return original if invalid

        // If it's already a hex color, return as is
        if (IsValidHexColor(color)) return color;

        // If it's RGB, return the hex equivalent
        if (IsValidRgbColor(color)) return normalized;

        // If it's a color name, return the hex equivalent
        if (IsValidColorName(color)) return normalized;

        return color;
    }
}