using System.ComponentModel.DataAnnotations;
using Ivy.Views.Forms;

namespace Ivy.Test;

public class TestModel
{
    [Required]
    public string RequiredString { get; set; } = string.Empty;

    public string? NullableString { get; set; }

    public string NonNullableString { get; set; } = string.Empty;

    public decimal Decimal { get; set; } = 1;
}

public class FormHelpersTest
{
    [Fact]
    public void IsRequired_ShouldReturnTrue_WhenRequiredAttributeIsPresentOnProperty()
    {
        // Arrange
        var propertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.RequiredString));

        // Act
        var result = FormHelpers.IsRequired(propertyInfo!);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRequired_ShouldReturnFalse_WhenPropertyIsNullableString()
    {
        // Arrange
        var propertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.NullableString));

        // Act
        var result = FormHelpers.IsRequired(propertyInfo!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRequired_ShouldReturnTrue_WhenPropertyIsNonNullableStringWithoutRequiredAttribute()
    {
        // Arrange
        var propertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.NonNullableString));

        // Act
        var result = FormHelpers.IsRequired(propertyInfo!);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRequired_ShouldReturnFalse_WhenPropertyIsNotString()
    {
        // Arrange
        var propertyInfo = typeof(TestModel).GetProperty(nameof(TestModel.Decimal));

        // Act
        var result = FormHelpers.IsRequired(propertyInfo!);

        // Assert
        Assert.False(result);
    }
}