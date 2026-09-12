using Ivy.Core.Server;

namespace Ivy.Test;

public class TlsPolicyTests
{
    [Theory]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData("yes", true)]
    [InlineData("on", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("Yes", true)]
    [InlineData("YES", true)]
    [InlineData("On", true)]
    [InlineData("ON", true)]
    public void IsEnabled_AcceptedTokens_ReturnsTrue(string value, bool expected)
    {
        Assert.Equal(expected, TlsPolicy.IsEnabled(value, fallback: false));
        Assert.Equal(expected, TlsPolicy.IsEnabled(value, fallback: true));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("false")]
    [InlineData("no")]
    [InlineData("off")]
    [InlineData("False")]
    [InlineData("NO")]
    [InlineData("OFF")]
    public void IsEnabled_RejectedTokens_ReturnsFalse(string value)
    {
        Assert.False(TlsPolicy.IsEnabled(value, fallback: false));
        Assert.False(TlsPolicy.IsEnabled(value, fallback: true));
    }

    [Theory]
    [InlineData("banana")]
    [InlineData("maybe")]
    [InlineData("1.0")]
    [InlineData("yes please")]
    [InlineData(" true")]
    [InlineData("true ")]
    public void IsEnabled_UnrecognizedValue_ReturnsFalse_EvenWithTrueFallback(string value)
    {
        Assert.False(TlsPolicy.IsEnabled(value, fallback: false));
        Assert.False(TlsPolicy.IsEnabled(value, fallback: true));
    }

    [Fact]
    public void IsEnabled_NullValue_ReturnsFallback()
    {
        Assert.False(TlsPolicy.IsEnabled(null, fallback: false));
        Assert.True(TlsPolicy.IsEnabled(null, fallback: true));
    }

    [Fact]
    public void IsEnabled_EmptyString_ReturnsFallback()
    {
        Assert.False(TlsPolicy.IsEnabled("", fallback: false));
        Assert.True(TlsPolicy.IsEnabled("", fallback: true));
    }
}
