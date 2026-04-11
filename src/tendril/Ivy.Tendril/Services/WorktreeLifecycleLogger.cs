using System.Globalization;
using System.Text.RegularExpressions;

namespace Ivy.Tendril.Services;

public class WorktreeLifecycleLogger : IWorktreeLifecycleLogger
{
    private static readonly Regex PlanIdRegex = new(@"(\d{5})-", RegexOptions.Compiled);
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public WorktreeLifecycleLogger(string tendrilHome)
    {
        var logDir = Path.Combine(tendrilHome, "Logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, "worktrees.log");
    }

    public void LogCreation(string planId, string repoPath, string worktreePath, string branch)
    {
        WriteEntry(planId, "Creation",
            $"repo=\"{repoPath}\" worktree=\"{worktreePath}\" branch=\"{branch}\"");
    }

    public void LogCreationFailed(string planId, string repoPath, string worktreePath, string error)
    {
        WriteEntry(planId, "CreationFailed",
            $"repo=\"{repoPath}\" worktree=\"{worktreePath}\" error=\"{Escape(error)}\"");
    }

    public void LogCleanupAttempt(string planId, string worktreePath, string trigger, bool gitFileExists)
    {
        WriteEntry(planId, "CleanupAttempt",
            $"worktree=\"{worktreePath}\" trigger=\"{trigger}\" gitFileExists=\"{gitFileExists}\"");
    }

    public void LogCleanupSuccess(string planId, string worktreePath)
    {
        WriteEntry(planId, "CleanupSuccess", $"worktree=\"{worktreePath}\"");
    }

    public void LogCleanupFailed(string planId, string worktreePath, string error)
    {
        WriteEntry(planId, "CleanupFailed",
            $"worktree=\"{worktreePath}\" error=\"{Escape(error)}\"");
    }

    public static string ExtractPlanId(string planFolderPath)
    {
        var folderName = Path.GetFileName(planFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var match = PlanIdRegex.Match(folderName);
        return match.Success ? match.Groups[1].Value : "unknown";
    }

    private void WriteEntry(string planId, string eventType, string metadata)
    {
        var timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        var entry = $"[{timestamp}] [{planId}] [{eventType}] {metadata}";

        lock (_lock)
        {
            File.AppendAllText(_logFilePath, entry + Environment.NewLine);
        }
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
}
