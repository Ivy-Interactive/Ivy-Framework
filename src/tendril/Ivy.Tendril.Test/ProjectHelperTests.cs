using Ivy.Tendril.Helpers;

namespace Ivy.Tendril.Test;

public class ProjectHelperTests
{
    [Theory]
    [InlineData("Framework", new[] { "Framework" })]
    [InlineData("Framework, Tendril", new[] { "Framework", "Tendril" })]
    [InlineData("Framework,Tendril,Agent", new[] { "Framework", "Tendril", "Agent" })]
    [InlineData("  Framework  ,  Tendril  ", new[] { "Framework", "Tendril" })]
    [InlineData("", new string[0])]
    [InlineData(null, new string[0])]
    public void ParseProjects_HandlesVariousFormats(string? input, string[] expected)
    {
        var result = ProjectHelper.ParseProjects(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Framework", "Framework", true)]
    [InlineData("Framework, Tendril", "Framework", true)]
    [InlineData("Framework, Tendril", "Tendril", true)]
    [InlineData("Framework, Tendril", "Agent", false)]
    [InlineData("Framework", "framework", true)]
    [InlineData("", "Framework", false)]
    [InlineData(null, "Framework", false)]
    public void ContainsProject_ChecksMembership(string? projectValue, string projectToFind, bool expected)
    {
        var result = ProjectHelper.ContainsProject(projectValue, projectToFind);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Framework", "Framework")]
    [InlineData("Framework, Tendril", "Framework, Tendril")]
    [InlineData("  Framework  ,  Tendril  ", "Framework, Tendril")]
    [InlineData("", "Auto")]
    [InlineData(null, "Auto")]
    public void FormatProjectsForDisplay_FormatsCorrectly(string? input, string expected)
    {
        var result = ProjectHelper.FormatProjectsForDisplay(input);
        Assert.Equal(expected, result);
    }
}
