using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Ivy.Tendril.Test.TestHelpers;

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

    [Fact]
    public void GetAllChangesData_WithMultipleCommits_ReturnsCombinedData()
    {
        var gitService = new StubGitService(
            "Test commit",
            [("A", "new.cs"), ("M", "existing.cs")],
            combinedDiff: "diff --git a/new.cs b/new.cs\n+new content",
            combinedFiles: [("A", "new.cs"), ("M", "existing.cs")]
        );
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], ["commit1", "commit2"], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var result = PlanContentHelpers.GetAllChangesData(plan, config, gitService);

        Assert.NotNull(result);
        Assert.Equal(2, result.Files.Count);
        Assert.Equal(1, result.AddedCount);
        Assert.Equal(1, result.ModifiedCount);
        Assert.Equal(0, result.DeletedCount);
        Assert.Contains("new content", result.Diff!);
    }

    [Fact]
    public void GetAllChangesData_WithSingleCommit_UsesCommitDiff()
    {
        var gitService = new StubGitService(
            "Single commit",
            [("A", "file.cs")],
            commitDiff: "diff --git a/file.cs b/file.cs\n+added"
        );
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], ["abc123"], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var result = PlanContentHelpers.GetAllChangesData(plan, config, gitService);

        Assert.NotNull(result);
        Assert.Single(result.Files);
        Assert.Equal(1, result.AddedCount);
        Assert.Contains("added", result.Diff!);
    }

    [Fact]
    public void GetAllChangesData_WithEmptyCommits_ReturnsNull()
    {
        var gitService = new StubGitService("Test", []);
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], [], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var result = PlanContentHelpers.GetAllChangesData(plan, config, gitService);

        Assert.Null(result);
    }

    [Fact]
    public void GetAllChangesData_WithCommitsNotInRepo_ReturnsNull()
    {
        var gitService = new StubGitService(null, null);
        var config = new StubConfigService();

        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Draft,
            ["/fake/repo"], ["unknown1", "unknown2"], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        var plan = new PlanFile(metadata, "", Path.GetTempPath(), "");

        var result = PlanContentHelpers.GetAllChangesData(plan, config, gitService);

        Assert.Null(result);
    }

    private class StubGitService(
        string? commitTitle = null,
        List<(string Status, string FilePath)>? commitFiles = null,
        string? commitDiff = null,
        string? combinedDiff = null,
        List<(string Status, string FilePath)>? combinedFiles = null) : IGitService
    {
        public string? GetCommitTitle(string repoPath, string commitHash) => commitTitle;
        public string? GetCommitDiff(string repoPath, string commitHash) => commitDiff;
        public List<(string Status, string FilePath)>? GetCommitFiles(string repoPath, string commitHash) => commitFiles;
        public int? GetCommitFileCount(string repoPath, string commitHash) => commitFiles?.Count;
        public string? GetCombinedDiff(string repoPath, string firstCommit, string lastCommit) => combinedDiff;

        public List<(string Status, string FilePath)>? GetCombinedChangedFiles(string repoPath, string firstCommit,
            string lastCommit) => combinedFiles;
    }

}
