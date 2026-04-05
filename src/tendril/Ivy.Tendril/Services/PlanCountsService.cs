namespace Ivy.Tendril.Services;

public record PlanCounts(int Drafts, int ActiveJobs, int Reviews, int Icebox, int Recommendations);

public class PlanCountsService : IDisposable
{
    private readonly PlanReaderService _planReaderService;
    private readonly JobService _jobService;
    private readonly PlanWatcherService _planWatcher;
    private PlanCounts _current;

    public event Action? CountsChanged;

    public PlanCounts Current => _current;

    public PlanCountsService(PlanReaderService planReaderService, JobService jobService, PlanWatcherService planWatcher)
    {
        _planReaderService = planReaderService;
        _jobService = jobService;
        _planWatcher = planWatcher;
        _current = ComputeCounts();
        _planWatcher.PlansChanged += OnSourceChanged;
        _jobService.JobsChanged += OnSourceChanged;
    }

    private void OnSourceChanged()
    {
        Refresh();
    }

    private void Refresh()
    {
        var updated = ComputeCounts();
        if (updated != _current)
        {
            _current = updated;
            CountsChanged?.Invoke();
        }
    }

    private PlanCounts ComputeCounts()
    {
        var snapshot = _planReaderService.ComputePlanCounts();
        var jobs = _jobService.GetJobs();

        return new PlanCounts(
            Drafts: snapshot.Drafts + snapshot.Failed,
            ActiveJobs: jobs.Count(j => j.Status == "Running" || j.Status == "Queued"),
            Reviews: snapshot.ReadyForReview + snapshot.Failed,
            Icebox: snapshot.Icebox,
            Recommendations: snapshot.PendingRecommendations
        );
    }

    public void Dispose()
    {
        _planWatcher.PlansChanged -= OnSourceChanged;
        _jobService.JobsChanged -= OnSourceChanged;
    }
}
