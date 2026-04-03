using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class InboxWatcherServiceTests
{
    [Fact]
    public void ParseContent_PlainMarkdown_ReturnsAutoProject()
    {
        var content = "Add a new color picker widget with HSL support";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("[Auto]", project);
        Assert.Equal(content, description);
        Assert.Null(sourcePath);
    }

    [Fact]
    public void ParseContent_WithFrontmatter_ExtractsProject()
    {
        var content = "---\nproject: Framework\n---\nAdd a new color picker widget";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("Framework", project);
        Assert.Equal("Add a new color picker widget", description);
        Assert.Null(sourcePath);
    }

    [Fact]
    public void ParseContent_FrontmatterWithoutProject_ReturnsAuto()
    {
        var content = "---\nlevel: Critical\n---\nFix the login bug";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("[Auto]", project);
        Assert.Equal("Fix the login bug", description);
        Assert.Null(sourcePath);
    }

    [Fact]
    public void ParseContent_EmptyDescriptionAfterFrontmatter_ReturnsFull()
    {
        var content = "---\nproject: Agent\n---\n";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("Agent", project);
        Assert.Equal(content, description);
        Assert.Null(sourcePath);
    }

    [Fact]
    public void ParseContent_IncompleteYamlFrontmatter_TreatsAsPlainContent()
    {
        var content = "--- some header without closing";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("[Auto]", project);
        Assert.Equal(content, description);
        Assert.Null(sourcePath);
    }

    [Fact]
    public void ParseContent_WithSourcePath_ExtractsAll()
    {
        var content = "---\nproject: Agent\nsourcePath: D:\\Tests\\Session123\n---\nFix the widget rendering";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("Agent", project);
        Assert.Equal("Fix the widget rendering", description);
        Assert.Equal("D:\\Tests\\Session123", sourcePath);
    }

    [Fact]
    public void ParseContent_WithoutSourcePath_ReturnsNull()
    {
        var content = "---\nproject: Framework\n---\nAdd a button";

        var (project, description, sourcePath) = InboxWatcherService.ParseContent(content);

        Assert.Equal("Framework", project);
        Assert.Equal("Add a button", description);
        Assert.Null(sourcePath);
    }

    [Fact]
    public void ProcessExistingFiles_PicksUpFilesInInbox()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"inbox-test-{Guid.NewGuid():N}");
        var inboxDir = Path.Combine(tempDir, "Inbox");
        Directory.CreateDirectory(inboxDir);

        try
        {
            // Place a file in the inbox before creating the service
            File.WriteAllText(Path.Combine(inboxDir, "test-entry.md"), "Test inbox entry");

            var config = new ConfigService(new TendrilSettings { TendrilData = tempDir });
            var jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), inboxDir);
            using var watcher = new InboxWatcherService(config, jobService);

            // The constructor calls ProcessExistingFiles, which dispatches async processing.
            // Wait briefly for the async task to pick up and rename the file to .processing.
            Thread.Sleep(2000);

            // The .md file should have been renamed to .processing (job started)
            Assert.Empty(Directory.GetFiles(inboxDir, "*.md"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
