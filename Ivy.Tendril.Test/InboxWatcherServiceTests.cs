using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class InboxWatcherServiceTests
{
    [Fact]
    public void ParseContent_PlainMarkdown_ReturnsAutoProject()
    {
        var content = "Add a new color picker widget with HSL support";

        var (project, description) = InboxWatcherService.ParseContent(content);

        Assert.Equal("[Auto]", project);
        Assert.Equal(content, description);
    }

    [Fact]
    public void ParseContent_WithFrontmatter_ExtractsProject()
    {
        var content = "---\nproject: Framework\n---\nAdd a new color picker widget";

        var (project, description) = InboxWatcherService.ParseContent(content);

        Assert.Equal("Framework", project);
        Assert.Equal("Add a new color picker widget", description);
    }

    [Fact]
    public void ParseContent_FrontmatterWithoutProject_ReturnsAuto()
    {
        var content = "---\nlevel: Critical\n---\nFix the login bug";

        var (project, description) = InboxWatcherService.ParseContent(content);

        Assert.Equal("[Auto]", project);
        Assert.Equal("Fix the login bug", description);
    }

    [Fact]
    public void ParseContent_EmptyDescriptionAfterFrontmatter_ReturnsFull()
    {
        var content = "---\nproject: Agent\n---\n";

        var (project, description) = InboxWatcherService.ParseContent(content);

        Assert.Equal("Agent", project);
        Assert.Equal(content, description);
    }

    [Fact]
    public void ParseContent_IncompleteYamlFrontmatter_TreatsAsPlainContent()
    {
        var content = "--- some header without closing";

        var (project, description) = InboxWatcherService.ParseContent(content);

        Assert.Equal("[Auto]", project);
        Assert.Equal(content, description);
    }
}
