namespace Ivy.Docs.Tools.Test;

public class LinkConverterTests
{
    [Fact]
    public void Convert1()
    {
        var converter = new LinkConverter("01_Onboarding/01_Introduction.md");
        var input = "[Example](02_Installation.md)";
        var expectedMarkdown = "[Example](app://onboarding/installation-app)";
        var expectedTypes = new HashSet<string> { "Onboarding.InstallationApp" };
        var (types, markdown) = converter.Convert(input);
        Assert.Equal(expectedTypes, types);
        Assert.Equal(expectedMarkdown, markdown);
    }

    [Theory]
    [InlineData("01_Onboarding/01_Introduction.md", "[Intro](./02_Installation.md)", "[Intro](app://onboarding/installation-app)", "Onboarding.InstallationApp")]
    [InlineData("01_Onboarding/01_Introduction.md", "[External](https://example.com)", "[External](https://example.com)", null)]
    [InlineData("01_Onboarding/01_Introduction.md", "[Parent](../Common/Utils.md)", "[Parent](app://common/utils-app)", "Common.UtilsApp")]
    [InlineData("GettingStarted/Index.md", "[Step](./Setup/Step1.md)", "[Step](app://getting-started/setup/step1-app)", "GettingStarted.Setup.Step1App")]
    [InlineData("Index.md", "[Start](Foo.md)", "[Start](app://foo-app)", "FooApp")]
    public void Convert_VariousLinks_Works(string sourcePath, string input, string expectedMarkdown, string? expectedType)
    {
        var converter = new LinkConverter(sourcePath);
        var (types, markdown) = converter.Convert(input);

        if (expectedType != null)
            Assert.Contains(expectedType, types);
        else
            Assert.Empty(types);

        Assert.Equal(expectedMarkdown, markdown);
    }

    [Theory]
    [InlineData(
        "01_Onboarding/01_Introduction.md",
        "[One](./02_Installation.md) and [Two](../Common/Utils.md)",
        "[One](app://onboarding/installation-app) and [Two](app://common/utils-app)",
        new[] { "Onboarding.InstallationApp", "Common.UtilsApp" })]
    [InlineData(
        "GettingStarted/Index.md",
        "See [Setup](./Setup/Step1.md) or visit [Site](https://example.com)",
        "See [Setup](app://getting-started/setup/step1-app) or visit [Site](https://example.com)",
        new[] { "GettingStarted.Setup.Step1App" })]
    [InlineData(
        "Index.md",
        "[A](Foo.md), [B](Bar/Baz.md), [C](https://external.com)",
        "[A](app://foo-app), [B](app://bar/baz-app), [C](https://external.com)",
        new[] { "FooApp", "Bar.BazApp" })]
    [InlineData(
        "01_Onboarding\\02_Concepts\\Clients.md",
        "- [Forms](./Forms.md)",
        "- [Forms](app://onboarding/concepts/forms-app)",
        new[] { "Onboarding.Concepts.FormsApp" })]
    public void Convert_MultipleLinks_Works(string sourcePath, string input, string expectedMarkdown, string[] expectedTypes)
    {
        var converter = new LinkConverter(sourcePath);
        var (types, markdown) = converter.Convert(input);

        Assert.Equal(expectedMarkdown, markdown);
        Assert.Equal(expectedTypes.ToHashSet(), types);
    }


    [Theory]
    [InlineData(
        "01_Onboarding/01_Introduction.md",
        "![Foo](./images/Foo.png)",
        "![Foo](./images/Foo.png)",
        new string[] { })]
    public void Convert_Images_Works(string sourcePath, string input, string expectedMarkdown, string[] expectedTypes)
    {
        var converter = new LinkConverter(sourcePath);
        var (types, markdown) = converter.Convert(input);

        Assert.Equal(expectedMarkdown, markdown);
        Assert.Equal(expectedTypes.ToHashSet(), types);
    }
}