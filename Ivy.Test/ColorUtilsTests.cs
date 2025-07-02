using Ivy.Shared;

namespace Ivy.Test;

public class ColorUtilsTests
{
    [Fact]
    public void IsValidHexColor_ValidHexColors_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.True(ColorUtils.IsValidHexColor("#ff0000"));
        Assert.True(ColorUtils.IsValidHexColor("#00ff00"));
        Assert.True(ColorUtils.IsValidHexColor("#0000ff"));
        Assert.True(ColorUtils.IsValidHexColor("#ffffff"));
        Assert.True(ColorUtils.IsValidHexColor("#000000"));
        Assert.True(ColorUtils.IsValidHexColor("#abc123"));
        Assert.True(ColorUtils.IsValidHexColor("#ABC123"));
    }

    [Fact]
    public void IsValidHexColor_InvalidHexColors_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.False(ColorUtils.IsValidHexColor("ff0000")); // Missing #
        Assert.False(ColorUtils.IsValidHexColor("#ff000")); // Too short
        Assert.False(ColorUtils.IsValidHexColor("#ff00000")); // Too long
        Assert.False(ColorUtils.IsValidHexColor("#ff000g")); // Invalid character
        Assert.False(ColorUtils.IsValidHexColor(""));
        Assert.False(ColorUtils.IsValidHexColor(null));
    }

    [Fact]
    public void IsValidRgbColor_ValidRgbColors_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.True(ColorUtils.IsValidRgbColor("rgb(255,0,0)"));
        Assert.True(ColorUtils.IsValidRgbColor("rgb(0,255,0)"));
        Assert.True(ColorUtils.IsValidRgbColor("rgb(0,0,255)"));
        Assert.True(ColorUtils.IsValidRgbColor("rgb(255,255,255)"));
        Assert.True(ColorUtils.IsValidRgbColor("rgb(0,0,0)"));
        Assert.True(ColorUtils.IsValidRgbColor("rgb( 255 , 0 , 0 )")); // With spaces
    }

    [Fact]
    public void IsValidRgbColor_InvalidRgbColors_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.False(ColorUtils.IsValidRgbColor("rgb(256,0,0)")); // Value > 255
        Assert.False(ColorUtils.IsValidRgbColor("rgb(-1,0,0)")); // Negative value
        Assert.False(ColorUtils.IsValidRgbColor("rgb(255,0)")); // Missing component
        Assert.False(ColorUtils.IsValidRgbColor("rgb(255,0,0,0)")); // Too many components
        Assert.False(ColorUtils.IsValidRgbColor("rgb(255,0,0,)")); // Trailing comma
        Assert.False(ColorUtils.IsValidRgbColor(""));
        Assert.False(ColorUtils.IsValidRgbColor(null));
    }

    [Fact]
    public void IsValidColorName_ValidColorNames_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.True(ColorUtils.IsValidColorName("red"));
        Assert.True(ColorUtils.IsValidColorName("blue"));
        Assert.True(ColorUtils.IsValidColorName("green"));
        Assert.True(ColorUtils.IsValidColorName("primary"));
        Assert.True(ColorUtils.IsValidColorName("secondary"));
        Assert.True(ColorUtils.IsValidColorName("RED")); // Case insensitive
        Assert.True(ColorUtils.IsValidColorName("Blue"));
    }

    [Fact]
    public void IsValidColorName_InvalidColorNames_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.False(ColorUtils.IsValidColorName("notacolor"));
        Assert.False(ColorUtils.IsValidColorName(""));
        Assert.False(ColorUtils.IsValidColorName(null));
    }

    [Fact]
    public void IsValidColor_ValidColors_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.True(ColorUtils.IsValidColor("#ff0000")); // Hex
        Assert.True(ColorUtils.IsValidColor("rgb(255,0,0)")); // RGB
        Assert.True(ColorUtils.IsValidColor("red")); // Color name
    }

    [Fact]
    public void IsValidColor_InvalidColors_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.False(ColorUtils.IsValidColor("notacolor"));
        Assert.False(ColorUtils.IsValidColor(""));
        Assert.False(ColorUtils.IsValidColor(null));
    }

    [Fact]
    public void NormalizeColor_ValidColors_ReturnsHex()
    {
        // Arrange & Act & Assert
        Assert.Equal("#ff0000", ColorUtils.NormalizeColor("#ff0000")); // Already hex
        Assert.Equal("#ff0000", ColorUtils.NormalizeColor("rgb(255,0,0)")); // RGB to hex
        Assert.Equal("#dd5860", ColorUtils.NormalizeColor("red")); // Color name to hex
    }

    [Fact]
    public void NormalizeColor_InvalidColors_ReturnsNull()
    {
        // Arrange & Act & Assert
        Assert.Null(ColorUtils.NormalizeColor("notacolor"));
        Assert.Null(ColorUtils.NormalizeColor(""));
        Assert.Null(ColorUtils.NormalizeColor(null));
    }

    [Fact]
    public void GetColorHex_ValidColorNames_ReturnsHex()
    {
        // Arrange & Act & Assert
        Assert.Equal("#dd5860", ColorUtils.GetColorHex("red"));
        Assert.Equal("#381ff4", ColorUtils.GetColorHex("blue"));
        Assert.Equal("#74c997", ColorUtils.GetColorHex("primary"));
    }

    [Fact]
    public void GetColorHex_InvalidColorNames_ReturnsNull()
    {
        // Arrange & Act & Assert
        Assert.Null(ColorUtils.GetColorHex("notacolor"));
        Assert.Null(ColorUtils.GetColorHex(""));
        Assert.Null(ColorUtils.GetColorHex(null));
    }

    [Fact]
    public void GetAvailableColorNames_ReturnsAllColorNames()
    {
        // Arrange & Act
        var colorNames = ColorUtils.GetAvailableColorNames().ToList();

        // Assert
        Assert.Contains("red", colorNames);
        Assert.Contains("blue", colorNames);
        Assert.Contains("green", colorNames);
        Assert.Contains("primary", colorNames);
        Assert.Contains("secondary", colorNames);
        Assert.Contains("destructive", colorNames);
    }
}