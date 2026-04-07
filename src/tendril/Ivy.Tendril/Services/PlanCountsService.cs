using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public record PlanCounts(int Drafts, int ActiveJobs, int Reviews, int Icebox, int Recommendations);

public class PlanCountsService : IPlanCountsService, IDisposable
{
    private readonly IJobService _jobService;
    private readonly IPlanReaderService _planReaderService;
    private readonly IPlanWatcherService _planWatcher;

    public PlanCountsService(IPlanReaderService planReaderService, IJobService jobService,
        IPlanWatcherService planWatcher)
    {
        _planReaderService = planReaderService;
        _jobService = jobService;
        _planWatcher = planWatcher;
        Current = ComputeCounts();
        _planWatcher.PlansChanged += OnPlansSourceChanged;
        _jobService.JobsChanged += OnSourceChanged;
    }

    public event Action? CountsChanged;

    public PlanCounts Current { get; private set; }

    public void Dispose()
    {
        _planWatcher.PlansChanged -= OnPlansSourceChanged;
        _jobService.JobsChanged -= OnSourceChanged;
    }

    private void OnPlansSourceChanged(string? _)
    {
        OnSourceChanged();
    }

    private void OnSourceChanged()
    {
        _planReaderService.InvalidateCaches();
        Refresh();
    }

    private void Refresh()
    {
        var updated = ComputeCounts();
        if (updated != Current)
        {
            Current = updated;
            CountsChanged?.Invoke();
        }
    }

    private PlanCounts ComputeCounts()
    {
        var snapshot = _planReaderService.ComputePlanCounts();
        var jobs = _jobService.GetJobs();

        return new PlanCounts(
            snapshot.Drafts,
            jobs.Count(j => j.Status is JobStatus.Running or JobStatus.Queued or JobStatus.Blocked),
            snapshot.ReadyForReview + snapshot.Failed,
            snapshot.Icebox,
            snapshot.PendingRecommendations
        );
    }
}