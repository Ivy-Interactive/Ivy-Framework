using Ivy.IvyML;

namespace Ivy.IvyML.Test;

public class IvyMLValidatorTests
{
    [Fact]
    public void ValidIvyML_ReturnsSuccess()
    {
        var result = IvyMLValidator.Validate("<TextBlock Content=\"Hello\" />");
        Assert.True(result.IsValid);
        Assert.NotNull(result.Widget);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void MalformedMarkup_ReturnsFailure()
    {
        var result = IvyMLValidator.Validate("<TextBlock Content=\"Hello\"");
        Assert.False(result.IsValid);
        Assert.Null(result.Widget);
        Assert.Contains("Malformed markup", result.ErrorMessage);
    }

    [Fact]
    public void UnknownElement_ReturnsFailure()
    {
        var result = IvyMLValidator.Validate("<NonExistentWidget />");
        Assert.False(result.IsValid);
        Assert.Null(result.Widget);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void UnknownProperty_ReturnsFailure()
    {
        var result = IvyMLValidator.Validate("<TextBlock FakeProperty=\"value\" />");
        Assert.False(result.IsValid);
        Assert.Null(result.Widget);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void NestedWidgets_ReturnsSuccess()
    {
        var ivyml = """
            <Card>
                <TextBlock Content="Hello" />
            </Card>
            """;
        var result = IvyMLValidator.Validate(ivyml);
        Assert.True(result.IsValid);
        Assert.NotNull(result.Widget);
    }
}
