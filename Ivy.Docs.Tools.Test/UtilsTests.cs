namespace Ivy.Docs.Tools.Test;

public class UtilsTests
{
    [Theory]
    [InlineData(@"C:\Foo\Bar\Input\", @"C:\Foo\Bar\Input\01_Baz\03_Fizz\Goo\Qux.md", @"Baz/Fizz/Goo")]
    [InlineData(@"C:\Data\", @"C:\Data\Baz\Fizz\Qux.md", @"Baz/Fizz")]
    [InlineData(@"C:\Root\", @"C:\Root\001_Alpha\Beta\Qux.md", @"Alpha/Beta")]
    [InlineData(@"C:\Test\", @"C:\Test\Qux.md", @"")]
    [InlineData(@"C:\Test\", @"C:\Test\01_Baz\Qux.md", @"Baz")]
    [InlineData(@"C:\Test\", @"C:\Test\Baz\Qux.md", @"Baz")]
    [InlineData(@"/mnt/data/input/", @"/mnt/data/input/01_Baz/03_Fizz/Goo/Qux.md", @"Baz/Fizz/Goo")]
    public void GetRelativeFolderWithoutOrder_ReturnsExpected(string inputFolder, string inputFile, string expected)
    {
        var result = Utils.GetRelativeFolderWithoutOrder(inputFolder, inputFile);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"01_Onboarding/01_Introduction.md", @"02_Installation.md", @"01_Onboarding/02_Installation.md")]
    [InlineData(@"01_Onboarding/01_Introduction.md", @"../XYZ/02_Foo.md", @"XYZ/02_Foo.md")]
    [InlineData(@"01_Onboarding/ABC/01_Introduction.md", @"./XYZ/02_Foo.md", @"01_Onboarding/ABC/XYZ/02_Foo.md")]
    [InlineData(@"01_Onboarding/ABC/01_Introduction.md", @"XYZ/02_Foo.md", @"01_Onboarding/ABC/XYZ/02_Foo.md")]
    public void GetPathForLink_ReturnsExpected(string source, string link, string expected)
    {
        var result = Utils.GetPathForLink(source, link);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"01_Onboarding/02_Installation.md", "Onboarding.InstallationApp")]
    public void GetTypeNameFromPath_ReturnsExpected(string path, string expectedTypeName)
    {
        var result = Utils.GetTypeNameFromPath(path);
        Assert.Equal(expectedTypeName, result);
    }

    [Theory]
    [InlineData("Onboarding.InstallationApp", "onboarding/installation-app")]
    public void GetAppIdFromTypeName_ReturnsExpected(string typeName, string expectedAppId)
    {
        var result = Utils.GetAppIdFromTypeName(typeName);
        Assert.Equal(expectedAppId, result);
    }
}

