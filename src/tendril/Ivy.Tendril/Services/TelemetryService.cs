using Ivy.Tendril.Apps.Jobs;
using Microsoft.Extensions.Logging;
using PostHog;

namespace Ivy.Tendril.Services;

public class TelemetryService : ITelemetryService, IAsyncDisposable
{
    private readonly PostHogClient? _client;
    private readonly string _distinctId;
    private readonly ILogger<TelemetryService>? _logger;

    public TelemetryService(bool enabled, ILogger<TelemetryService>? logger = null)
    {
        _logger = logger;

        if (!enabled)
        {
            _client = null;
            _distinctId = "";
            return;
        }

        try
        {
            // Public key — safe to expose (like a website tracking snippet)
            var sessionId = Guid.NewGuid().ToString();
            _client = new PostHogClient(new PostHogOptions
            {
                ProjectApiKey = "phc_uHeJHFURzThFPnizzGMzLEimLWnRAuqy8DunK8N3oYcd",
                HostUrl = new Uri("https://eu.i.posthog.com"),
                SuperProperties = new Dictionary<string, object>
                {
                    ["$session_id"] = sessionId
                }
            });
            _distinctId = GetOrCreateAnonymousId();
            _logger?.LogDebug("TelemetryService initialized with anonymous ID: {DistinctId}", _distinctId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize PostHog client");
            _client = null;
            _distinctId = "";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
            try
            {
                await FlushAsync();
                await _client.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during telemetry service disposal");
            }
    }

    public async Task IdentifyAsync(string appVersion)
    {
        if (_client == null) return;

        try
        {
            await _client.IdentifyAsync(
                _distinctId,
                personPropertiesToSet: new Dictionary<string, object>
                {
                    ["app_version"] = appVersion,
                    ["os"] = Environment.OSVersion.Platform.ToString()
                },
                personPropertiesToSetOnce: new Dictionary<string, object>
                {
                    ["first_seen"] = DateTime.UtcNow.ToString("o")
                },
                cancellationToken: default);
            _logger?.LogDebug("Identified anonymous user with person properties");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to identify user in PostHog");
        }
    }

    public void TrackAppStarted(AppStartContext context)
    {
        try
        {
            _client?.Capture(_distinctId, "app_started", new Dictionary<string, object>
            {
                ["version"] = context.Version,
                ["project_count"] = context.ProjectCount,
                ["llm_configured"] = context.LlmConfigured
            });
            _logger?.LogDebug("Tracked app_started event");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to track app_started event");
        }
    }

    public void TrackPlanCreated(PlanCreatedContext context)
    {
        try
        {
            _client?.Capture(_distinctId, "plan_created", new Dictionary<string, object>
            {
                ["level"] = context.Level,
                ["duration_seconds"] = context.DurationSeconds ?? 0
            });
            _logger?.LogDebug("Tracked plan_created event");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to track plan_created event");
        }
    }

    public void TrackPrCreated(PrCreatedContext context)
    {
        try
        {
            _client?.Capture(_distinctId, "pr_created", new Dictionary<string, object>
            {
                ["duration_seconds"] = context.DurationSeconds ?? 0
            });
            _logger?.LogDebug("Tracked pr_created event");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to track pr_created event");
        }
    }

    public void TrackJobCompleted(string jobType, JobStatus status, int? durationSeconds)
    {
        try
        {
            _client?.Capture(_distinctId, "job_completed", new Dictionary<string, object>
            {
                ["job_type"] = jobType,
                ["status"] = status.ToString(),
                ["duration_seconds"] = durationSeconds ?? 0
            });
            _logger?.LogDebug("Tracked job_completed event: {JobType} - {Status}", jobType, status);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to track job_completed event");
        }
    }

    public void TrackPlanStateTransition(string fromState, string toState)
    {
        try
        {
            _client?.Capture(_distinctId, "plan_state_transition", new Dictionary<string, object>
            {
                ["from_state"] = fromState,
                ["to_state"] = toState
            });
            _logger?.LogDebug("Tracked plan_state_transition event: {FromState} -> {ToState}", fromState, toState);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to track plan_state_transition event");
        }
    }

    public async Task FlushAsync()
    {
        if (_client == null) return;

        try
        {
            await _client.FlushAsync();
            _logger?.LogDebug("Flushed telemetry events to PostHog");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to flush telemetry events");
        }
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
            var existing = FileHelper.ReadAllText(idFile).Trim();
            if (!string.IsNullOrEmpty(existing)) return existing;
        }

        var newId = Guid.NewGuid().ToString();
        FileHelper.WriteAllText(idFile, newId);
        return newId;
    }
}