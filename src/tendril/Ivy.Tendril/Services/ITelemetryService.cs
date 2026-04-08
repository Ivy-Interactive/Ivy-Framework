using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public interface ITelemetryService
{
    void TrackAppStarted(AppStartContext context);
    void TrackPlanCreated(PlanCreatedContext context);
    void TrackPrCreated(PrCreatedContext context);
    void TrackJobCompleted(string jobType, JobStatus status, int? durationSeconds);
    void TrackPlanStateTransition(string fromState, string toState);
    Task IdentifyAsync(string appVersion);
    Task FlushAsync();
}

public record AppStartContext(
    string Version,
    int ProjectCount,
    bool LlmConfigured);

public record PlanCreatedContext(
    string Level,
    int? DurationSeconds);

public record PrCreatedContext(
    int? DurationSeconds);