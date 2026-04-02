using System.Collections.Concurrent;
using Ivy;
using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public record JobNotification(string Title, string Message, bool IsSuccess);

public class JobService
{
    private readonly ConcurrentDictionary<string, JobItem> _jobs = new();
    private int _counter;
    private PlanReaderService? _planReaderService;
    private readonly TimeSpan _jobTimeout;
    private readonly TimeSpan _staleOutputTimeout;

    public event Action? JobsChanged;
    public ConcurrentQueue<JobNotification> PendingNotifications { get; } = new();

    private static readonly string PromptsRoot =
        Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".promptwares"));

    private static readonly Dictionary<string, string> ScriptPaths = new()
    {
        ["MakePlan"] = Path.Combine(PromptsRoot, "MakePlan.ps1"),
        ["UpdatePlan"] = Path.Combine(PromptsRoot, "UpdatePlan.ps1"),
        ["SplitPlan"] = Path.Combine(PromptsRoot, "SplitPlan.ps1"),
        ["ExpandPlan"] = Path.Combine(PromptsRoot, "ExpandPlan.ps1"),
        ["ExecutePlan"] = Path.Combine(PromptsRoot, "ExecutePlan.ps1"),
        ["IvyFrameworkVerification"] = Path.Combine(PromptsRoot, "IvyFrameworkVerification.ps1"),
        ["MakePr"] = Path.Combine(PromptsRoot, "MakePr.ps1"),
        ["CreateIssue"] = Path.Combine(PromptsRoot, "CreateIssue.ps1"),
    };

    public JobService(ConfigService configService)
    {
        _jobTimeout = TimeSpan.FromMinutes(configService.Settings.JobTimeout);
        _staleOutputTimeout = TimeSpan.FromMinutes(configService.Settings.StaleOutputTimeout);
    }

    public JobService(TimeSpan jobTimeout, TimeSpan staleOutputTimeout)
    {
        _jobTimeout = jobTimeout;
        _staleOutputTimeout = staleOutputTimeout;
    }

    public void SetPlanReaderService(PlanReaderService planReaderService)
    {
        _planReaderService = planReaderService;
    }

    public string StartJob(string type, params string[] args)
    {
        var id = $"job-{Interlocked.Increment(ref _counter):D3}";
        var scriptPath = ScriptPaths.GetValueOrDefault(type, "");

        // Extract plan folder and project from args
        var planFile = "";
        var project = "General";

        // For MakePlan: args are named params like -Description "..." -Project "..."
        // For others: args[0] is the plan folder path
        if (type == "MakePlan")
        {
            planFile = GetNamedArg(args, "-Description") is { } desc
                ? (desc.Length > 80 ? desc[..80] + "..." : desc)
                : "New Plan";
            project = GetNamedArg(args, "-Project") ?? "General";
            if (project == "[Auto]") project = "General";
        }
        else
        {
            var planFolder = args.Length > 0 ? args[0] : "";
            planFile = Path.GetFileName(planFolder);
            if (Directory.Exists(planFolder))
            {
                var planYamlPath = Path.Combine(planFolder, "plan.yaml");
                if (File.Exists(planYamlPath))
                {
                    var yaml = File.ReadAllText(planYamlPath);
                    var match = System.Text.RegularExpressions.Regex.Match(yaml, @"(?m)^project:\s*(.+)$");
                    if (match.Success) project = match.Groups[1].Value.Trim();
                }
            }
        }

        var job = new JobItem
        {
            Id = id,
            Type = type,
            PlanFile = planFile,
            Project = project,
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
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
        };
        psi.Environment["TENDRIL_JOB_ID"] = id;
        psi.Environment["TENDRIL_URL"] = "http://localhost:5010";

        foreach (var arg in processArgs)
            psi.ArgumentList.Add(arg);

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

        // Monitor for completion in background with timeout and stale output detection
        var cts = new CancellationTokenSource(_jobTimeout);
        job.TimeoutCts = cts;

        Task.Run(async () =>
        {
            var timedOut = false;

            try
            {
                // Wait for process exit or timeout
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                timedOut = true;
            }

            if (!timedOut && !process.HasExited)
            {
                // Shouldn't happen, but guard against it
                timedOut = true;
            }

            if (timedOut)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                CompleteJob(id, exitCode: null, timedOut: true, staleOutput: false);
                return;
            }

            CompleteJob(id, process.ExitCode, timedOut: false, staleOutput: false);
        });

        // Start stale output watchdog
        if (_staleOutputTimeout > TimeSpan.Zero)
        {
            _ = RunStaleOutputWatchdog(id, cts);
        }

        JobsChanged?.Invoke();
        return id;
    }

    private async Task RunStaleOutputWatchdog(string id, CancellationTokenSource timeoutCts)
    {
        var checkInterval = TimeSpan.FromSeconds(60);

        while (!timeoutCts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(checkInterval, timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!_jobs.TryGetValue(id, out var job) || job.Status != "Running")
                break;

            if (job.LastOutputAt.HasValue)
            {
                var sinceLastOutput = DateTime.UtcNow - job.LastOutputAt.Value;
                if (sinceLastOutput >= _staleOutputTimeout)
                {
                    // Stale output detected — cancel the timeout CTS to trigger the main monitor
                    job.StaleOutputDetected = true;
                    try { job.Process?.Kill(entireProcessTree: true); } catch { }
                    CompleteJob(id, exitCode: null, timedOut: true, staleOutput: true);
                    break;
                }
            }
        }
    }

    public void CompleteJob(string id, int? exitCode, bool timedOut = false, bool staleOutput = false)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;
        if (job.Status != "Running") return;

        if (timedOut)
        {
            job.Status = "Timeout";
            var reason = staleOutput
                ? $"No output for {(int)_staleOutputTimeout.TotalMinutes} minutes"
                : $"Exceeded {(int)_jobTimeout.TotalMinutes} minute timeout";
            job.StatusMessage = reason;
        }
        else
        {
            var success = exitCode == 0;
            job.StatusMessage = success ? null : ExtractFailureReason(job.OutputLines);
            job.Status = success ? "Completed" : "Failed";
        }

        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

        var isSuccess = job.Status == "Completed";
        var title = job.Status == "Timeout" ? "Job Timed Out" : (isSuccess ? "Job Completed" : "Job Failed");
        var message = job.PlanFile ?? job.Type;
        if (!isSuccess && job.StatusMessage != null)
            message += $": {job.StatusMessage}";
        PendingNotifications.Enqueue(new JobNotification(title, message, isSuccess));

        if (job.Status is "Failed" or "Timeout")
            ResetPlanState(job);
        else if (isSuccess && job.Type == "ExecutePlan")
            EnsurePlanStateTransitioned(job);
        else if (isSuccess && job.Type == "CreateIssue")
            SetPlanState(job, "Completed");
        else if (isSuccess && job.Type == "MakePlan")
            VerifyMakePlanResult(job);

        WriteJobLog(job);
        JobsChanged?.Invoke();

        if (!_jobs.Values.Any(j => j.Status == "Running"))
            SendNativeNotification();
    }

    public void StopJob(string id)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        job.CancellationRequested = true;
        try { job.TimeoutCts?.Cancel(); } catch { }
        try { job.Process?.Kill(entireProcessTree: true); } catch { }
        job.Status = "Stopped";
        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

        ResetPlanState(job);
        JobsChanged?.Invoke();
    }

    public void DeleteJob(string id)
    {
        _jobs.TryRemove(id, out _);
        JobsChanged?.Invoke();
    }

    public void ClearCompletedJobs()
    {
        var completedIds = _jobs.Values
            .Where(j => j.Status == "Completed")
            .Select(j => j.Id)
            .ToList();
        foreach (var id in completedIds)
            _jobs.TryRemove(id, out _);
        if (completedIds.Count > 0)
            JobsChanged?.Invoke();
    }

    public void ClearFailedJobs()
    {
        var failedIds = _jobs.Values
            .Where(j => j.Status is "Failed" or "Timeout")
            .Select(j => j.Id)
            .ToList();
        foreach (var id in failedIds)
            _jobs.TryRemove(id, out _);
        if (failedIds.Count > 0)
            JobsChanged?.Invoke();
    }

    public List<JobItem> GetJobs()
    {
        return _jobs.Values.OrderByDescending(j => j.StartedAt ?? DateTime.MinValue).ToList();
    }

    public JobItem? GetJob(string id)
    {
        return _jobs.GetValueOrDefault(id);
    }

    internal static string ExtractFailureReason(List<string> outputLines)
    {
        if (outputLines.Count == 0)
            return "Unknown error (exit code non-zero)";

        // Search from end for stderr lines
        var stderrLines = new List<string>();
        for (var i = outputLines.Count - 1; i >= 0 && stderrLines.Count < 3; i--)
        {
            var line = outputLines[i];
            if (line.StartsWith("[stderr] "))
            {
                var content = line["[stderr] ".Length..].Trim();
                if (content.Length > 0)
                    stderrLines.Insert(0, content);
            }
        }

        string reason;
        if (stderrLines.Count > 0)
        {
            reason = string.Join(" | ", stderrLines);
        }
        else
        {
            // Fall back to last non-empty output line
            reason = "";
            for (var i = outputLines.Count - 1; i >= 0; i--)
            {
                var trimmed = outputLines[i].Trim();
                if (trimmed.Length > 0)
                {
                    reason = trimmed;
                    break;
                }
            }

            if (reason.Length == 0)
                return "Unknown error (exit code non-zero)";
        }

        return reason.Length > 200 ? reason[..200] + "..." : reason;
    }

    private static string? GetNamedArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }

    private void EnsurePlanStateTransitioned(JobItem job)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            if (!File.Exists(planYamlPath)) return;

            var content = File.ReadAllText(planYamlPath);
            var stateMatch = System.Text.RegularExpressions.Regex.Match(content, @"(?m)^state:\s*(.+)$");
            if (!stateMatch.Success) return;

            var currentState = stateMatch.Groups[1].Value.Trim();
            if (currentState is "Executing" or "Building")
            {
                content = System.Text.RegularExpressions.Regex.Replace(
                    content, @"(?m)^state:\s*.*$", "state: ReadyForReview");
                content = System.Text.RegularExpressions.Regex.Replace(
                    content, @"(?m)^updated:\s*.*$", $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                File.WriteAllText(planYamlPath, content);
            }
        }
        catch { /* Don't let state transition failures crash job completion */ }
    }

    private void SetPlanState(JobItem job, string state)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            if (!File.Exists(planYamlPath)) return;

            var content = File.ReadAllText(planYamlPath);
            content = System.Text.RegularExpressions.Regex.Replace(
                content, @"(?m)^state:\s*.*$", $"state: {state}");
            content = System.Text.RegularExpressions.Regex.Replace(
                content, @"(?m)^updated:\s*.*$", $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            File.WriteAllText(planYamlPath, content);
        }
        catch { /* Don't let state transition failures crash job completion */ }
    }

    private void VerifyMakePlanResult(JobItem job)
    {
        try
        {
            if (_planReaderService == null) return;
            var plansDir = _planReaderService.PlansDirectory;
            if (!Directory.Exists(plansDir)) return;

            var outputText = string.Join("\n", job.OutputLines);
            var created = System.Text.RegularExpressions.Regex.IsMatch(outputText, @"Plan created:");
            var duplicate = System.Text.RegularExpressions.Regex.IsMatch(outputText, @"identified as duplicate:");

            if (!created && !duplicate)
            {
                // Agent exited 0 but didn't create a plan or detect a duplicate — flag it
                job.OutputLines.Add("[Tendril] WARNING: MakePlan completed but no plan folder or trash entry was found.");
                job.Status = "Failed";
                job.StatusMessage = "No plan created";
            }
        }
        catch { /* Don't let verification failures crash job completion */ }
    }

    private void ResetPlanState(JobItem job)
    {
        try
        {
            if (job.Type is "MakePlan" or "MakePr" or "CreateIssue") return;

            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            if (!File.Exists(planYamlPath)) return;

            var content = File.ReadAllText(planYamlPath);
            var newState = job.Type == "ExecutePlan" ? "Failed" : "Draft";
            content = System.Text.RegularExpressions.Regex.Replace(
                content, @"(?m)^state:\s*.*$", $"state: {newState}");
            content = System.Text.RegularExpressions.Regex.Replace(
                content, @"(?m)^updated:\s*.*$", $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            File.WriteAllText(planYamlPath, content);
        }
        catch { /* Don't let state reset failures crash job completion */ }
    }

    private void SendNativeNotification()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var completed = _jobs.Values.Count(j => j.Status == "Completed");
        var failed = _jobs.Values.Count(j => j.Status is "Failed" or "Timeout");
        var title = "Tendril \u2014 All Jobs Finished";
        var body = failed > 0
            ? $"{completed} completed, {failed} failed"
            : $"{completed} job(s) completed successfully";

        Task.Run(() =>
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-NoProfile -Command \"New-BurntToastNotification -Text '{title}', '{body}'\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch { /* Notification is best-effort */ }
        });
    }

    private void WriteJobLog(JobItem job)
    {
        if (_planReaderService == null || string.IsNullOrEmpty(job.PlanFile))
            return;

        // MakePlan jobs use the description as PlanFile (no folder exists yet) —
        // the agent writes its own logs inside the properly-named plan folder.
        if (job.Type == "MakePlan")
            return;

        try
        {
            var duration = job.DurationSeconds.HasValue ? $"{job.DurationSeconds}s" : "unknown";
            var logContent = $"# {job.Type}\n\n" +
                $"- **Status:** {job.Status}\n" +
                $"- **Started:** {job.StartedAt:u}\n" +
                $"- **Completed:** {job.CompletedAt:u}\n" +
                $"- **Duration:** {duration}\n";

            if (job.Status == "Timeout" && job.StatusMessage != null)
                logContent += $"- **Timeout Reason:** {job.StatusMessage}\n";

            _planReaderService.AddLog(job.PlanFile, job.Type, logContent);
        }
        catch
        {
            // Don't let log writing failures crash the job completion
        }
    }
}
