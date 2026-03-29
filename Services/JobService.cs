using System.Collections.Concurrent;
using Ivy;
using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public class JobService
{
    private readonly ConcurrentDictionary<string, JobItem> _jobs = new();
    private int _counter;
    private PlanReaderService? _planReaderService;

    private static readonly string PromptsRoot =
        Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".promptwares"));

    private static readonly Dictionary<string, string> ScriptPaths = new()
    {
        ["MakePlan"] = Path.Combine(PromptsRoot, "MakePlan.ps1"),
        ["UpdatePlan"] = Path.Combine(PromptsRoot, "UpdatePlan.ps1"),
        ["SplitPlan"] = Path.Combine(PromptsRoot, "SplitPlan.ps1"),
        ["ExpandPlan"] = Path.Combine(PromptsRoot, "ExpandPlan.ps1"),
        ["ExecutePlan"] = Path.Combine(PromptsRoot, "ExecutePlan.ps1"),
    };

    public void SetPlanReaderService(PlanReaderService planReaderService)
    {
        _planReaderService = planReaderService;
    }

    public string StartJob(string type, params string[] args)
    {
        var id = $"job-{Interlocked.Increment(ref _counter):D3}";
        var scriptPath = ScriptPaths.GetValueOrDefault(type, "");
        var planFolder = args.Length > 0 ? args[0] : "";

        var job = new JobItem
        {
            Id = id,
            Type = type,
            PlanFile = Path.GetFileName(planFolder),
            Queue = "Tendril",
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            ScriptPath = scriptPath,
            Args = args,
        };

        _jobs[id] = job;

        // Launch process
        var processArgs = new List<string> { "-NoProfile", "-File", scriptPath };
        processArgs.AddRange(args);

        var workingDirectory = Path.GetFullPath(
            Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = string.Join(" ", processArgs.Select(a => $"\"{a}\"")),
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var process = new System.Diagnostics.Process { StartInfo = psi };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                job.OutputLines.Add(e.Data);
                job.LastOutputAt = DateTime.UtcNow;
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                job.OutputLines.Add($"[stderr] {e.Data}");
                job.LastOutputAt = DateTime.UtcNow;
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        job.Process = process;

        // Monitor for completion in background
        Task.Run(async () =>
        {
            await process.WaitForExitAsync();
            CompleteJob(id, process.ExitCode);
        });

        return id;
    }

    public void CompleteJob(string id, int? exitCode)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.Status = exitCode == 0 ? "Completed" : "Failed";
        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

        WriteJobLog(job);
    }

    public void StopJob(string id)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.CancellationRequested = true;
        try { job.Process?.Kill(entireProcessTree: true); } catch { }
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

    private void WriteJobLog(JobItem job)
    {
        if (_planReaderService == null || string.IsNullOrEmpty(job.PlanFile))
            return;

        try
        {
            var duration = job.DurationSeconds.HasValue ? $"{job.DurationSeconds}s" : "unknown";
            var logContent = $"# {job.Type}\n\n" +
                $"- **Status:** {job.Status}\n" +
                $"- **Started:** {job.StartedAt:u}\n" +
                $"- **Completed:** {job.CompletedAt:u}\n" +
                $"- **Duration:** {duration}\n";

            _planReaderService.AddLog(job.PlanFile, job.Type, logContent);
        }
        catch
        {
            // Don't let log writing failures crash the job completion
        }
    }
}
