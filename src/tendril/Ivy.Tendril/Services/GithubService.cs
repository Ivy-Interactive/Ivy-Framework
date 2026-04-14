using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ivy.Helpers;

namespace Ivy.Tendril.Services;

public class GithubService(IConfigService config) : IGithubService
{
    // ConcurrentDictionary required: multiple UseQuery calls from different views/dialogs
    // can fetch different repos simultaneously, causing concurrent writes to different keys.
    private readonly ConcurrentDictionary<string, List<string>> _assigneeCache = new();
    private readonly IConfigService _config = config;
    private readonly ConcurrentDictionary<string, List<string>> _labelCache = new();
    private readonly ConcurrentDictionary<string, RepoConfig?> _repoPathCache = new();
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

    public async Task<(List<string> assignees, string? error)> GetAssigneesAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_assigneeCache.TryGetValue(key, out var cached))
            return (cached, null);

        var (assignees, error) = await FetchAssigneesFromGhCliAsync(owner, repo);

        if (error is null)
            _assigneeCache[key] = assignees;

        return (assignees, error);
    }

    public async Task<(List<string> labels, string? error)> GetLabelsAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_labelCache.TryGetValue(key, out var cached))
            return (cached, null);

        var (labels, error) = await FetchLabelsFromGhCliAsync(owner, repo);

        if (error is null)
            _labelCache[key] = labels;

        return (labels, error);
    }

    public async Task<(Dictionary<string, string> statuses, string? error)> GetPrStatusesAsync(string owner, string repo)
    {
        return await FetchPrStatusesFromGhCliAsync(owner, repo);
    }

    public async Task<(List<GitHubIssue> issues, string? error)> SearchIssuesAsync(string owner, string repo,
        string? query, string? assignee, string[]? labels)
    {
        try
        {
            var args =
                $"issue list --repo {owner}/{repo} --state open --limit 100 --json number,title,body,labels,assignees";
            if (!string.IsNullOrWhiteSpace(query))
                args += $" --search \"{query}\"";
            if (!string.IsNullOrWhiteSpace(assignee))
                args += $" --assignee {assignee}";
            if (labels is { Length: > 0 })
                args += $" --label \"{string.Join(",", labels)}\"";

            var psi = new ProcessStartInfo("gh", args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null)
                return ([], "GitHub CLI (gh) is not available. Please install it from https://cli.github.com/");

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitOrKillAsync(60000);

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh issue list failed for {owner}/{repo}: {stderr}");
                var errorMsg = !string.IsNullOrWhiteSpace(stderr)
                    ? stderr.Trim()
                    : $"GitHub CLI exited with code {process.ExitCode}";
                return ([], errorMsg);
            }

            var issues = ParseIssuesFromJson(output);
            return (issues, null);
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to parse issues for {owner}/{repo}: {ex.Message}");
            return ([], "Invalid response from GitHub CLI. The output could not be parsed.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to search issues for {owner}/{repo}: {ex.Message}");
            return ([], $"Failed to fetch issues: {ex.Message}");
        }
    }

    internal static List<GitHubIssue> ParseIssuesFromJson(string json)
    {
        var issues = new List<GitHubIssue>();
        using var doc = JsonDocument.Parse(json);
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var number = element.GetProperty("number").GetInt32();
            var title = element.GetProperty("title").GetString() ?? "";
            var body = element.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() : null;
            var issueLabels = element.GetProperty("labels").EnumerateArray()
                .Select(l => l.GetProperty("name").GetString() ?? "")
                .ToArray();
            var issueAssignees = element.GetProperty("assignees").EnumerateArray()
                .Select(a => a.GetProperty("login").GetString() ?? "")
                .ToArray();
            issues.Add(new GitHubIssue(number, title, body, issueLabels, issueAssignees));
        }

        return issues;
    }

    public RepoConfig? GetRepoConfigFromPathCached(string repoPath)
    {
        return _repoPathCache.GetOrAdd(repoPath, path => GetRepoConfigFromPath(path));
    }

    internal static RepoConfig? GetRepoConfigFromPath(string repoPath)
    {
        try
        {
            if (!Directory.Exists(repoPath))
            {
                Console.Error.WriteLine($"[GithubService] Repository path does not exist: {repoPath}");
                return null;
            }

            var psi = new ProcessStartInfo("git", "remote get-url origin")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                Console.Error.WriteLine($"[GithubService] Failed to start git process for {repoPath}");
                return null;
            }

            var url = process.StandardOutput.ReadToEnd().Trim();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExitOrKill(10000);

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] git remote get-url failed for {repoPath}: {stderr}");
                return null;
            }

            var config = ParseRepoConfigFromUrl(url);
            if (config is null)
            {
                Console.Error.WriteLine($"[GithubService] Failed to parse remote URL for {repoPath}: {url}");
            }
            return config;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Exception getting repo config for {repoPath}: {ex.Message}");
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

    private static async Task<(List<string> labels, string? error)> FetchLabelsFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", $"api repos/{owner}/{repo}/labels --jq \".[].name\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null)
                return ([], "GitHub CLI (gh) is not available. Please install it from https://cli.github.com/");

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitOrKillAsync(60000);

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh api labels failed for {owner}/{repo}: {stderr}");
                var errorMsg = !string.IsNullOrWhiteSpace(stderr)
                    ? stderr.Trim()
                    : $"GitHub CLI exited with code {process.ExitCode}";
                return ([], errorMsg);
            }

            var labels = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
            return (labels, null);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch labels for {owner}/{repo}: {ex.Message}");
            return ([], $"Failed to fetch labels: {ex.Message}");
        }
    }

    private static async Task<(Dictionary<string, string> statuses, string? error)> FetchPrStatusesFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh",
                $"pr list --repo {owner}/{repo} --limit 100 --state all --json url,state")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null)
                return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                        "GitHub CLI (gh) is not available. Please install it from https://cli.github.com/");

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitOrKillAsync(60000);

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh pr list failed for {owner}/{repo}: {stderr}");
                var errorMsg = !string.IsNullOrWhiteSpace(stderr)
                    ? stderr.Trim()
                    : $"GitHub CLI exited with code {process.ExitCode}";
                return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), errorMsg);
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using var doc = JsonDocument.Parse(output);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var url = element.GetProperty("url").GetString();
                var state = element.GetProperty("state").GetString();
                if (url is not null && state is not null)
                {
                    var titleCase = state switch
                    {
                        "OPEN" => "Open",
                        "CLOSED" => "Closed",
                        "MERGED" => "Merged",
                        _ => state
                    };
                    result[url] = titleCase;
                }
            }

            return (result, null);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch PR statuses for {owner}/{repo}: {ex.Message}");
            return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    $"Failed to fetch PR statuses: {ex.Message}");
        }
    }

    private static async Task<(List<string> assignees, string? error)> FetchAssigneesFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", $"api repos/{owner}/{repo}/assignees --jq \".[].login\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null)
                return ([], "GitHub CLI (gh) is not available. Please install it from https://cli.github.com/");

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitOrKillAsync(60000);

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh api assignees failed for {owner}/{repo}: {stderr}");
                var errorMsg = !string.IsNullOrWhiteSpace(stderr)
                    ? stderr.Trim()
                    : $"GitHub CLI exited with code {process.ExitCode}";
                return ([], errorMsg);
            }

            var assignees = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
            return (assignees, null);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch assignees for {owner}/{repo}: {ex.Message}");
            return ([], $"Failed to fetch assignees: {ex.Message}");
        }
    }
}