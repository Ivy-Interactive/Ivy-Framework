using System.Collections.Concurrent;
using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public class JobService
{
    private readonly ConcurrentDictionary<string, JobItem> _jobs = new();
    private int _counter;

    private static readonly string PromptsRoot =
        Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".promptwares"));

    private static readonly Dictionary<string, string> ScriptPaths = new()
    {
        ["MakePlan"] = Path.Combine(PromptsRoot, "MakePlan.ps1"),
        ["UpdatePlan"] = Path.Combine(PromptsRoot, "UpdatePlan.ps1"),
        ["SplitPlan"] = Path.Combine(PromptsRoot, "SplitPlan.ps1"),
        ["ExpandPlan"] = Path.Combine(PromptsRoot, "ExpandPlan.ps1"),
    };

    public string StartJob(string type, params string[] args)
    {
        var id = $"job-{Interlocked.Increment(ref _counter):D3}";
        var scriptPath = ScriptPaths.GetValueOrDefault(type, "");
        var planFile = args.Length > 0 ? Path.GetFileName(args[0]) : "";

        var job = new JobItem
        {
            Id = id,
            Type = type,
            PlanFile = planFile,
            Queue = "Tendril",
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            ScriptPath = scriptPath,
            Args = args,
        };

        _jobs[id] = job;
        return id;
    }

    public void CompleteJob(string id, int? exitCode)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.Status = exitCode == 0 ? "Completed" : "Failed";
        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;
    }

    public void StopJob(string id)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.CancellationRequested = true;
        job.Status = "Stopped";
        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;
    }

    public void DeleteJob(string id)
    {
        _jobs.TryRemove(id, out _);
    }

    public List<JobItem> GetJobs()
    {
        return _jobs.Values.OrderByDescending(j => j.StartedAt ?? DateTime.MinValue).ToList();
    }

    public JobItem? GetJob(string id)
    {
        return _jobs.GetValueOrDefault(id);
    }

    public void InitializePty(string id, PtyHandle ptyHandle)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.PtyStream = ptyHandle.Stream;
        job.PtyHandleInput = ptyHandle.HandleInput;
        job.PtyHandleResize = ptyHandle.HandleResize;
        job.PtyKill = ptyHandle.Kill;
        job.PtyClosed = ptyHandle.Closed;
        job.PtyExitCode = ptyHandle.ExitCode;
    }

    public void UpdatePtyState(string id, bool closed, int? exitCode)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.PtyClosed = closed;
        job.PtyExitCode = exitCode;
    }
}
