using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public interface ITelemetryService
{
    void TrackAppStarted(AppStartContext context);
    void TrackPlanCreated(PlanCreatedContext context);
    void TrackPrCreated(PrCreatedContext context);
    void TrackJobCompleted(string jobType, JobStatus status, int? durationSeconds);
    void TrackPlanStateTransition(int planId, string fromState, string toState);
    Task FlushAsync();
}

public record AppStartContext(
    string Version,
    int ProjectCount,
    bool LlmConfigured);

public record PlanCreatedContext(
    string Project,
    string Level,
    int? DurationSeconds);

public record PrCreatedContext(
    string Project,
    string RepoUrl,
    int? DurationSeconds);
