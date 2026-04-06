using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public record JobNotification(string Title, string Message, bool IsSuccess);

public class JobService : IJobService
{
    private readonly ConcurrentDictionary<string, JobItem> _jobs = new();
    private readonly Channel<string> _jobQueue = Channel.CreateUnbounded<string>();
    private int _counter;
    private readonly IPlanReaderService? _planReaderService;
    private readonly IConfigService? _configService;
    private readonly ModelPricingService? _modelPricingService;
    private readonly ITelemetryService? _telemetryService;
    private readonly TimeSpan _jobTimeout;
    private readonly TimeSpan _staleOutputTimeout;
    private readonly int _maxConcurrentJobs;
    private readonly SemaphoreSlim _jobSlotSemaphore;

    private readonly string? _inboxPath;
    private readonly SynchronizationContext? _syncContext;

    public event Action? JobsChanged;
    public event Action<JobNotification>? NotificationReady;

    private static readonly string PromptsRoot =
        Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".promptwares"));

    internal static readonly string SharedRoot = Path.Combine(PromptsRoot, ".shared");

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

    public JobService(
        IConfigService configService,
        ModelPricingService? modelPricingService = null,
        IPlanReaderService? planReaderService = null,
        ITelemetryService? telemetryService = null)
    {
        _syncContext = SynchronizationContext.Current;
        _configService = configService;
        _modelPricingService = modelPricingService;
        _planReaderService = planReaderService;
        _telemetryService = telemetryService;
        _jobTimeout = TimeSpan.FromMinutes(configService.Settings.JobTimeout);
        _staleOutputTimeout = TimeSpan.FromMinutes(configService.Settings.StaleOutputTimeout);
        _maxConcurrentJobs = configService.Settings.MaxConcurrentJobs;
        _jobSlotSemaphore = _maxConcurrentJobs > 0
            ? new SemaphoreSlim(_maxConcurrentJobs, _maxConcurrentJobs)
            : new SemaphoreSlim(0, 1);
        _inboxPath = Path.Combine(configService.TendrilHome, "Inbox");
    }

    public JobService(
        TimeSpan jobTimeout,
        TimeSpan staleOutputTimeout,
        string? inboxPath = null,
        int maxConcurrentJobs = 5,
        IPlanReaderService? planReaderService = null,
        ITelemetryService? telemetryService = null)
    {
        _syncContext = SynchronizationContext.Current;
        _jobTimeout = jobTimeout;
        _staleOutputTimeout = staleOutputTimeout;
        _maxConcurrentJobs = maxConcurrentJobs;
        _jobSlotSemaphore = maxConcurrentJobs > 0
            ? new SemaphoreSlim(maxConcurrentJobs, maxConcurrentJobs)
            : new SemaphoreSlim(0, 1);
        _inboxPath = inboxPath;
        _planReaderService = planReaderService;
        _telemetryService = telemetryService;
    }

    private void RaiseJobsChanged()
    {
        if (_syncContext != null)
        {
            _syncContext.Post(_ => JobsChanged?.Invoke(), null);
        }
        else
        {
            JobsChanged?.Invoke();
        }
    }

    private void RaiseNotification(JobNotification notification)
    {
        if (_syncContext != null)
        {
            _syncContext.Post(_ => NotificationReady?.Invoke(notification), null);
        }
        else
        {
            NotificationReady?.Invoke(notification);
        }
    }

    public string StartJob(string type, string[] args, string? inboxFilePath)
    {
        return StartJobInternal(type, args, inboxFilePath);
    }

    public string StartJob(string type, params string[] args)
    {
        return StartJobInternal(type, args, inboxFilePath: null);
    }

    private string StartJobInternal(string type, string[] args, string? inboxFilePath)
    {
        var id = $"job-{Interlocked.Increment(ref _counter):D3}";
        var scriptPath = ScriptPaths.GetValueOrDefault(type, "");

        // Extract plan folder and project from args
        var planFile = "";
        var project = "[Auto]";

        // For MakePlan: args are named params like -Description "..." -Project "..."
        // For others: args[0] is the plan folder path
        if (type == "MakePlan")
        {
            planFile = GetNamedArg(args, "-Description") is { } desc
                ? (desc.Length > 80 ? desc[..80] + "..." : desc)
                : "New Plan";
            project = GetNamedArg(args, "-Project") ?? "[Auto]";
        }
        else
        {
            var planFolder = args.Length > 0 ? args[0] : "";
            planFile = Path.GetFileName(planFolder);
            if (Directory.Exists(planFolder))
            {
                var plan = ReadPlanYaml(planFolder);
                if (plan != null) project = plan.Project;
            }
        }

        var job = new JobItem
        {
            Id = id,
            Type = type,
            PlanFile = planFile,
            Project = project,
            Status = JobStatus.Pending,
            ScriptPath = scriptPath,
            Args = args,
        };

        // For MakePlan jobs: track the inbox file for crash recovery
        if (type == "MakePlan")
        {
            if (inboxFilePath != null)
            {
                // Inbox-originated job — file already renamed to .processing by InboxWatcherService
                job.InboxFile = inboxFilePath;
            }
            else if (_inboxPath != null)
            {
                // Manual MakePlan — write a .processing inbox file as a write-ahead log
                try
                {
                    Directory.CreateDirectory(_inboxPath);
                    var description = GetNamedArg(args, "-Description") ?? "New Plan";
                    var inboxProject = GetNamedArg(args, "-Project") ?? "[Auto]";
                    var pendingFile = Path.Combine(_inboxPath, $"pending-{id}.md.processing");
                    var content = $"---\nproject: {inboxProject}\n---\n{description}";
                    File.WriteAllText(pendingFile, content);
                    job.InboxFile = pendingFile;
                }
                catch { /* Best-effort — don't fail the job if we can't write the recovery file */ }
            }
        }

        _jobs[id] = job;

        // Check dependencies for ExecutePlan jobs
        if (type == "ExecutePlan")
        {
            var planFolder = args.Length > 0 ? args[0] : "";
            var (ok, blockReason) = CheckDependencies(planFolder);
            if (!ok)
            {
                job.Status = JobStatus.Blocked;
                job.StatusMessage = blockReason;
                job.CompletedAt = DateTime.UtcNow;

                // Reset plan state back to Draft since we can't execute
                ResetPlanStateToBlocked(job);

                var blockedNotification = new JobNotification("Job Blocked", $"{planFile}: {blockReason}", false);
                RaiseNotification(blockedNotification);
                RaiseJobsChanged();
                return id;
            }
        }

        // Try to acquire a job slot
        if (!_jobSlotSemaphore.Wait(0))
        {
            job.Status = JobStatus.Queued;
            job.StatusMessage = $"Waiting (max {_maxConcurrentJobs} concurrent jobs)";
            _jobQueue.Writer.TryWrite(id);
            RaiseJobsChanged();
            return id;
        }

        LaunchJob(job);
        return id;
    }

    /// <summary>
    /// Creates a job in "Running" state without launching a real process.
    /// Used by tests to exercise CompleteJob without background monitor races.
    /// </summary>
    internal string CreateTestJob(string type, params string[] args)
    {
        var id = $"job-{Interlocked.Increment(ref _counter):D3}";
        var job = new JobItem
        {
            Id = id,
            Type = type,
            PlanFile = args.Length > 0 ? args[0] : type,
            Status = JobStatus.Running,
            StartedAt = DateTime.UtcNow,
            ScriptPath = "",
            Args = args,
            TimeoutCts = new CancellationTokenSource(),
        };
        _jobs[id] = job;
        _jobSlotSemaphore.Wait(0); // Acquire slot so CompleteJob can release it
        return id;
    }

    private void LaunchJob(JobItem job)
    {
        var id = job.Id;
        var type = job.Type;
        var args = job.Args;
        var scriptPath = job.ScriptPath;

        job.Status = JobStatus.Running;
        job.StartedAt = DateTime.UtcNow;
        job.StatusMessage = null;

        // Run before-hooks
        var planFolderForHooks = type != "MakePlan" && args.Length > 0 ? args[0] : "";
        RunHooks("before", type, planFolderForHooks, job.Project, job);

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
        // Generate session ID for cost tracking — passed to child process so both sides share the same ID
        job.SessionId = Guid.NewGuid().ToString();

        psi.Environment["TENDRIL_JOB_ID"] = id;
        psi.Environment["TENDRIL_URL"] = Environment.GetEnvironmentVariable("TENDRIL_URL") ?? "http://localhost:5010";
        psi.Environment["TENDRIL_SHARED"] = SharedRoot;
        psi.Environment["TENDRIL_SESSION_ID"] = job.SessionId;
        if (_configService != null)
            psi.Environment["TENDRIL_CONFIG"] = _configService.ConfigPath;

        foreach (var arg in processArgs)
            psi.ArgumentList.Add(arg);

        var process = new System.Diagnostics.Process { StartInfo = psi };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                job.LastOutputAt = DateTime.UtcNow;
                if (!e.Data.Contains("\"type\":\"heartbeat\""))
                {
                    job.OutputLines.Enqueue(e.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                job.OutputLines.Enqueue($"[stderr] {e.Data}");
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
                try { process.Kill(entireProcessTree: true); } catch { /* Process may have already exited */ }
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

        RaiseJobsChanged();
    }

    internal void RunHooks(string when, string jobType, string planFolder, string project, JobItem job)
    {
        if (_configService == null) return;

        var projectConfig = _configService.GetProject(project);
        if (projectConfig == null) return;

        var hooks = projectConfig.Hooks
            .Where(h => h.When.Equals(when, StringComparison.OrdinalIgnoreCase))
            .Where(h => h.Promptwares.Count == 0 || h.Promptwares.Contains(jobType, StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var hook in hooks)
        {
            try
            {
                // Evaluate condition if set
                if (!string.IsNullOrWhiteSpace(hook.Condition))
                {
                    var condPsi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"-NoProfile -EncodedCommand {EncodeForPowerShell(hook.Condition)}",
                        WorkingDirectory = string.IsNullOrEmpty(planFolder) ? "." : planFolder,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var condProc = System.Diagnostics.Process.Start(condPsi);
                    var condOutput = condProc?.StandardOutput.ReadToEnd().Trim() ?? "";
                    condProc?.WaitForExit(10000);

                    if (condProc?.ExitCode != 0 ||
                        condOutput.Equals("False", StringComparison.OrdinalIgnoreCase))
                    {
                        job.OutputLines.Enqueue($"[hook:{hook.Name}] Condition not met, skipping");
                        continue;
                    }
                }

                // Run the action
                var actionPsi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-NoProfile -EncodedCommand {EncodeForPowerShell(hook.Action)}",
                    WorkingDirectory = string.IsNullOrEmpty(planFolder) ? "." : planFolder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                actionPsi.Environment["TENDRIL_JOB_ID"] = job.Id;
                actionPsi.Environment["TENDRIL_JOB_TYPE"] = jobType;
                actionPsi.Environment["TENDRIL_JOB_STATUS"] = job.Status.ToString();
                actionPsi.Environment["TENDRIL_PLAN_FOLDER"] = planFolder;
                actionPsi.Environment["TENDRIL_CONFIG"] = _configService.ConfigPath;

                using var actionProc = System.Diagnostics.Process.Start(actionPsi);
                var output = actionProc?.StandardOutput.ReadToEnd().Trim() ?? "";
                var stderr = actionProc?.StandardError.ReadToEnd().Trim() ?? "";
                actionProc?.WaitForExit(30000);

                if (!string.IsNullOrEmpty(output))
                    job.OutputLines.Enqueue($"[hook:{hook.Name}] {output}");
                if (!string.IsNullOrEmpty(stderr))
                    job.OutputLines.Enqueue($"[hook:{hook.Name}] [stderr] {stderr}");

                if (actionProc?.ExitCode != 0)
                    job.OutputLines.Enqueue($"[hook:{hook.Name}] Hook failed with exit code {actionProc?.ExitCode}");
            }
            catch (Exception ex)
            {
                job.OutputLines.Enqueue($"[hook:{hook.Name}] Error: {ex.Message}");
            }
        }
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

            if (!_jobs.TryGetValue(id, out var job) || job.Status != JobStatus.Running)
                break;

            if (job.LastOutputAt.HasValue)
            {
                var sinceLastOutput = DateTime.UtcNow - job.LastOutputAt.Value;
                if (sinceLastOutput >= _staleOutputTimeout)
                {
                    // Stale output detected — cancel the timeout CTS to trigger the main monitor
                    job.StaleOutputDetected = true;
                    try { job.Process?.Kill(entireProcessTree: true); } catch { /* Process may have already exited */ }
                    CompleteJob(id, exitCode: null, timedOut: true, staleOutput: true);
                    break;
                }
            }
        }
    }

    public void CompleteJob(string id, int? exitCode, bool timedOut = false, bool staleOutput = false)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;
        if (job.Status != JobStatus.Running) return;

        if (timedOut)
        {
            job.Status = JobStatus.Timeout;
            var reason = staleOutput
                ? $"No output for {(int)_staleOutputTimeout.TotalMinutes} minutes"
                : $"Exceeded {(int)_jobTimeout.TotalMinutes} minute timeout";
            job.StatusMessage = reason;
        }
        else
        {
            var success = exitCode == 0;
            job.StatusMessage = success ? null : ExtractFailureReason(job.OutputLines.ToList());
            job.Status = success ? JobStatus.Completed : JobStatus.Failed;
        }

        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

        // Release job slot for next queued job
        _jobSlotSemaphore.Release();

        // Run after-hooks
        var planFolderForHooks = job.Args.Length > 0 ? job.Args[0] : "";
        RunHooks("after", job.Type, planFolderForHooks, job.Project, job);

        var isSuccess = job.Status == JobStatus.Completed;
        var title = job.Status == JobStatus.Timeout ? $"{job.Type} Timed Out" : (isSuccess ? $"{job.Type} Completed" : $"{job.Type} Failed");
        var message = job.PlanFile ?? job.Type;
        if (!isSuccess && job.StatusMessage != null)
            message += $": {job.StatusMessage}";
        var completionNotification = new JobNotification(title, message, isSuccess);
        RaiseNotification(completionNotification);

        if (job.Status is JobStatus.Failed or JobStatus.Timeout)
            ResetPlanState(job);
        else if (isSuccess && job.Type == "ExecutePlan")
            EnsurePlanStateTransitioned(job);
        else if (isSuccess && job.Type == "CreateIssue")
            SetPlanState(job, "Completed");
        else if (isSuccess && job.Type == "MakePlan")
        {
            VerifyMakePlanResult(job);
            if (job.Status == JobStatus.Completed)
            {
                var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
                var level = "NiceToHave";
                if (Directory.Exists(planFolder))
                {
                    var plan = ReadPlanYaml(planFolder);
                    if (plan != null) level = plan.Level;
                }

                _telemetryService?.TrackPlanCreated(new PlanCreatedContext(
                    Project: job.Project,
                    Level: level,
                    DurationSeconds: job.DurationSeconds));
            }
        }

        if (isSuccess && job.Type == "MakePr")
        {
            _telemetryService?.TrackPrCreated(new PrCreatedContext(
                Project: job.Project,
                DurationSeconds: job.DurationSeconds));
        }

        _telemetryService?.TrackJobCompleted(job.Type, job.Status, job.DurationSeconds);

        // Flush telemetry events to ensure they reach PostHog
        if (_telemetryService != null)
            _ = Task.Run(async () => await _telemetryService.FlushAsync());

        CleanupInboxFile(job);
        WriteJobLog(job);

        // Calculate and log costs automatically (delayed to allow session to complete)
        if (isSuccess && _modelPricingService != null && !string.IsNullOrEmpty(job.SessionId))
        {
            var sessionId = job.SessionId;
            var jobArgs = job.Args;
            var jobType = job.Type;
            var jobId = job.Id;

            _ = Task.Run(async () =>
            {
                // Wait for Claude session to write final usage data
                await Task.Delay(TimeSpan.FromSeconds(30));

                try
                {
                    var costCalc = _modelPricingService.CalculateSessionCost(sessionId);
                    if (costCalc.TotalCost > 0)
                    {
                        if (_jobs.TryGetValue(jobId, out var j))
                        {
                            j.Cost = (decimal)costCalc.TotalCost;
                            RaiseJobsChanged();
                        }

                        if (jobArgs.Length > 0)
                        {
                            LogCostToCsv(jobArgs[0], jobType, costCalc.TotalTokens, costCalc.TotalCost);
                        }
                    }
                }
                catch { /* Best-effort cost tracking */ }
            });
        }

        RaiseJobsChanged();

        // Try to start queued jobs now that a slot is free
        ProcessJobQueue();

        if (!_jobs.Values.Any(j => j.Status == JobStatus.Running))
            SendNativeNotification();
    }

    public void StopJob(string id)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;

        var wasRunning = job.Status == JobStatus.Running;
        job.CancellationRequested = true;
        try { job.TimeoutCts?.Cancel(); } catch { /* CTS may already be disposed */ }
        try { job.Process?.Kill(entireProcessTree: true); } catch { /* Process may have already exited */ }
        job.Status = JobStatus.Stopped;
        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

        // Release job slot if the job was running
        if (wasRunning)
            _jobSlotSemaphore.Release();

        CleanupInboxFile(job);
        ResetPlanState(job);
        RaiseJobsChanged();

        // Try to start queued jobs now that a slot is free
        if (wasRunning)
            ProcessJobQueue();
    }

    public void DeleteJob(string id)
    {
        _jobs.TryRemove(id, out _);
        RaiseJobsChanged();
    }

    public void ClearCompletedJobs()
    {
        var completedIds = _jobs.Values
            .Where(j => j.Status == JobStatus.Completed)
            .Select(j => j.Id)
            .ToList();
        foreach (var id in completedIds)
            _jobs.TryRemove(id, out _);
        if (completedIds.Count > 0)
            RaiseJobsChanged();
    }

    public void ClearFailedJobs()
    {
        var failedIds = _jobs.Values
            .Where(j => j.Status is JobStatus.Failed or JobStatus.Timeout or JobStatus.Blocked)
            .Select(j => j.Id)
            .ToList();
        foreach (var id in failedIds)
            _jobs.TryRemove(id, out _);
        if (failedIds.Count > 0)
            RaiseJobsChanged();
    }

    public List<JobItem> GetJobs()
    {
        return _jobs.Values.OrderByDescending(j => j.StartedAt ?? DateTime.MinValue).ToList();
    }

    public JobItem? GetJob(string id)
    {
        return _jobs.GetValueOrDefault(id);
    }

    private void ProcessJobQueue()
    {
        while (_jobQueue.Reader.TryPeek(out var queuedId))
        {
            // Try to acquire a job slot (non-blocking)
            if (!_jobSlotSemaphore.Wait(0))
                break;

            if (!_jobQueue.Reader.TryRead(out queuedId))
            {
                // Failed to dequeue — release the slot we just acquired
                _jobSlotSemaphore.Release();
                break;
            }

            if (!_jobs.TryGetValue(queuedId, out var queuedJob) || queuedJob.Status != JobStatus.Queued)
            {
                // Job was removed or state changed — release the slot
                _jobSlotSemaphore.Release();
                continue;
            }

            LaunchJob(queuedJob);
        }
    }

    private static string EncodeForPowerShell(string command)
    {
        var bytes = System.Text.Encoding.Unicode.GetBytes(command);
        return Convert.ToBase64String(bytes);
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

    internal static string? ReadPlanYamlRaw(string planFolder)
    {
        var planYamlPath = Path.Combine(planFolder, "plan.yaml");
        return File.Exists(planYamlPath) ? FileHelper.ReadAllText(planYamlPath) : null;
    }

    internal static PlanYaml? ReadPlanYaml(string planFolder)
    {
        var yaml = ReadPlanYamlRaw(planFolder);
        if (yaml == null) return null;

        try
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            return deserializer.Deserialize<PlanYaml>(yaml);
        }
        catch
        {
            return null;
        }
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
            var plan = ReadPlanYaml(planFolder);
            if (plan == null) return;

            if (plan.State is "Executing" or "Building")
            {
                var hasIncomplete = plan.Verifications?.Any(v => v.Status is "Pending" or "Fail") ?? false;
                var targetState = hasIncomplete ? "Failed" : "ReadyForReview";

                var content = ReadPlanYamlRaw(planFolder);
                if (content == null) return;
                content = Regex.Replace(content, @"(?m)^state:\s*.*$", $"state: {targetState}");
                content = Regex.Replace(content, @"(?m)^updated:\s*.*$", $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                var planYamlPath = Path.Combine(planFolder, "plan.yaml");
                FileHelper.WriteAllText(planYamlPath, content);
            }
        }
        catch { /* Don't let state transition failures crash job completion */ }
    }

    private void SetPlanState(JobItem job, string state)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var content = ReadPlanYamlRaw(planFolder);
            if (content == null) return;

            content = Regex.Replace(content, @"(?m)^state:\s*.*$", $"state: {state}");
            content = Regex.Replace(content, @"(?m)^updated:\s*.*$", $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            FileHelper.WriteAllText(planYamlPath, content);
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
                job.OutputLines.Enqueue("[Tendril] WARNING: MakePlan completed but no plan folder or trash entry was found.");
                job.Status = JobStatus.Failed;
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
            var content = ReadPlanYamlRaw(planFolder);
            if (content == null) return;

            var newState = job.Type == "ExecutePlan" ? "Failed" : "Draft";
            content = Regex.Replace(content, @"(?m)^state:\s*.*$", $"state: {newState}");
            content = Regex.Replace(content, @"(?m)^updated:\s*.*$", $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            FileHelper.WriteAllText(planYamlPath, content);
        }
        catch { /* Don't let state reset failures crash job completion */ }
    }

    private (bool Ok, string? BlockReason) CheckDependencies(string planFolder)
    {
        try
        {
            var planYaml = ReadPlanYaml(planFolder);
            if (planYaml?.DependsOn == null || planYaml.DependsOn.Count == 0)
                return (true, null);

            var plansDir = _planReaderService?.PlansDirectory;
            if (plansDir == null) return (true, null);

            foreach (var dep in planYaml.DependsOn)
            {
                var depFolder = Path.Combine(plansDir, dep);
                var depPlan = ReadPlanYaml(depFolder);

                if (depPlan == null)
                    return (false, $"Dependency '{dep}' not found");

                if (!depPlan.State.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    return (false, $"Dependency '{dep}' is '{depPlan.State}', not Completed");

                // Check that all PRs are actually merged
                if (depPlan.Prs.Count == 0)
                    continue;

                foreach (var prUrl in depPlan.Prs)
                {
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "gh",
                            Arguments = $"pr view \"{prUrl}\" --json state -q .state",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };
                        var proc = System.Diagnostics.Process.Start(psi);
                        var output = proc?.StandardOutput.ReadToEnd().Trim() ?? "";
                        proc?.WaitForExit(10000);

                        if (!output.Equals("MERGED", StringComparison.OrdinalIgnoreCase))
                            return (false, $"Dependency '{dep}' PR {prUrl} is '{output}', not MERGED");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Failed to check PR status for '{dep}': {ex.Message}");
                    }
                }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Dependency check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether the given inbox file path is already tracked by a running MakePlan job.
    /// Used by InboxWatcherService to avoid re-processing files.
    /// </summary>
    public bool IsInboxFileTracked(string filePath)
    {
        return _jobs.Values.Any(j =>
            j.Type == "MakePlan" &&
            j.Status == JobStatus.Running &&
            j.InboxFile != null &&
            j.InboxFile.Equals(filePath, StringComparison.OrdinalIgnoreCase));
    }

    private void CleanupInboxFile(JobItem job)
    {
        if (string.IsNullOrEmpty(job.InboxFile)) return;
        try
        {
            if (File.Exists(job.InboxFile))
                File.Delete(job.InboxFile);
        }
        catch { /* Best-effort cleanup */ }
    }

    private void ResetPlanStateToBlocked(JobItem job)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var content = ReadPlanYamlRaw(planFolder);
            if (content == null) return;

            content = Regex.Replace(content, @"(?m)^state:\s*.*$", "state: Draft");
            content = Regex.Replace(content, @"(?m)^updated:\s*.*$",
                $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            FileHelper.WriteAllText(planYamlPath, content);
        }
        catch { /* Don't let state reset failures crash job completion */ }
    }

    private void SendNativeNotification()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var completed = _jobs.Values.Count(j => j.Status == JobStatus.Completed);
        var failed = _jobs.Values.Count(j => j.Status is JobStatus.Failed or JobStatus.Timeout);
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

    internal static void LogCostToCsv(string planFolder, string jobType, int tokens, double cost)
    {
        if (!Directory.Exists(planFolder)) return;

        var csvPath = Path.Combine(planFolder, "costs.csv");
        if (!File.Exists(csvPath))
        {
            FileHelper.WriteAllText(csvPath, "Promptware,Tokens,Cost\n");
        }

        var line = $"{jobType},{tokens},{cost:F4}\n";
        FileHelper.AppendAllText(csvPath, line);
    }

    internal void WriteJobLog(JobItem job)
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

            if (!string.IsNullOrEmpty(job.SessionId))
                logContent += $"- **SessionId:** {job.SessionId}\n";

            if (job.Status == JobStatus.Timeout && job.StatusMessage != null)
                logContent += $"- **Timeout Reason:** {job.StatusMessage}\n";

            _planReaderService.AddLog(job.PlanFile, job.Type, logContent);

            // Persist raw output for failed/timeout jobs
            if (job.Status is JobStatus.Failed or JobStatus.Timeout && job.OutputLines.Count > 0)
            {
                var planFolder = job.Args.Length > 0 ? job.Args[0] : null;
                if (planFolder != null && Directory.Exists(planFolder))
                {
                    var logsDir = Path.Combine(planFolder, "logs");
                    Directory.CreateDirectory(logsDir);
                    var outputFile = Path.Combine(logsDir, $"{job.Type}-{job.Id}.output.log");
                    File.WriteAllLines(outputFile, job.OutputLines);
                }
            }
        }
        catch
        {
            // Don't let log writing failures crash the job completion
        }
    }
}
