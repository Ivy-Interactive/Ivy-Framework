using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Ivy.Tendril.Services;

public class GithubService(IConfigService config) : IGithubService
{
    private readonly IConfigService _config = config;
    private readonly Dictionary<string, List<string>> _assigneeCache = new();
    private readonly Dictionary<string, List<string>> _labelCache = new();
    private List<RepoConfig>? _repoCache;

    public List<RepoConfig> GetRepos()
    {
        if (_repoCache is not null)
            return _repoCache;

        var uniquePaths = _config.Settings.Projects
            .SelectMany(p => p.RepoPaths)
            .Distinct()
            .ToList();

        var repos = new List<RepoConfig>();
        foreach (var repoPath in uniquePaths)
        {
            var repoConfig = GetRepoConfigFromPath(repoPath);
            if (repoConfig is not null)
                repos.Add(repoConfig);
        }

        _repoCache = repos;
        return repos;
    }

    internal static RepoConfig? GetRepoConfigFromPath(string repoPath)
    {
        try
        {
            var psi = new ProcessStartInfo("git", "remote get-url origin")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
            };

            using var process = Process.Start(psi);
            if (process is null) return null;

            var url = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            if (process.ExitCode != 0) return null;

            return ParseRepoConfigFromUrl(url);
        }
        catch
        {
            return null;
        }
    }

    internal static RepoConfig? ParseRepoConfigFromUrl(string url)
    {
        var match = Regex.Match(url, @"[/:](?<owner>[^/]+)/(?<name>[^/]+?)(?:\.git)?$");
        if (!match.Success) return null;

        return new RepoConfig
        {
            Owner = match.Groups["owner"].Value,
            Name = match.Groups["name"].Value
        };
    }

    public async Task<List<string>> GetAssigneesAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_assigneeCache.TryGetValue(key, out var cached))
            return cached;

        var assignees = await FetchAssigneesFromGhCliAsync(owner, repo);
        _assigneeCache[key] = assignees;
        return assignees;
    }

    public async Task<List<string>> GetLabelsAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_labelCache.TryGetValue(key, out var cached))
            return cached;

        var labels = await FetchLabelsFromGhCliAsync(owner, repo);
        _labelCache[key] = labels;
        return labels;
    }

    private static async Task<List<string>> FetchLabelsFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", $"api repos/{owner}/{repo}/labels --jq \".[].name\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
            };

            using var process = Process.Start(psi);
            if (process is null) return new();

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh api labels failed for {owner}/{repo}: {stderr}");
                return new();
            }

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch labels for {owner}/{repo}: {ex.Message}");
            return new();
        }
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
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
            };

            using var process = Process.Start(psi);
            if (process is null) return new();

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh api assignees failed for {owner}/{repo}: {stderr}");
                return new();
            }

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch assignees for {owner}/{repo}: {ex.Message}");
            return new();
        }
    }
}
