using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class WorktreeCleanupServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _plansDir;
    private readonly WorktreeCleanupService _service;

    public WorktreeCleanupServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-cleanup-test-{Guid.NewGuid()}");
        _plansDir = Path.Combine(_tempDir, "plans");
        Directory.CreateDirectory(_plansDir);
        _service = new WorktreeCleanupService(_plansDir, NullLogger<WorktreeCleanupService>.Instance);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreatePlan(string folderName, string state, DateTime? updated = null)
    {
        var dir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(dir);
        var updatedStr = (updated ?? DateTime.UtcNow.AddHours(-2)).ToString("o");
        File.WriteAllText(Path.Combine(dir, "plan.yaml"),
            $"state: {state}\nproject: Test\ntitle: Test Plan\nupdated: {updatedStr}\n");
        return dir;
    }

    private void CreateWorktreeDir(string planDir, string repoName, bool withContent = true)
    {
        var worktreeDir = Path.Combine(planDir, "worktrees", repoName);
        Directory.CreateDirectory(worktreeDir);
        if (withContent)
            File.WriteAllText(Path.Combine(worktreeDir, "dummy.txt"), "test");
    }

    private static void CreateEmptyWorktreesDir(string planDir)
    {
        Directory.CreateDirectory(Path.Combine(planDir, "worktrees"));
    }

    [Fact]
    public void RunCleanup_Skips_Active_State_Plans()
    {
        var activeStates = new[] { "Draft", "Building", "Executing", "ReadyForReview", "Blocked" };
        foreach (var state in activeStates)
        {
            var dir = CreatePlan($"01{Array.IndexOf(activeStates, state):D3}-{state}Plan", state);
            CreateWorktreeDir(dir, "Repo");
        }

        _service.RunCleanup();

        foreach (var state in activeStates)
        {
            var worktreesDir = Path.Combine(_plansDir, $"01{Array.IndexOf(activeStates, state):D3}-{state}Plan", "worktrees");
            Assert.True(Directory.Exists(worktreesDir), $"Worktrees for {state} plan should not be removed");
        }
    }

    [Fact]
    public void RunCleanup_Cleans_Terminal_State_Plans_Past_Grace_Period()
    {
        var terminalStates = new[] { "Completed", "Failed", "Skipped", "Icebox" };
        foreach (var state in terminalStates)
        {
            var dir = CreatePlan($"02{Array.IndexOf(terminalStates, state):D3}-{state}Plan", state,
                DateTime.UtcNow.AddHours(-2));
            // Create empty worktrees dir (simulates post-RemoveWorktrees state where git cleaned subdirs)
            CreateEmptyWorktreesDir(dir);
        }

        _service.RunCleanup();

        foreach (var state in terminalStates)
        {
            var worktreesDir = Path.Combine(_plansDir, $"02{Array.IndexOf(terminalStates, state):D3}-{state}Plan", "worktrees");
            Assert.False(Directory.Exists(worktreesDir), $"Empty worktrees dir for {state} plan should be removed");
        }
    }

    [Fact]
    public void RunCleanup_Respects_Grace_Period()
    {
        // Plan just failed 5 minutes ago — should NOT be cleaned (grace period is 10 minutes)
        var dir = CreatePlan("03000-RecentFail", "Failed", DateTime.UtcNow.AddMinutes(-5));
        CreateWorktreeDir(dir, "Repo");

        _service.RunCleanup();

        var worktreesDir = Path.Combine(dir, "worktrees");
        Assert.True(Directory.Exists(worktreesDir), "Recently failed plan should keep worktrees during grace period");
    }

    [Fact]
    public void RunCleanup_Skips_Plans_Without_Worktrees()
    {
        CreatePlan("04000-NoWorktree", "Failed", DateTime.UtcNow.AddHours(-2));

        // Should not throw
        _service.RunCleanup();
    }

    [Fact]
    public void RunCleanup_Skips_Plans_Without_PlanYaml()
    {
        var dir = Path.Combine(_plansDir, "05000-NoPlanYaml");
        Directory.CreateDirectory(dir);
        var worktreeDir = Path.Combine(dir, "worktrees", "Repo");
        Directory.CreateDirectory(worktreeDir);

        // Should not throw
        _service.RunCleanup();

        Assert.True(Directory.Exists(worktreeDir), "Worktree should not be removed when plan.yaml is missing");
    }

    [Fact]
    public void CleanupPlanWorktrees_Static_Cleans_Terminal_Plan()
    {
        var dir = CreatePlan("06000-StaticTest", "Completed", DateTime.UtcNow.AddHours(-2));
        CreateEmptyWorktreesDir(dir);

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        var worktreesDir = Path.Combine(dir, "worktrees");
        Assert.False(Directory.Exists(worktreesDir));
    }

    [Fact]
    public void CleanupPlanWorktrees_Static_Skips_Active_Plan()
    {
        var dir = CreatePlan("07000-ActivePlan", "Executing", DateTime.UtcNow.AddHours(-2));
        CreateWorktreeDir(dir, "Repo");

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        var worktreesDir = Path.Combine(dir, "worktrees");
        Assert.True(Directory.Exists(worktreesDir));
    }

    [Fact]
    public void RemoveWorktrees_Remains_Functional_After_Extraction()
    {
        // Verify RemoveWorktrees is accessible as internal static
        var dir = CreatePlan("08000-ExtractTest", "Failed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        // No .git file — RemoveWorktrees should skip this entry gracefully
        File.WriteAllText(Path.Combine(worktreeDir, "file.txt"), "test");

        PlanReaderService.RemoveWorktrees(dir);

        // The directory still exists because there's no .git file for git worktree remove,
        // but the method should not throw
        Assert.True(Directory.Exists(worktreeDir));
    }

    [Fact]
    public void CleanupPlanWorktrees_ForceDeletes_Orphan_Without_GitFile()
    {
        // Worktree directory with no .git file — RemoveWorktrees skips it,
        // but the force-delete fallback should clean it up
        var dir = CreatePlan("09000-OrphanNoGit", "Completed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        File.WriteAllText(Path.Combine(worktreeDir, "file.txt"), "orphaned content");

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        Assert.False(Directory.Exists(worktreeDir), "Orphan directory without .git should be force-deleted");
        Assert.False(Directory.Exists(Path.Combine(dir, "worktrees")), "Worktrees directory should be removed");
    }

    [Fact]
    public void CleanupPlanWorktrees_ForceDeletes_Orphan_With_Stale_GitFile()
    {
        // Worktree directory with a .git file pointing to a non-existent repo entry —
        // git worktree remove will fail, but force-delete fallback should clean it up
        var dir = CreatePlan("09001-OrphanStaleGit", "Failed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        File.WriteAllText(Path.Combine(worktreeDir, ".git"),
            "gitdir: /nonexistent/path/.git/worktrees/TestRepo");
        File.WriteAllText(Path.Combine(worktreeDir, "source.cs"), "// stale code");

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        Assert.False(Directory.Exists(worktreeDir), "Orphan directory with stale .git should be force-deleted");
        Assert.False(Directory.Exists(Path.Combine(dir, "worktrees")), "Worktrees directory should be removed");
    }

    [Fact]
    public void CleanupPlanWorktrees_ForceDeletes_ReadOnly_Files()
    {
        // Worktree directory with read-only files (common in git objects) —
        // ClearReadOnlyAttributes should make them deletable
        var dir = CreatePlan("09002-ReadOnlyFiles", "Completed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        var subDir = Path.Combine(worktreeDir, "objects");
        Directory.CreateDirectory(subDir);

        var readOnlyFile = Path.Combine(subDir, "pack.idx");
        File.WriteAllText(readOnlyFile, "binary content");
        File.SetAttributes(readOnlyFile, FileAttributes.ReadOnly);

        var normalFile = Path.Combine(worktreeDir, "HEAD");
        File.WriteAllText(normalFile, "ref: refs/heads/main");

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        Assert.False(Directory.Exists(worktreeDir), "Directory with read-only files should be force-deleted");
        Assert.False(Directory.Exists(Path.Combine(dir, "worktrees")), "Worktrees directory should be removed");
    }

    [Fact]
    public void ForceDeleteDirectory_Deletes_Normal_Directory()
    {
        var dir = Path.Combine(_tempDir, "force-delete-normal");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "file.txt"), "content");

        WorktreeCleanupService.ForceDeleteDirectory(dir);

        Assert.False(Directory.Exists(dir), "Normal directory should be deleted by ForceDeleteDirectory");
    }

    [Fact]
    public void ForceDeleteDirectory_Deletes_ReadOnly_Directory()
    {
        var dir = Path.Combine(_tempDir, "force-delete-readonly");
        var subDir = Path.Combine(dir, "sub");
        Directory.CreateDirectory(subDir);

        var roFile = Path.Combine(subDir, "readonly.dat");
        File.WriteAllText(roFile, "locked content");
        File.SetAttributes(roFile, FileAttributes.ReadOnly);

        WorktreeCleanupService.ForceDeleteDirectory(dir);

        Assert.False(Directory.Exists(dir), "Directory with read-only files should be deleted by ForceDeleteDirectory");
    }

    [Fact]
    public void ForceDeleteDirectory_Handles_Nonexistent_Path_Gracefully()
    {
        var dir = Path.Combine(_tempDir, "force-delete-nonexistent");

        // DirectoryNotFoundException inherits from IOException, so the first catch handles it.
        // The Windows rmdir fallback also tolerates missing paths.
        // Verify the method does not throw on nonexistent directories.
        var ex = Record.Exception(() => WorktreeCleanupService.ForceDeleteDirectory(dir));
        Assert.Null(ex);
    }

    [Fact]
    public void ForceDeleteDirectory_Deletes_Deeply_Nested_Directory()
    {
        var dir = Path.Combine(_tempDir, "force-delete-deep");
        var deep = Path.Combine(dir, "a", "b", "c", "d");
        Directory.CreateDirectory(deep);
        File.WriteAllText(Path.Combine(deep, "leaf.txt"), "deep");

        WorktreeCleanupService.ForceDeleteDirectory(dir);

        Assert.False(Directory.Exists(dir), "Deeply nested directory should be deleted by ForceDeleteDirectory");
    }

    [Fact]
    public void RemoveWorktrees_Logs_Warning_When_GitFile_Missing()
    {
        var dir = CreatePlan("10000-LogWarningTest", "Failed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        File.WriteAllText(Path.Combine(worktreeDir, "file.txt"), "test");

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);

        PlanReaderService.RemoveWorktrees(dir, logger);

        Assert.Single(logEntries);
        Assert.Contains("has no .git file", logEntries[0]);
        Assert.Contains("TestRepo", logEntries[0]);
    }

    [Fact]
    public void CleanupPlanWorktrees_ForceDeletes_DeepNodeModulesPath()
    {
        // Simulates the real-world failure: cleanup must succeed over a deeply nested
        // node_modules tree like the one that crashed on 'helper-string-parser'.
        var dir = CreatePlan("11000-DeepNodeModules", "Completed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");

        var deep = Path.Combine(
            worktreeDir,
            "frontend", "node_modules", "some-pkg", "node_modules",
            "helper-string-parser", "lib", "esm", "helpers");
        Directory.CreateDirectory(deep);

        // Spread several files at different depths.
        File.WriteAllText(Path.Combine(worktreeDir, "package.json"), "{}");
        File.WriteAllText(
            Path.Combine(worktreeDir, "frontend", "node_modules", "some-pkg", "index.js"),
            "module.exports = {};");
        File.WriteAllText(Path.Combine(deep, "parse-array.js"), "exports.parseArray = () => [];");
        File.WriteAllText(Path.Combine(deep, "parse-object.js"), "exports.parseObject = () => ({});");

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        Assert.False(Directory.Exists(worktreeDir),
            "Deeply nested node_modules worktree should be force-deleted");
        Assert.False(Directory.Exists(Path.Combine(dir, "worktrees")),
            "Worktrees directory should be removed");
    }

    [Fact]
    public void ForceDeleteDirectory_Logs_Fallback_When_Initial_Delete_Fails()
    {
        // Windows-only: holding a file handle open forces Directory.Delete to throw,
        // which should trigger the rmdir fallback and produce a log entry.
        if (!OperatingSystem.IsWindows()) return;

        var testDir = Path.Combine(_tempDir, "force-delete-fallback");
        Directory.CreateDirectory(testDir);
        var lockedFile = Path.Combine(testDir, "locked.txt");
        File.WriteAllText(lockedFile, "content");

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);

        // Open the file with exclusive access to force Directory.Delete to fail.
        using (var stream = new FileStream(lockedFile, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            // Expect IOException: Directory.Delete fails because of the lock, and
            // rmdir /s /q also can't delete the file while it's held open.
            var ex = Assert.Throws<IOException>(() =>
                WorktreeCleanupService.ForceDeleteDirectory(testDir, logger));
            Assert.Contains("after 3 retries", ex.Message);
        }

        Assert.Contains(logEntries, e => e.Contains("falling back to rmdir"));

        // Cleanup after the stream is released.
        if (Directory.Exists(testDir))
            Directory.Delete(testDir, true);
    }

    [Fact]
    public void ForceDeleteDirectory_Retries_Before_Throwing()
    {
        // Windows-only: keep a file locked for the entire call so every retry
        // attempt fails. Verify retry log entries appear and the final message
        // mentions "after 3 retries". Slow test (~3s back-off).
        if (!OperatingSystem.IsWindows()) return;

        var testDir = Path.Combine(_tempDir, "force-delete-retry");
        Directory.CreateDirectory(testDir);
        var lockedFile = Path.Combine(testDir, "locked.txt");
        File.WriteAllText(lockedFile, "content");

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);

        using (var stream = new FileStream(lockedFile, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var ex = Assert.Throws<IOException>(() =>
                WorktreeCleanupService.ForceDeleteDirectory(testDir, logger));
            Assert.Contains("after 3 retries", ex.Message);
        }

        Assert.Contains(logEntries, e => e.Contains("retry 1/3"));
        Assert.Contains(logEntries, e => e.Contains("retry 2/3"));
        Assert.Contains(logEntries, e => e.Contains("retry 3/3"));

        if (Directory.Exists(testDir))
            Directory.Delete(testDir, true);
    }

    [Fact]
    public void CleanupPlanWorktrees_LogsMissingGitFileAge()
    {
        var dir = CreatePlan("12000-MissingGitAge", "Completed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        File.WriteAllText(Path.Combine(worktreeDir, "file.txt"), "content");

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);

        WorktreeCleanupService.CleanupPlanWorktrees(dir, logger);

        Assert.Contains(logEntries, e => e.Contains("no .git file") && e.Contains("ago"));
        Assert.False(Directory.Exists(worktreeDir), "Orphan directory should still be cleaned up");
    }

    [Fact]
    public void CleanupPlanWorktrees_HandlesVeryRecentOrphanedDirectory()
    {
        var dir = CreatePlan("12001-RecentOrphan", "Completed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        File.WriteAllText(Path.Combine(worktreeDir, "file.txt"), "content");

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);

        WorktreeCleanupService.CleanupPlanWorktrees(dir, logger);

        var ageLog = logEntries.FirstOrDefault(e => e.Contains("no .git file") && e.Contains("ago"));
        Assert.NotNull(ageLog);
        Assert.False(Directory.Exists(worktreeDir), "Very recent orphan should still be cleaned up");
    }

    [Fact]
    public void CleanupRecursiveArtifacts_Removes_Nested_Plans_In_Worktrees()
    {
        var dir = CreatePlan("13000-RecursiveTest", "Executing");
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        var nestedPlans = Path.Combine(worktreeDir, "src", "tendril", "Plans", "01234-OldPlan");
        Directory.CreateDirectory(nestedPlans);
        File.WriteAllText(Path.Combine(nestedPlans, "plan.yaml"), "state: Completed");

        _service.RunCleanup();

        Assert.False(Directory.Exists(Path.Combine(worktreeDir, "src", "tendril", "Plans")),
            "Nested Plans directory should be deleted");
        Assert.True(Directory.Exists(worktreeDir),
            "Worktree repo directory should remain");
    }

    [Fact]
    public void CleanupRecursiveArtifacts_Handles_Multiple_Nesting_Levels()
    {
        var dir = CreatePlan("13001-MultiLevel", "Executing");
        var worktreeDir = Path.Combine(dir, "worktrees", "Repo");

        // Level 1: Plans/B inside worktree
        var level1 = Path.Combine(worktreeDir, "Plans", "B-Plan", "worktrees", "Repo");
        Directory.CreateDirectory(level1);

        // Level 2: Plans/C nested inside level 1
        var level2 = Path.Combine(level1, "Plans", "C-Plan");
        Directory.CreateDirectory(level2);
        File.WriteAllText(Path.Combine(level2, "plan.yaml"), "state: Failed");

        _service.RunCleanup();

        Assert.False(Directory.Exists(Path.Combine(worktreeDir, "Plans")),
            "All nested Plans directories should be removed");
        Assert.True(Directory.Exists(worktreeDir),
            "Top-level worktree directory should remain");
    }

    [Fact]
    public void CleanupRecursiveArtifacts_Skips_Plans_With_No_Worktrees()
    {
        CreatePlan("13002-NoWorktrees", "Executing");

        var ex = Record.Exception(() => _service.RunCleanup());
        Assert.Null(ex);
    }

    [Fact]
    public void CleanupLegacyPromptwaresDirs_Removes_DotPromptwaresInWorktrees()
    {
        var dir = CreatePlan("13010-LegacyCleanup", "Executing");
        var promptwaresDir = Path.Combine(dir, "worktrees", "TestRepo", ".promptwares");
        Directory.CreateDirectory(promptwaresDir);

        _service.CleanupLegacyPromptwaresDirs();

        Assert.False(Directory.Exists(promptwaresDir), ".promptwares directory should be removed");
    }

    [Fact]
    public void CleanupLegacyPromptwaresDirs_Handles_Nested_DotPromptwaresDirs()
    {
        var dir = CreatePlan("13011-NestedLegacy", "Executing");
        var nestedDir = Path.Combine(dir, "worktrees", "Repo", "src", "tendril", ".promptwares");
        Directory.CreateDirectory(nestedDir);
        File.WriteAllText(Path.Combine(nestedDir, "leftover.md"), "old content");

        _service.CleanupLegacyPromptwaresDirs();

        Assert.False(Directory.Exists(nestedDir), "Nested .promptwares directory should be removed");
    }

    [Fact]
    public void CleanupLegacyPromptwaresDirs_Skips_Plans_With_No_Worktrees()
    {
        CreatePlan("13012-NoWorktrees", "Executing");

        var ex = Record.Exception(() => _service.CleanupLegacyPromptwaresDirs());
        Assert.Null(ex);
    }

    [Fact]
    public void CleanupRecursiveArtifacts_Logs_On_Delete_Failure()
    {
        if (!OperatingSystem.IsWindows()) return;

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);
        var service = new WorktreeCleanupService(_plansDir, new LoggerAdapter(logger));

        var dir = CreatePlan("13003-LockedNested", "Executing");
        var worktreeDir = Path.Combine(dir, "worktrees", "Repo");
        var nestedPlans = Path.Combine(worktreeDir, "src", "Plans", "OldPlan");
        Directory.CreateDirectory(nestedPlans);
        var lockedFile = Path.Combine(nestedPlans, "locked.txt");
        File.WriteAllText(lockedFile, "content");

        using (var stream = new FileStream(lockedFile, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            service.RunCleanup();
        }

        Assert.Contains(logEntries, e => e.Contains("Removing recursive Plans artifact") || e.Contains("Failed to delete nested Plans"));

        // Cleanup
        if (Directory.Exists(nestedPlans))
            Directory.Delete(nestedPlans, true);

        service.Dispose();
    }

    [Fact]
    public void CleanupLegacyPromptwaresDirs_Logs_On_Delete_Failure()
    {
        if (!OperatingSystem.IsWindows()) return;

        var dir = CreatePlan("13013-LockedLegacy", "Executing");
        var promptwaresDir = Path.Combine(dir, "worktrees", "TestRepo", ".promptwares");
        Directory.CreateDirectory(promptwaresDir);
        var lockedFile = Path.Combine(promptwaresDir, "locked.txt");
        File.WriteAllText(lockedFile, "content");

        var logEntries = new List<string>();
        var logger = new CapturingLogger(logEntries);
        var service = new WorktreeCleanupService(_plansDir, new CapturingLogger<WorktreeCleanupService>(logEntries));

        using (var stream = new FileStream(lockedFile, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            service.CleanupLegacyPromptwaresDirs();
        }

        Assert.Contains(logEntries, e => e.Contains("Failed to delete legacy .promptwares"));

        // Cleanup
        if (Directory.Exists(promptwaresDir))
            Directory.Delete(promptwaresDir, true);
    }

    private class LoggerAdapter(ILogger inner) : ILogger<WorktreeCleanupService>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => inner.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            inner.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private class CapturingLogger(List<string> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(formatter(state, exception));
        }
    }

    private class CapturingLogger<T>(List<string> entries) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(formatter(state, exception));
        }
    }
}
