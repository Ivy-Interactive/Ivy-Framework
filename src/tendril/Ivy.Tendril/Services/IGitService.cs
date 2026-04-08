namespace Ivy.Tendril.Services;

public interface IGitService
{
    string? GetCommitTitle(string repoPath, string commitHash);
    string? GetCommitDiff(string repoPath, string commitHash);
    List<(string Status, string FilePath)>? GetCommitFiles(string repoPath, string commitHash);
}