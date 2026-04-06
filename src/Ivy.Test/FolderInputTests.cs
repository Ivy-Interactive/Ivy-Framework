namespace Ivy.Test;

public class FolderInputTests
{
    [Fact]
    public void FolderInput_DefaultProperties()
    {
        var input = new FolderInput();

        Assert.Null(input.Value);
        Assert.False(input.Disabled);
        Assert.Null(input.Invalid);
        Assert.Null(input.Placeholder);
        Assert.True(input.Nullable);
        Assert.False(input.AutoFocus);
        Assert.Null(input.OnBlur);
        Assert.Null(input.OnFocus);
        Assert.Null(input.OnChange);
    }

    [Fact]
    public void FolderInput_SupportedStateTypes_ReturnsStringType()
    {
        var input = new FolderInput();

        var types = input.SupportedStateTypes();

        Assert.Single(types);
        Assert.Equal(typeof(string), types[0]);
    }

    [Fact]
    public void FolderInput_ValidateValue_ReturnsSuccess()
    {
        var input = new FolderInput();

        var result = input.ValidateValue("any-folder");

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FolderInput_ValidateValue_WithNull_ReturnsSuccess()
    {
        var input = new FolderInput();

        var result = input.ValidateValue(null);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FolderInputExtensions_Placeholder_SetsPlaceholder()
    {
        var input = new FolderInput();

        var result = input.Placeholder("Choose directory");

        Assert.Equal("Choose directory", result.Placeholder);
    }

    [Fact]
    public void FolderInputExtensions_Disabled_SetsDisabled()
    {
        var input = new FolderInput();

        var result = input.Disabled();

        Assert.True(result.Disabled);
    }

    [Fact]
    public void FolderInputExtensions_Disabled_False_SetsDisabledFalse()
    {
        var input = new FolderInput { Disabled = true };

        var result = input.Disabled(false);

        Assert.False(result.Disabled);
    }

    [Fact]
    public void FolderInputExtensions_Invalid_SetsInvalid()
    {
        var input = new FolderInput();

        var result = input.Invalid("Required");

        Assert.Equal("Required", result.Invalid);
    }

    [Fact]
    public void FolderInputExtensions_Invalid_Null_ClearsInvalid()
    {
        var input = new FolderInput { Invalid = "Error" };

        var result = input.Invalid(null);

        Assert.Null(result.Invalid);
    }
}
