using System.Diagnostics;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class ExecutePlanWorktreeCleanupTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _scriptPath;

    public ExecutePlanWorktreeCleanupTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-ep-cleanup-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        _scriptPath = Path.GetFullPath(Path.Combine(
            System.AppContext.BaseDirectory, "..", "..", "..", "..",
            "Ivy.Tendril", "Promptwares", "ExecutePlan", "Tools", "Cleanup-Worktrees.ps1"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateFakePlan(string name)
    {
        var planDir = Path.Combine(_tempDir, name);
        Directory.CreateDirectory(planDir);
        File.WriteAllText(Path.Combine(planDir, "plan.yaml"),
            "state: Completed\nproject: Test\ntitle: Test Plan\nupdated: 2020-01-01T00:00:00Z\n");
        return planDir;
    }

    private string CreateWorktreeDir(string planDir, string repoName, bool withGitFile = false)
    {
        var wtDir = Path.Combine(planDir, "worktrees", repoName);
        Directory.CreateDirectory(wtDir);
        File.WriteAllText(Path.Combine(wtDir, "dummy.cs"), "// test");

        if (withGitFile)
            File.WriteAllText(Path.Combine(wtDir, ".git"),
                "gitdir: /nonexistent/.git/worktrees/" + repoName);

        return wtDir;
    }

    private (int exitCode, string output) RunCleanupScript(string planPath, Dictionary<string, string>? envVars = null)
    {
        var psi = new ProcessStartInfo("pwsh", $"-NoProfile -File \"{_scriptPath}\" -PlanPath \"{planPath}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (envVars != null)
        {
            foreach (var kv in envVars)
                psi.EnvironmentVariables[kv.Key] = kv.Value;
        }

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(30000);
        return (process.ExitCode, stdout + stderr);
    }

    [Fact]
    public void CleanupScript_Removes_Worktree_Directory()
    {
        var planDir = CreateFakePlan("01000-CleanupTest");
        var wtDir = CreateWorktreeDir(planDir, "TestRepo");

        Assert.True(Directory.Exists(wtDir));

        var (exitCode, _) = RunCleanupScript(planDir);

        Assert.Equal(0, exitCode);
        Assert.False(Directory.Exists(wtDir), "Worktree directory should be removed");
        Assert.False(Directory.Exists(Path.Combine(planDir, "worktrees")),
            "Worktrees parent directory should be removed");
    }

    [Fact]
    public void CleanupScript_Handles_Orphaned_Worktree_Without_GitFile()
    {
        var planDir = CreateFakePlan("02000-OrphanTest");
        var wtDir = CreateWorktreeDir(planDir, "OrphanRepo");

        var (exitCode, _) = RunCleanupScript(planDir);

        Assert.Equal(0, exitCode);
        Assert.False(Directory.Exists(wtDir), "Orphaned worktree without .git should still be cleaned");
    }

    [Fact]
    public void CleanupScript_Handles_Stale_GitFile()
    {
        var planDir = CreateFakePlan("03000-StaleGitTest");
        var wtDir = CreateWorktreeDir(planDir, "StaleRepo", withGitFile: true);

        var (exitCode, _) = RunCleanupScript(planDir);

        Assert.Equal(0, exitCode);
        Assert.False(Directory.Exists(wtDir), "Worktree with stale .git should be cleaned");
    }

    [Fact]
    public void CleanupScript_NoOp_When_No_Worktrees_Directory()
    {
        var planDir = CreateFakePlan("04000-NoWorktreeDir");

        var (exitCode, _) = RunCleanupScript(planDir);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void CleanupScript_Removes_Multiple_Worktrees()
    {
        var planDir = CreateFakePlan("05000-MultiWorktree");
        CreateWorktreeDir(planDir, "RepoA");
        CreateWorktreeDir(planDir, "RepoB");
        CreateWorktreeDir(planDir, "RepoC");

        var (exitCode, _) = RunCleanupScript(planDir);

        Assert.Equal(0, exitCode);
        Assert.False(Directory.Exists(Path.Combine(planDir, "worktrees")),
            "All worktrees and parent directory should be removed");
    }

    [Fact]
    public void GracePeriod_Is_Ten_Minutes()
    {
        var field = typeof(WorktreeCleanupService)
            .GetField("GracePeriod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(field);
        var value = (TimeSpan)field!.GetValue(null)!;
        Assert.Equal(TimeSpan.FromMinutes(10), value);
    }
}
