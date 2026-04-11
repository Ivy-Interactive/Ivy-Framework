using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Ivy.Tendril.Services;

public class PrStatusSyncService : IStartable, IDisposable
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

    private readonly IPlanDatabaseService _database;
    private readonly IGithubService _githubService;
    private readonly IPlanReaderService _planReader;
    private readonly ILogger<PrStatusSyncService> _logger;
    private Timer? _timer;

    public PrStatusSyncService(
        IPlanDatabaseService database,
        IGithubService githubService,
        IPlanReaderService planReader,
        ILogger<PrStatusSyncService> logger)
    {
        _database = database;
        _githubService = githubService;
        _planReader = planReader;
        _logger = logger;
    }

    public void Start()
    {
        _timer = new Timer(_ => _ = RunSyncAsync(), null, TimeSpan.FromSeconds(5), CheckInterval);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    internal async Task RunSyncAsync()
    {
        try
        {
            var prUrls = CollectPrUrlsFromPlans();
            if (prUrls.Count == 0) return;

            var existingStatuses = _database.GetAllPrStatuses();
            var urlsToCheck = new List<string>();

            foreach (var url in prUrls)
            {
                if (existingStatuses.TryGetValue(url, out var status) &&
                    string.Equals(status, "Merged", StringComparison.OrdinalIgnoreCase))
                    continue;

                urlsToCheck.Add(url);
            }

            // Also add any new PRs not yet in the database
            foreach (var url in prUrls)
            {
                if (!existingStatuses.ContainsKey(url) && !urlsToCheck.Contains(url))
                    urlsToCheck.Add(url);
            }

            if (urlsToCheck.Count == 0)
            {
                _logger.LogDebug("All {Total} PRs are merged, nothing to check", prUrls.Count);
                return;
            }

            var grouped = GroupByOwnerRepo(urlsToCheck);
            var now = DateTime.UtcNow;

            foreach (var (ownerRepo, urls) in grouped)
            {
                var parts = ownerRepo.Split('/');
                if (parts.Length != 2) continue;

                try
                {
                    var statuses = await _githubService.GetPrStatusesAsync(parts[0], parts[1]);
                    foreach (var url in urls)
                    {
                        var resolvedStatus = statuses.GetValueOrDefault(url, "Open");
                        _database.UpsertPrStatus(url, parts[0], parts[1], resolvedStatus, now);
                    }

                    _logger.LogDebug("Synced {Count} PR statuses for {Repo}", urls.Count, ownerRepo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch PR statuses for {Repo}", ownerRepo);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PR status sync failed");
        }
    }

    private List<string> CollectPrUrlsFromPlans()
    {
        var plans = _planReader.GetPlans();
        return plans
            .Where(p => p.Prs.Count > 0)
            .SelectMany(p => p.Prs)
            .Where(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                          (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static Dictionary<string, List<string>> GroupByOwnerRepo(List<string> prUrls)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in prUrls)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length < 2) continue;
                var key = $"{segments[0]}/{segments[1]}";
                if (!result.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    result[key] = list;
                }

                list.Add(url);
            }
            catch (UriFormatException)
            {
                // skip malformed URLs
            }
        }

        return result;
    }
}
