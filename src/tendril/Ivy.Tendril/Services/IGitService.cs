namespace Ivy.Tendril.Services;

public interface IGitService
{
    string? GetCommitTitle(string repoPath, string commitHash);
    string? GetCommitDiff(string repoPath, string commitHash);
    List<(string Status, string FilePath)>? GetCommitFiles(string repoPath, string commitHash);
    int? GetCommitFileCount(string repoPath, string commitHash);
    string? GetCombinedDiff(string repoPath, string firstCommit, string lastCommit);
    List<(string Status, string FilePath)>? GetCombinedChangedFiles(string repoPath, string firstCommit, string lastCommit);
}