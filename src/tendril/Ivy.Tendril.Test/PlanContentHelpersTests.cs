using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PlanContentHelpersTests
{
    [Fact]
    public void GetArtifacts_WithSubDirectories_ReturnsCategorizedFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var screenshotsDir = Path.Combine(planDir, "artifacts", "screenshots");
            Directory.CreateDirectory(screenshotsDir);
            File.WriteAllText(Path.Combine(screenshotsDir, "shot1.png"), "fake");
            File.WriteAllText(Path.Combine(screenshotsDir, "shot2.png"), "fake");

            var result = PlanContentHelpers.GetArtifacts(planDir);

            Assert.True(result.ContainsKey("screenshots"));
            Assert.Equal(2, result["screenshots"].Count);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetArtifacts_WithNoArtifacts_ReturnsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);

            var result = PlanContentHelpers.GetArtifacts(tempDir);

            Assert.Empty(result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BuildCommitRows_ReturnsCorrectShortHash()
    {
        var gitService = new StubGitService("Test commit",
            [("M", "file.cs")]);
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], ["abcdef1234567890"], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var rows = PlanContentHelpers.BuildCommitRows(plan, config, gitService);

        Assert.Single(rows);
        Assert.Equal("abcdef1", rows[0].ShortHash);
        Assert.Equal("abcdef1234567890", rows[0].Hash);
        Assert.Equal("Test commit", rows[0].Title);
        Assert.Equal(1, rows[0].FileCount);
    }

    [Fact]
    public void BuildCommitRows_WithNullTitle_SetsEmptyTitleAndNullFileCount()
    {
        var gitService = new StubGitService(null, null);
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], ["abcdef1234567890"], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var rows = PlanContentHelpers.BuildCommitRows(plan, config, gitService);

        Assert.Single(rows);
        Assert.Equal("", rows[0].Title);
        Assert.Null(rows[0].FileCount);
    }

    [Fact]
    public void BuildCommitRows_WithEmptyFiles_SetsZeroFileCount()
    {
        var gitService = new StubGitService("Some commit", []);
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], ["abcdef1234567890"], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var rows = PlanContentHelpers.BuildCommitRows(plan, config, gitService);

        Assert.Single(rows);
        Assert.Equal("Some commit", rows[0].Title);
        Assert.Equal(0, rows[0].FileCount);
    }

    [Fact]
    public void BuildCommitWarningCallout_WithEmptyTitle_ReturnsWarning()
    {
        var rows = new List<PlanContentHelpers.CommitRow>
        {
            new("abc123", "abc123", "", 5),
        };

        var result = PlanContentHelpers.BuildCommitWarningCallout(rows);

        Assert.NotNull(result);
        Assert.IsType<Callout>(result);
    }

    [Fact]
    public void BuildCommitWarningCallout_WithZeroFileCount_ReturnsWarning()
    {
        var rows = new List<PlanContentHelpers.CommitRow>
        {
            new("abc123", "abc123", "Some commit", 0),
        };

        var result = PlanContentHelpers.BuildCommitWarningCallout(rows);

        Assert.NotNull(result);
        Assert.IsType<Callout>(result);
    }

    [Fact]
    public void BuildCommitWarningCallout_WithNullFileCount_ReturnsNull()
    {
        var rows = new List<PlanContentHelpers.CommitRow>
        {
            new("abc123", "abc123", "Some commit", null),
        };

        var result = PlanContentHelpers.BuildCommitWarningCallout(rows);

        Assert.Null(result);
    }

    [Fact]
    public void BuildCommitWarningCallout_AllHealthy_ReturnsNull()
    {
        var rows = new List<PlanContentHelpers.CommitRow>
        {
            new("abc123", "abc123", "First commit", 3),
            new("def456", "def456", "Second commit", 1),
        };

        var result = PlanContentHelpers.BuildCommitWarningCallout(rows);

        Assert.Null(result);
    }

    private class StubGitService(string? commitTitle = null,
        List<(string Status, string FilePath)>? commitFiles = null) : IGitService
    {
        public string? GetCommitTitle(string repoPath, string commitHash) => commitTitle;
        public string? GetCommitDiff(string repoPath, string commitHash) => null;
        public List<(string Status, string FilePath)>? GetCommitFiles(string repoPath, string commitHash) => commitFiles;
        public int? GetCommitFileCount(string repoPath, string commitHash) => commitFiles?.Count;
    }

    private class StubConfigService : IConfigService
    {
        public TendrilSettings Settings => new();
        public string TendrilHome => "";
        public string ConfigPath => "";
        public string PlanFolder => "";
        public List<ProjectConfig> Projects => [];
        public List<LevelConfig> Levels => [];
        public string[] LevelNames => [];
        public EditorConfig Editor => new() { Command = "code", Label = "VS Code" };
        public bool NeedsOnboarding => false;
        public ProjectConfig? GetProject(string name) => null;
        public BadgeVariant GetBadgeVariant(string level) => BadgeVariant.Outline;
        public Colors? GetProjectColor(string projectName) => null;
        public void SaveSettings() { }
        public void SetPendingTendrilHome(string path) { }
        public string? GetPendingTendrilHome() => null;
        public void SetPendingProject(ProjectConfig project) { }
        public ProjectConfig? GetPendingProject() => null;
        public void SetPendingVerificationDefinitions(List<VerificationConfig> definitions) { }
        public List<VerificationConfig>? GetPendingVerificationDefinitions() => null;
        public void CompleteOnboarding(string tendrilHome) { }
        public void OpenInEditor(string path) { }
    }
}
