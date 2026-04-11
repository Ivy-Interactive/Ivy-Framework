namespace Ivy.Tendril.Services;

public interface IWorktreeLifecycleLogger
{
    void LogCreation(string planId, string repoPath, string worktreePath, string branch);
    void LogCreationFailed(string planId, string repoPath, string worktreePath, string error);
    void LogCleanupAttempt(string planId, string worktreePath, string trigger, bool gitFileExists);
    void LogCleanupSuccess(string planId, string worktreePath);
    void LogCleanupFailed(string planId, string worktreePath, string error);
}
