using System.ComponentModel;
using Ivy;

namespace Ivy.Tests.Extensions;

public class OptionExtensionsTests
{
    // Test enums
    private enum ColorPalette
    {
        BlueToRed,
        GreenYellowRed,
        PurpleToOrange
    }

    private enum SimpleColor
    {
        Red,
        Blue,
        Green
    }

    private enum ColorWithDescription
    {
        [Description("Custom Red Label")]
        Red,
        [Description("Custom Blue Label")]
        Blue,
        GreenWithoutDescription
    }

    [Fact]
    public void ToOptions_WithPascalCaseEnum_FormatsWithSpaces()
    {
        // Arrange
        var values = new[] { ColorPalette.BlueToRed, ColorPalette.GreenYellowRed, ColorPalette.PurpleToOrange };

        // Act
        var options = values.ToOptions();

        // Assert
        Assert.Equal(3, options.Length);
        Assert.Equal("Blue To Red", options[0].Label);
        Assert.Equal(ColorPalette.BlueToRed, options[0].TypedValue);
        Assert.Equal("Green Yellow Red", options[1].Label);
        Assert.Equal(ColorPalette.GreenYellowRed, options[1].TypedValue);
        Assert.Equal("Purple To Orange", options[2].Label);
        Assert.Equal(ColorPalette.PurpleToOrange, options[2].TypedValue);
    }

    [Fact]
    public void ToOptions_WithDescriptionAttribute_PrefersDescription()
    {
        // Arrange
        var values = new[] { ColorWithDescription.Red, ColorWithDescription.Blue, ColorWithDescription.GreenWithoutDescription };

        // Act
        var options = values.ToOptions();

        // Assert
        Assert.Equal(3, options.Length);
        Assert.Equal("Custom Red Label", options[0].Label);
        Assert.Equal(ColorWithDescription.Red, options[0].TypedValue);
        Assert.Equal("Custom Blue Label", options[1].Label);
        Assert.Equal(ColorWithDescription.Blue, options[1].TypedValue);
        Assert.Equal("Green Without Description", options[2].Label);
        Assert.Equal(ColorWithDescription.GreenWithoutDescription, options[2].TypedValue);
    }

    [Fact]
    public void ToOptions_WithSingleWordEnum_NoChange()
    {
        // Arrange
        var values = new[] { SimpleColor.Red, SimpleColor.Blue, SimpleColor.Green };

        // Act
        var options = values.ToOptions();

        // Assert
        Assert.Equal(3, options.Length);
        Assert.Equal("Red", options[0].Label);
        Assert.Equal(SimpleColor.Red, options[0].TypedValue);
        Assert.Equal("Blue", options[1].Label);
        Assert.Equal(SimpleColor.Blue, options[1].TypedValue);
        Assert.Equal("Green", options[2].Label);
        Assert.Equal(SimpleColor.Green, options[2].TypedValue);
    }

    [Fact]
    public void ToOptions_WithNonEnumType_UsesToString()
    {
        // Arrange
        var values = new[] { "StringValue", "AnotherString" };

        // Act
        var options = values.ToOptions();

        // Assert
        Assert.Equal(2, options.Length);
        Assert.Equal("StringValue", options[0].Label);
        Assert.Equal("StringValue", options[0].TypedValue);
        Assert.Equal("AnotherString", options[1].Label);
        Assert.Equal("AnotherString", options[1].TypedValue);
    }

    [Fact]
    public void ToOptions_WithIntegerType_UsesToString()
    {
        // Arrange
        var values = new[] { 1, 2, 3 };

        // Act
        var options = values.ToOptions();

        // Assert
        Assert.Equal(3, options.Length);
        Assert.Equal("1", options[0].Label);
        Assert.Equal(1, options[0].TypedValue);
        Assert.Equal("2", options[1].Label);
        Assert.Equal(2, options[1].TypedValue);
        Assert.Equal("3", options[2].Label);
        Assert.Equal(3, options[2].TypedValue);
    }

    [Fact]
    public void ToOptions_WithNullValue_HandlesGracefully()
    {
        // Arrange
        string?[] values = [null, "NotNull"];

        // Act
        var options = values.ToOptions();

        // Assert
        Assert.Equal(2, options.Length);
        Assert.Equal("?", options[0].Label);
        Assert.Null(options[0].TypedValue);
        Assert.Equal("NotNull", options[1].Label);
        Assert.Equal("NotNull", options[1].TypedValue);
    }

    [Fact]
    public void ToOptions_TypeOverload_WithEnum_UsesGetDescription()
    {
        // Arrange
        var enumType = typeof(ColorPalette);

        // Act
        var options = enumType.ToOptions();

        // Assert
        Assert.Equal(3, options.Length);
        Assert.Equal("Blue To Red", options[0].Label);
        Assert.Equal(ColorPalette.BlueToRed, options[0].Value);
        Assert.Equal("Green Yellow Red", options[1].Label);
        Assert.Equal(ColorPalette.GreenYellowRed, options[1].Value);
        Assert.Equal("Purple To Orange", options[2].Label);
        Assert.Equal(ColorPalette.PurpleToOrange, options[2].Value);
    }

    [Fact]
    public void ToOptions_TypeOverload_WithDescriptionAttribute_PrefersDescription()
    {
        // Arrange
        var enumType = typeof(ColorWithDescription);

        // Act
        var options = enumType.ToOptions();

        // Assert
        Assert.Equal(3, options.Length);
        Assert.Equal("Custom Red Label", options[0].Label);
        Assert.Equal("Custom Blue Label", options[1].Label);
        Assert.Equal("Green Without Description", options[2].Label);
    }
}
