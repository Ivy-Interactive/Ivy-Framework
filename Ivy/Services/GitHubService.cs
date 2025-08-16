using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ivy.Services;

public record Contributor(
    string Login,
    string AvatarUrl,
    string HtmlUrl,
    int Contributions,
    string LastCommitDate);

public interface IGitHubService
{
    Task<List<Contributor>> GetFileContributorsAsync(string filePath);
}

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GitHubService> _logger;
    private readonly string? _githubToken;
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;

    public GitHubService(
        HttpClient httpClient,
        ICacheService cacheService,
        ILogger<GitHubService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _logger = logger;
        _githubToken = configuration["GitHub:Token"];
        _repositoryOwner = configuration["GitHub:RepositoryOwner"] ?? "Ivy-Interactive";
        _repositoryName = configuration["GitHub:RepositoryName"] ?? "Ivy-Framework";

        // Setup HTTP client headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework-Docs");
        if (!string.IsNullOrEmpty(_githubToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_githubToken}");
        }
    }

    public async Task<List<Contributor>> GetFileContributorsAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return new List<Contributor>();
        }

        var cacheKey = $"github_contributors_{_repositoryOwner}_{_repositoryName}_{filePath.Replace('/', '_').Replace('\\', '_')}";

        try
        {
            // Try to get from cache first (1 hour TTL as per design doc)
            var cachedContributors = await _cacheService.GetAsync<List<Contributor>>(cacheKey);
            if (cachedContributors != null && cachedContributors.Any())
            {
                _logger.LogDebug("Retrieved contributors for {FilePath} from cache", filePath);
                return cachedContributors;
            }

            // Fetch from GitHub API
            var contributors = await FetchContributorsFromGitHubAsync(filePath);

            // Cache for 1 hour
            if (contributors.Any())
            {
                await _cacheService.SetAsync(cacheKey, contributors, TimeSpan.FromHours(1));
                _logger.LogDebug("Cached contributors for {FilePath}", filePath);
            }

            return contributors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contributors for file: {FilePath}", filePath);
            return new List<Contributor>();
        }
    }

    private async Task<List<Contributor>> FetchContributorsFromGitHubAsync(string filePath)
    {
        try
        {
            // Get commits for the specific file
            var commitsUrl = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/commits?path={Uri.EscapeDataString(filePath)}&per_page=100";

            _logger.LogDebug("Fetching commits from: {Url}", commitsUrl);

            var response = await _httpClient.GetAsync(commitsUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub API returned {StatusCode} for file: {FilePath}", response.StatusCode, filePath);
                return new List<Contributor>();
            }

            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);
            var commits = document.RootElement;

            // Group commits by author and count contributions
            var contributorMap = new Dictionary<string, Contributor>();

            foreach (var commit in commits.EnumerateArray())
            {
                var author = commit.GetProperty("author");
                var commitInfo = commit.GetProperty("commit");
                var authorInfo = commitInfo.GetProperty("author");

                // Skip if no author (can happen with GitHub's web-based commits)
                if (author.ValueKind == JsonValueKind.Null)
                    continue;

                var login = author.GetProperty("login").GetString() ?? "";
                var avatarUrl = author.GetProperty("avatar_url").GetString() ?? "";
                var htmlUrl = author.GetProperty("html_url").GetString() ?? "";
                var commitDate = authorInfo.GetProperty("date").GetString() ?? "";

                if (contributorMap.TryGetValue(login, out var existingContributor))
                {
                    // Update contribution count and last commit date if this one is newer
                    var currentDate = DateTime.Parse(commitDate);
                    var existingDate = DateTime.Parse(existingContributor.LastCommitDate);

                    contributorMap[login] = existingContributor with
                    {
                        Contributions = existingContributor.Contributions + 1,
                        LastCommitDate = currentDate > existingDate ? commitDate : existingContributor.LastCommitDate
                    };
                }
                else
                {
                    contributorMap[login] = new Contributor(
                        Login: login,
                        AvatarUrl: avatarUrl,
                        HtmlUrl: htmlUrl,
                        Contributions: 1,
                        LastCommitDate: commitDate
                    );
                }
            }

            // Return top 5 contributors ordered by contribution count, then by most recent commit
            return contributorMap.Values
                .OrderByDescending(c => c.Contributions)
                .ThenByDescending(c => DateTime.Parse(c.LastCommitDate))
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contributors from GitHub for file: {FilePath}", filePath);
            return new List<Contributor>();
        }
    }
}
