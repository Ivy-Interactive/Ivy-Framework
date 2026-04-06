using PostHog;

namespace Ivy.Tendril.Services;

public class TelemetryService : ITelemetryService, IAsyncDisposable
{
    private readonly PostHogClient? _client;
    private readonly string _distinctId;

    public TelemetryService(bool enabled)
    {
        if (!enabled)
        {
            _client = null;
            _distinctId = "";
            return;
        }

        // Public key — safe to expose (like a website tracking snippet)
        _client = new PostHogClient(new PostHogOptions
        {
            ProjectApiKey = "phc_uHeJHFURzThFPnizzGMzLEimLWnRAuqy8DunK8N3oYcd"
        });
        _distinctId = GetOrCreateAnonymousId();
    }

    public void TrackAppStarted()
    {
        _client?.Capture(_distinctId, "app_started");
    }

    public void TrackPlanCreated()
    {
        _client?.Capture(_distinctId, "plan_created");
    }

    public void TrackPrCreated()
    {
        _client?.Capture(_distinctId, "pr_created");
    }

    public void TrackJobCompleted(string jobType, string status, int? durationSeconds)
    {
        _client?.Capture(_distinctId, "job_completed", new Dictionary<string, object>
        {
            ["job_type"] = jobType,
            ["status"] = status,
            ["duration_seconds"] = durationSeconds ?? 0
        });
    }

    private static string GetOrCreateAnonymousId()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Tendril");
        Directory.CreateDirectory(dir);
        var idFile = Path.Combine(dir, ".anonymous-id");

        if (File.Exists(idFile))
        {
            var existing = File.ReadAllText(idFile).Trim();
            if (!string.IsNullOrEmpty(existing)) return existing;
        }

        var newId = Guid.NewGuid().ToString();
        File.WriteAllText(idFile, newId);
        return newId;
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
            await _client.DisposeAsync();
    }
}
