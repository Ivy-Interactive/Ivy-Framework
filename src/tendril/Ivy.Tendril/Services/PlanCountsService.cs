using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public record PlanCounts(int Drafts, int RunningJobs, int Reviews, int Icebox, int Recommendations);

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
        var plans = _planReaderService.GetPlans();
        var jobs = _jobService.GetJobs();
        return new PlanCounts(
            Drafts: plans.Count(p => p.Status == PlanStatus.Draft),
            RunningJobs: jobs.Count(j => j.Status == "Running"),
            Reviews: plans.Count(p => p.Status is PlanStatus.ReadyForReview or PlanStatus.Failed),
            Icebox: plans.Count(p => p.Status == PlanStatus.Icebox),
            Recommendations: _planReaderService.GetPendingRecommendationsCount()
        );
    }

    public void Dispose()
    {
        _planWatcher.PlansChanged -= OnSourceChanged;
        _jobService.JobsChanged -= OnSourceChanged;
    }
}
