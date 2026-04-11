using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class WorktreeLifecycleLoggerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly WorktreeLifecycleLogger _logger;

    public WorktreeLifecycleLoggerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"wt-lifecycle-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _logger = new WorktreeLifecycleLogger(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void LogCreation_WritesFormattedEntry()
    {
        _logger.LogCreation("03058", @"D:\Repos\MyRepo", @"D:\Plans\03058\worktrees\MyRepo", "plan-03058-MyRepo");

        var logFile = Path.Combine(_tempDir, "Logs", "worktrees.log");
        Assert.True(File.Exists(logFile));

        var content = File.ReadAllText(logFile);
        Assert.Contains("[03058]", content);
        Assert.Contains("[Creation]", content);
        Assert.Contains("repo=\"D:\\Repos\\MyRepo\"", content);
        Assert.Contains("branch=\"plan-03058-MyRepo\"", content);
    }

    [Fact]
    public void LogCreationFailed_WritesErrorEntry()
    {
        _logger.LogCreationFailed("03058", @"D:\Repos\MyRepo", @"D:\Plans\worktrees\MyRepo", "branch already exists");

        var content = ReadLogContent();
        Assert.Contains("[CreationFailed]", content);
        Assert.Contains("error=\"branch already exists\"", content);
    }

    [Fact]
    public void LogCleanupAttempt_WritesGitFileExistsFlag()
    {
        _logger.LogCleanupAttempt("03058", @"D:\Plans\worktrees\MyRepo", "TerminalState(Completed)", true);

        var content = ReadLogContent();
        Assert.Contains("[CleanupAttempt]", content);
        Assert.Contains("trigger=\"TerminalState(Completed)\"", content);
        Assert.Contains("gitFileExists=\"True\"", content);
    }

    [Fact]
    public void LogCleanupSuccess_WritesSuccessEntry()
    {
        _logger.LogCleanupSuccess("03058", @"D:\Plans\worktrees\MyRepo");

        var content = ReadLogContent();
        Assert.Contains("[CleanupSuccess]", content);
        Assert.Contains("worktree=\"D:\\Plans\\worktrees\\MyRepo\"", content);
    }

    [Fact]
    public void LogCleanupFailed_WritesErrorEntry()
    {
        _logger.LogCleanupFailed("03058", @"D:\Plans\worktrees\MyRepo", "Access denied");

        var content = ReadLogContent();
        Assert.Contains("[CleanupFailed]", content);
        Assert.Contains("error=\"Access denied\"", content);
    }

    [Fact]
    public async Task ConcurrentWrites_AllEntriesWritten()
    {
        const int threadCount = 10;
        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            var planId = $"{i:D5}";
            tasks[i] = Task.Run(() =>
                _logger.LogCreation(planId, @"D:\Repos\MyRepo", @"D:\Plans\worktrees\MyRepo", "branch"));
        }

        await Task.WhenAll(tasks);

        var lines = File.ReadAllLines(Path.Combine(_tempDir, "Logs", "worktrees.log"));
        Assert.Equal(threadCount, lines.Length);
    }

    [Fact]
    public void LogCreatesDirectoryIfNotExists()
    {
        var nestedDir = Path.Combine(_tempDir, "nested", "deep");
        var logger = new WorktreeLifecycleLogger(nestedDir);

        logger.LogCreation("03058", "repo", "wt", "branch");

        Assert.True(File.Exists(Path.Combine(nestedDir, "Logs", "worktrees.log")));
    }

    [Theory]
    [InlineData(@"D:\Tendril\Plans\03058-CentralizeWorktreeLifecycleLogging", "03058")]
    [InlineData(@"D:\Plans\00015-LogWarning", "00015")]
    [InlineData(@"D:\Plans\unknown-folder", "unknown")]
    [InlineData(@"D:\Plans\03058-Test\", "03058")]
    public void ExtractPlanId_ParsesCorrectly(string path, string expected)
    {
        Assert.Equal(expected, WorktreeLifecycleLogger.ExtractPlanId(path));
    }

    [Fact]
    public void LogCreationFailed_EscapesQuotesInError()
    {
        _logger.LogCreationFailed("03058", "repo", "wt", "error with \"quotes\" inside");

        var content = ReadLogContent();
        Assert.Contains("error=\"error with \\\"quotes\\\" inside\"", content);
    }

    private string ReadLogContent()
    {
        return File.ReadAllText(Path.Combine(_tempDir, "Logs", "worktrees.log"));
    }
}
