namespace Ivy.Tendril.Services;

public record GitHubIssue(
    int Number,
    string Title,
    string? Body,
    string[] Labels,
    string[] Assignees
);

public interface IGithubService
{
    List<RepoConfig> GetRepos();
    Task<(List<string> assignees, string? error)> GetAssigneesAsync(string owner, string repo);
    Task<(List<string> labels, string? error)> GetLabelsAsync(string owner, string repo);
    Task<Dictionary<string, string>> GetPrStatusesAsync(string owner, string repo);
    Task<(List<GitHubIssue> issues, string? error)> SearchIssuesAsync(string owner, string repo, string? query, string? assignee, string[]? labels);
}