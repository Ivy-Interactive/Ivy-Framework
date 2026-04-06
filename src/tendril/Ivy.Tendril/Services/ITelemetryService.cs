using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public interface ITelemetryService
{
    void TrackAppStarted();
    void TrackPlanCreated();
    void TrackPrCreated();
    void TrackJobCompleted(string jobType, JobStatus status, int? durationSeconds);
    Task FlushAsync();
}
