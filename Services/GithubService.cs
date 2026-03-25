using System.Diagnostics;

namespace Ivy.Tendril.Services;

public class GithubService
{
    private readonly ConfigService _config;
    private readonly Dictionary<string, List<string>> _assigneeCache = new();

    public GithubService(ConfigService config)
    {
        _config = config;
    }

    public List<RepoConfig> GetRepos() => _config.Settings.Repos;

    public async Task<List<string>> GetAssigneesAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_assigneeCache.TryGetValue(key, out var cached))
            return cached;

        var assignees = await FetchAssigneesFromGhCliAsync(owner, repo);
        _assigneeCache[key] = assignees;
        return assignees;
    }

    private static async Task<List<string>> FetchAssigneesFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", $"api repos/{owner}/{repo}/assignees --jq \".[].login\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null) return new();

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0) return new();

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
        }
        catch
        {
            return new();
        }
    }
}
