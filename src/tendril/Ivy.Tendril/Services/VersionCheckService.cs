using System.Text.Json;

namespace Ivy.Tendril.Services;

public class VersionCheckService(IHttpClientFactory httpClientFactory) : IVersionCheckService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private VersionInfo? _cachedResult;
    private DateTime _lastCheckTime = DateTime.MinValue;

    public async Task<VersionInfo> CheckForUpdatesAsync()
    {
        if (_cachedResult != null && DateTime.UtcNow - _lastCheckTime < CacheDuration)
            return _cachedResult;

        var currentVersion = GetCurrentVersion();
        string? latestVersion = null;

        try
        {
            latestVersion = await FetchLatestVersionAsync();
        }
        catch
        {
            // Network failure — return null latest version
        }

        var hasUpdate = latestVersion != null
            && Version.TryParse(currentVersion, out var current)
            && Version.TryParse(latestVersion, out var latest)
            && latest > current;

        var now = DateTime.UtcNow;
        _cachedResult = new VersionInfo(currentVersion, latestVersion, hasUpdate, now);
        _lastCheckTime = now;

        return _cachedResult;
    }

    internal static string GetCurrentVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        return version?.ToString(3) ?? "0.0.0";
    }

    private async Task<string?> FetchLatestVersionAsync()
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        var response = await client.GetAsync("https://api.nuget.org/v3-flatcontainer/ivy.tendril/index.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("versions", out var versions))
            return null;

        Version? highest = null;
        string? highestString = null;

        foreach (var v in versions.EnumerateArray())
        {
            var versionString = v.GetString();
            if (versionString == null) continue;
            if (versionString.Contains('-')) continue; // skip pre-release

            if (Version.TryParse(versionString, out var parsed) && (highest == null || parsed > highest))
            {
                highest = parsed;
                highestString = versionString;
            }
        }

        return highestString;
    }
}
