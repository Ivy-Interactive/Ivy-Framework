namespace Ivy.Test.Helpers;

public class StringHelperTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("hello", "hello")]
    public void NullIfEmpty_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, input.NullIfEmpty());
    }

    [Theory]
    [InlineData("capital call", "Capital Call")]
    [InlineData("CAPITAL CALL", "Capital Call")]
    [InlineData("management fee", "Management Fee")]
    [InlineData("distribution", "Distribution")]
    [InlineData("cash-flow", "Cash-Flow")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ToTitleCase_ReturnsExpected(string? input, string expected)
    {
        Assert.Equal(expected, StringHelper.ToTitleCase(input));
    }

    [Theory]
    [InlineData("FooBar", "fooBar")]
    [InlineData("A", "a")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void TitleCaseToCamelCase_ReturnsExpected(string input, string expected)
    {
        Assert.Equal(expected, StringHelper.TitleCaseToCamelCase(input));
    }

    [Theory]
    [InlineData("fooBar", "FooBar")]
    [InlineData("a", "A")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void CamelCaseToTitleCase_ReturnsExpected(string input, string expected)
    {
        Assert.Equal(expected, StringHelper.CamelCaseToTitleCase(input));
    }

    [Theory]
    [InlineData("avgSalary", "Avg Salary")]
    [InlineData("TotalSalary", "Total Salary")]
    [InlineData("count", "Count")]
    [InlineData(null, null)]
    public void SplitPascalCase_SplitsCorrectly(string? input, string? expected)
    {
        Assert.Equal(expected, StringHelper.SplitPascalCase(input));
    }

    [Theory]
    [InlineData("FooBarApp", "foo-bar")]
    [InlineData("FooBar", "foo-bar")]
    [InlineData("CLI", "cli")]
    [InlineData("CLIApp", "cli")]
    [InlineData("_FooBarApp", "_foo-bar")]
    [InlineData("HTMLParser", "html-parser")]
    public void TitleCaseToFriendlyUrl_ConvertsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, StringHelper.TitleCaseToFriendlyUrl(input));
    }

    [Theory]
    [InlineData("FooBar", "Foo Bar")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("DatePickerApp", "Date Picker")]
    [InlineData("UseStateApp", "UseState")]
    [InlineData("UseRefreshTokenApp", "UseRefreshToken")]
    [InlineData("UseContextApp", "UseContext")]
    public void TitleCaseToReadable_ConvertsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, StringHelper.TitleCaseToReadable(input));
    }

    [Theory]
    [InlineData("hello world", " world", "hello")]
    [InlineData("hello", "xyz", "hello")]
    public void TrimEnd_TrimsCorrectly(string source, string value, string expected)
    {
        Assert.Equal(expected, source.TrimEnd(value));
    }

    [Theory]
    [InlineData("hello///", '/', "hello")]
    [InlineData("hello", '/', "hello")]
    [InlineData("", '/', "")]
    public void EatRight_Char_EatsCorrectly(string input, char food, string expected)
    {
        Assert.Equal(expected, input.EatRight(food));
    }

    [Theory]
    [InlineData("///hello", '/', "hello")]
    [InlineData("hello", '/', "hello")]
    [InlineData("", '/', "")]
    public void EatLeft_Char_EatsCorrectly(string input, char food, string expected)
    {
        Assert.Equal(expected, input.EatLeft(food));
    }

    [Fact]
    public void EatRight_String_EatsCorrectly()
    {
        Assert.Equal("hello", "helloabcabc".EatRight("abc"));
    }

    [Fact]
    public void EatLeft_String_EatsCorrectly()
    {
        Assert.Equal("hello", "abcabchello".EatLeft("abc"));
    }

    [Theory]
    [InlineData("FirstName", null, "First Name")]
    [InlineData("createdAt", "DateTime", "Created")]
    public void LabelFor_ReturnsExpected(string name, string? typeName, string expected)
    {
        Type? type = typeName switch
        {
            "DateTime" => typeof(DateTime),
            _ => null
        };
        Assert.Equal(expected, StringHelper.LabelFor(name, type));
    }

    [Fact]
    public void GetShortHash_ReturnsDeterministicHash()
    {
        var hash1 = StringHelper.GetShortHash("test");
        var hash2 = StringHelper.GetShortHash("test");
        Assert.Equal(hash1, hash2);
        Assert.Equal(8, hash1.Length);
    }

    [Fact]
    public void GetShortHash_DifferentInputsProduceDifferentHashes()
    {
        var hash1 = StringHelper.GetShortHash("foo");
        var hash2 = StringHelper.GetShortHash("bar");
        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    public void FormatBytes_FormatsCorrectly(long bytes, string expected)
    {
        Assert.Equal(expected, StringHelper.FormatBytes(bytes));
    }

    [Theory]
    [InlineData(500, "500")]
    [InlineData(1500, "1.5K")]
    [InlineData(1500000, "1.5M")]
    [InlineData(1500000000, "1.5B")]
    public void FormatNumber_FormatsCorrectly(double number, string expected)
    {
        Assert.Equal(expected, StringHelper.FormatNumber(number));
    }
}
