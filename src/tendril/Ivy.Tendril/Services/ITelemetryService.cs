namespace Ivy.Tendril.Services;

public interface ITelemetryService
{
    void TrackAppStarted();
    void TrackPlanCreated();
    void TrackPrCreated();
    void TrackJobCompleted(string jobType, string status, int? durationSeconds);
}
