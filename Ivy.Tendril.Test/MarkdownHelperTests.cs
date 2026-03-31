using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class MarkdownHelperTests
{
    [Fact]
    public void AnnotateBrokenFileLinks_BrokenLink_AddsWarningIndicator()
    {
        var markdown = "[AgentContext.cs](file:///C:/nonexistent/path/AgentContext.cs)";
        var result = MarkdownHelper.AnnotateBrokenFileLinks(markdown);
        Assert.Contains("\u26a0\ufe0f", result);
        Assert.Contains("[AgentContext.cs \u26a0\ufe0f](file:///C:/nonexistent/path/AgentContext.cs)", result);
    }

    [Fact]
    public void AnnotateBrokenFileLinks_ValidLink_RemainsUnchanged()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var markdown = $"[test.txt](file:///{tempFile.Replace("\\", "/")})";
            var result = MarkdownHelper.AnnotateBrokenFileLinks(markdown);
            Assert.DoesNotContain("\u26a0\ufe0f", result);
            Assert.Equal(markdown, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void AnnotateBrokenFileLinks_MixedLinks_OnlyAnnotatesBroken()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var validLink = $"[valid.txt](file:///{tempFile.Replace("\\", "/")})";
            var brokenLink = "[broken.cs](file:///C:/nonexistent/broken.cs)";
            var markdown = $"See {validLink} and {brokenLink} for details.";

            var result = MarkdownHelper.AnnotateBrokenFileLinks(markdown);

            Assert.Contains(validLink, result);
            Assert.Contains("[broken.cs \u26a0\ufe0f](file:///C:/nonexistent/broken.cs)", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void AnnotateBrokenFileLinks_EmptyString_ReturnsEmpty()
    {
        Assert.Equal("", MarkdownHelper.AnnotateBrokenFileLinks(""));
    }

    [Fact]
    public void AnnotateBrokenFileLinks_Null_ReturnsNull()
    {
        Assert.Null(MarkdownHelper.AnnotateBrokenFileLinks(null!));
    }

    [Fact]
    public void AnnotateBrokenFileLinks_NoFileLinks_ReturnsUnchanged()
    {
        var markdown = "# Hello\n\n[Google](https://google.com)\n\nSome text.";
        var result = MarkdownHelper.AnnotateBrokenFileLinks(markdown);
        Assert.Equal(markdown, result);
    }

    [Fact]
    public void FindFilesInRepos_FindsMatchingFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var subDir = Path.Combine(tempDir, "sub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "Target.cs"), "// test");

            var results = MarkdownHelper.FindFilesInRepos([tempDir], "Target.cs");
            Assert.Single(results);
            Assert.EndsWith("Target.cs", results[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindFilesInRepos_NoMatch_ReturnsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            var results = MarkdownHelper.FindFilesInRepos([tempDir], "NonExistent.cs");
            Assert.Empty(results);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindFilesInRepos_NonExistentRepo_ReturnsEmpty()
    {
        var results = MarkdownHelper.FindFilesInRepos(["C:\\nonexistent\\repo"], "Target.cs");
        Assert.Empty(results);
    }
}
