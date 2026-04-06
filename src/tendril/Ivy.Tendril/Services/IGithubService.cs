namespace Ivy.Tendril.Services;

public interface IGithubService
{
    List<RepoConfig> GetRepos();
    Task<List<string>> GetAssigneesAsync(string owner, string repo);
    Task<List<string>> GetLabelsAsync(string owner, string repo);
}
