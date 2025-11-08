using Ivy;
using Xunit;

namespace Ivy.Test;

public class TitleCaseToFriendlyUrlTests
{
    [Theory]
    [InlineData("FooBarApp", "foo-bar")] // basic case with App suffix removal
    [InlineData("FooBar", "foo-bar")] // basic pascal case
    [InlineData("CLI", "cli")] // acronym should not be split into c-l-i
    [InlineData("CLIApp", "cli")] // acronym with App suffix
    [InlineData("_FooBarApp", "_foo-bar")] // leading underscore preserved
    [InlineData("_CLIApp", "_cli")] // leading underscore with acronym
    [InlineData("_CLI", "_cli")] // leading underscore acronym without App
    public void ConvertsCorrectly(string input, string expected)
    {
        var result = Utils.TitleCaseToFriendlyUrl(input);
        Assert.Equal(expected, result);
    }
}
