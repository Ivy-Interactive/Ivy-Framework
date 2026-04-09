using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Ivy.Helpers;
using Ivy.Tendril.Apps;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Services;

public record JobNotification(string Title, string Message, bool IsSuccess);

public class JobService : IJobService
{
    private static readonly string PromptsRoot = ResolvePromptsRoot();

    internal static readonly string SharedRoot = Path.Combine(PromptsRoot, ".shared");

    internal static string ResolvePromptsRoot()
    {
        // 1. Debug/source mode: check if Promptwares exists relative to BaseDirectory
        var sourceRoot = Path.GetFullPath(
            Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "Promptwares"));
        if (Directory.Exists(sourceRoot))
            return sourceRoot;

        // 2. Production mode: use TENDRIL_HOME/Promptwares
        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        if (!string.IsNullOrEmpty(tendrilHome))
        {
            var deployedRoot = Path.Combine(tendrilHome, "Promptwares");
            if (Directory.Exists(deployedRoot))
                return deployedRoot;
        }

        // 3. Fallback (will fail at runtime, but gives a clear error location)
        return sourceRoot;
    }

    private static readonly Dictionary<string, string> ScriptPaths = new()
    {
        ["MakePlan"] = Path.Combine(PromptsRoot, "MakePlan", "MakePlan.ps1"),
        ["UpdatePlan"] = Path.Combine(PromptsRoot, "UpdatePlan", "UpdatePlan.ps1"),
        ["SplitPlan"] = Path.Combine(PromptsRoot, "SplitPlan", "SplitPlan.ps1"),
        ["ExpandPlan"] = Path.Combine(PromptsRoot, "ExpandPlan", "ExpandPlan.ps1"),
        ["ExecutePlan"] = Path.Combine(PromptsRoot, "ExecutePlan", "ExecutePlan.ps1"),
        ["IvyFrameworkVerification"] = Path.Combine(PromptsRoot, "IvyFrameworkVerification", "IvyFrameworkVerification.ps1"),
        ["MakePr"] = Path.Combine(PromptsRoot, "MakePr", "MakePr.ps1"),
        ["CreateIssue"] = Path.Combine(PromptsRoot, "CreateIssue", "CreateIssue.ps1")
    };

    private readonly IConfigService? _configService;
    private readonly IPlanDatabaseService? _database;

    private readonly string? _inboxPath;
    private readonly Channel<string> _jobQueue = Channel.CreateUnbounded<string>();
    private readonly SemaphoreSlim _jobSlotSemaphore;
    private readonly TimeSpan _jobTimeout;
    private readonly ConcurrentDictionary<string, JobItem> _jobs = new();
    private readonly int _maxConcurrentJobs;
    private readonly ModelPricingService? _modelPricingService;
    private readonly IPlanReaderService? _planReaderService;
    private readonly IPlanWatcherService? _planWatcherService;
    private readonly TimeSpan _staleOutputTimeout;
    private readonly SynchronizationContext? _syncContext;
    private readonly ITelemetryService? _telemetryService;
    private int _counter;

    public JobService(
        IConfigService configService,
        ModelPricingService? modelPricingService = null,
        IPlanReaderService? planReaderService = null,
        ITelemetryService? telemetryService = null,
        IPlanWatcherService? planWatcherService = null,
        IPlanDatabaseService? database = null)
    {
        _syncContext = SynchronizationContext.Current;
        _configService = configService;
        _modelPricingService = modelPricingService;
        _planReaderService = planReaderService;
        _telemetryService = telemetryService;
        _planWatcherService = planWatcherService;
        _database = database;
        _jobTimeout = TimeSpan.FromMinutes(configService.Settings.JobTimeout);
        _staleOutputTimeout = TimeSpan.FromMinutes(configService.Settings.StaleOutputTimeout);
        _maxConcurrentJobs = configService.Settings.MaxConcurrentJobs;
        _jobSlotSemaphore = _maxConcurrentJobs > 0
            ? new SemaphoreSlim(_maxConcurrentJobs, _maxConcurrentJobs)
            : new SemaphoreSlim(0, 1);
        _inboxPath = Path.Combine(configService.TendrilHome, "Inbox");
        LoadHistoricalJobs();
    }

    public JobService(
        TimeSpan jobTimeout,
        TimeSpan staleOutputTimeout,
        string? inboxPath = null,
        int maxConcurrentJobs = 5,
        IPlanReaderService? planReaderService = null,
        ITelemetryService? telemetryService = null,
        IPlanDatabaseService? database = null)
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
        _database = database;
    }

    public event Action? JobsChanged;
    public event Action<JobNotification>? NotificationReady;

    public string StartJob(string type, string[] args, string? inboxFilePath)
    {
        return StartJobInternal(type, args, inboxFilePath);
    }

    public string StartJob(string type, params string[] args)
    {
        return StartJobInternal(type, args, null);
    }

    public void CompleteJob(string id, int? exitCode, bool timedOut = false, bool staleOutput = false)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;
        if (!job.TryClaimCompletion()) return;

        if (timedOut)
        {
            job.Status = JobStatus.Timeout;
            var reason = (staleOutput || job.StaleOutputDetected)
                ? $"No output for {(int)_staleOutputTimeout.TotalMinutes} minutes"
                : $"Exceeded {(int)_jobTimeout.TotalMinutes} minute timeout";
            job.StatusMessage = reason;
        }
        else
        {
            var success = exitCode == 0;
            if (!success)
                job.StatusMessage ??= ExtractFailureReason(job.OutputLines.ToList());
            else
                job.StatusMessage = null;
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
        var title = job.Status == JobStatus.Timeout ? $"{job.Type} Timed Out" :
            isSuccess ? $"{job.Type} Completed" : $"{job.Type} Failed";
        var message = job.PlanFile ?? job.Type;
        if (!isSuccess && job.StatusMessage != null)
            message += $": {job.StatusMessage}";
        var completionNotification = new JobNotification(title, message, isSuccess);
        RaiseNotification(completionNotification);

        if (job.Status is JobStatus.Failed or JobStatus.Timeout)
        {
            ResetPlanState(job);
            ScheduleWorktreeCleanup(job);
        }
        else if (isSuccess && job.Type == "ExecutePlan")
        {
            EnsurePlanStateTransitioned(job);
        }
        else if (isSuccess && job.Type == "CreateIssue")
        {
            SetPlanState(job, "Completed");
            RetryBlockedDependents(job.Args.Length > 0 ? job.Args[0] : "");
        }
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
                    level,
                    job.DurationSeconds));
            }
        }

        if (isSuccess && job.Type == "MakePr")
        {
            _telemetryService?.TrackPrCreated(new PrCreatedContext(
                job.DurationSeconds));
            RetryBlockedDependents(job.Args.Length > 0 ? job.Args[0] : "");
        }

        _telemetryService?.TrackJobCompleted(job.Type, job.Status, job.DurationSeconds);

        // Flush telemetry events to ensure they reach PostHog
        if (_telemetryService != null)
            _ = Task.Run(async () => { try { await _telemetryService.FlushAsync(); } catch { /* best-effort */ } });

        CleanupInboxFile(job);
        WriteJobLog(job);

        // Notify plan watcher so the database sync picks up any new/changed plans
        var notifyFolder = job.Args.Length > 0 ? job.Args[0] : null;
        _planWatcherService?.NotifyChanged(Directory.Exists(notifyFolder) ? notifyFolder : null);

        // Calculate and log costs automatically (delayed to allow session to complete)
        if (isSuccess && _modelPricingService != null && !string.IsNullOrEmpty(job.SessionId))
        {
            var sessionId = job.SessionId;
            var jobArgs = job.Args;
            var jobType = job.Type;
            var jobId = job.Id;
            var provider = job.Provider;

            _ = Task.Run(async () =>
            {
                // Wait for Claude session to write final usage data
                await Task.Delay(TimeSpan.FromSeconds(30));

                try
                {
                    var costCalc = _modelPricingService.CalculateSessionCost(sessionId, provider);
                    if (costCalc.TotalCost > 0)
                    {
                        if (_jobs.TryGetValue(jobId, out var j))
                        {
                            j.Cost = (decimal)costCalc.TotalCost;
                            j.Tokens = costCalc.TotalTokens;
                            PersistJob(j);
                            RaiseJobsChanged();
                        }

                        if (jobArgs.Length > 0)
                            LogCostToCsv(jobArgs[0], jobType, costCalc.TotalTokens, costCalc.TotalCost);
                    }
                }
                catch
                {
                    /* Best-effort cost tracking */
                }
            });
        }

        // Release process and timeout resources
        job.DisposeResources();

        // Persist completed job to SQLite
        PersistJob(job);

        // Free output buffer — all consumers (failure reason, hooks, log writing) are done.
        // Output for failed jobs was already written to logs/ above.
        job.TrimOutput();

        // Evict stale finished jobs from memory to prevent unbounded dictionary growth.
        // Job metadata is already persisted to SQLite; the in-memory copy is only needed
        // for active display and is reloaded from DB on next startup.
        EvictStaleJobs();

        RaiseJobsChanged();

        // After successful completion of jobs that may unblock dependencies
        if (isSuccess && job.Type is "ExecutePlan" or "MakePr") RetryBlockedJobs();

        // Try to start queued jobs now that a slot is free
        ProcessJobQueue();

        if (!_jobs.Values.Any(j => j.Status == JobStatus.Running))
            SendNativeNotification();
    }

    public void StopJob(string id)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;
        if (!job.TryClaimCompletion()) return;

        var wasRunning = job.Status == JobStatus.Running;
        job.CancellationRequested = true;
        try
        {
            job.TimeoutCts?.Cancel();
        }
        catch
        {
            /* CTS may already be disposed */
        }

        try
        {
            job.Process?.Kill(true);
        }
        catch
        {
            /* Process may have already exited */
        }

        job.DisposeResources();

        job.Status = JobStatus.Stopped;
        job.CompletedAt = DateTime.UtcNow;
        if (job.StartedAt.HasValue)
            job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

        // Release job slot if the job was running
        if (wasRunning)
            _jobSlotSemaphore.Release();

        CleanupInboxFile(job);
        ResetPlanState(job);
        job.TrimOutput();
        RaiseJobsChanged();

        // Try to start queued jobs now that a slot is free
        if (wasRunning)
            ProcessJobQueue();
    }

    public void DeleteJob(string id)
    {
        if (_jobs.TryRemove(id, out var removed))
        {
            removed.DisposeResources();
            try { _database?.DeleteJob(id); } catch { /* Best-effort */ }
        }
        RaiseJobsChanged();
    }

    /// <summary>
    ///     Removes finished jobs older than 1 hour from the in-memory dictionary.
    ///     Job metadata remains in SQLite and is reloaded on next startup via LoadHistoricalJobs.
    ///     Keeps the most recent 20 finished jobs regardless of age so the UI stays useful.
    /// </summary>
    private void EvictStaleJobs()
    {
        const int keepRecent = 20;
        var cutoff = DateTime.UtcNow.AddHours(-1);

        var staleJobs = _jobs.Values
            .Where(j => j.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Timeout or JobStatus.Stopped)
            .Where(j => j.CompletedAt.HasValue && j.CompletedAt.Value < cutoff)
            .OrderByDescending(j => j.CompletedAt)
            .Skip(keepRecent)
            .Select(j => j.Id)
            .ToList();

        foreach (var id in staleJobs)
            if (_jobs.TryRemove(id, out var removed))
                removed.DisposeResources();
    }

    public void ClearCompletedJobs()
    {
        var completedIds = _jobs.Values
            .Where(j => j.Status == JobStatus.Completed)
            .Select(j => j.Id)
            .ToList();
        foreach (var id in completedIds)
        {
            if (_jobs.TryRemove(id, out var removed))
                removed.DisposeResources();
            try { _database?.DeleteJob(id); } catch { /* Best-effort */ }
        }
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
        {
            if (_jobs.TryRemove(id, out var removed))
                removed.DisposeResources();
            try { _database?.DeleteJob(id); } catch { /* Best-effort */ }
        }
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

    /// <summary>
    ///     Checks whether the given inbox file path is already tracked by a running MakePlan job.
    ///     Used by InboxWatcherService to avoid re-processing files.
    /// </summary>
    public bool IsInboxFileTracked(string filePath)
    {
        return _jobs.Values.Any(j =>
            j.Type == "MakePlan" &&
            j.Status == JobStatus.Running &&
            j.InboxFile != null &&
            j.InboxFile.Equals(filePath, StringComparison.OrdinalIgnoreCase));
    }

    private void LoadHistoricalJobs()
    {
        if (_database == null) return;
        try
        {
            var historicalJobs = _database.GetRecentJobs();
            foreach (var job in historicalJobs) _jobs.TryAdd(job.Id, job);
        }
        catch
        {
            /* Best-effort — don't block startup */
        }
    }

    private void PersistJob(JobItem job)
    {
        try
        {
            _database?.UpsertJob(job);
        }
        catch
        {
            /* Best-effort persistence */
        }
    }

    private void RaiseJobsChanged()
    {
        if (_syncContext != null)
            _syncContext.Post(_ => JobsChanged?.Invoke(), null);
        else
            JobsChanged?.Invoke();
    }

    private void RaiseNotification(JobNotification notification)
    {
        if (_syncContext != null)
            _syncContext.Post(_ => NotificationReady?.Invoke(notification), null);
        else
            NotificationReady?.Invoke(notification);
    }

    private string StartJobSkipDepCheck(string type, params string[] args)
    {
        return StartJobInternal(type, args, inboxFilePath: null, skipDependencyCheck: true);
    }

    private string StartJobInternal(string type, string[] args, string? inboxFilePath, bool skipDependencyCheck = false)
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
            planFile = GetNamedArg(args, "-Description") is { Length: > 0 } desc
                ? desc.Length > 50 ? desc[..50] + "..." : desc
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
            Provider = _configService?.Settings.CodingAgent ?? "claude"
        };

        // For MakePlan jobs: track the inbox file for crash recovery
        if (type == "MakePlan")
        {
            if (inboxFilePath != null)
                // Inbox-originated job — file already renamed to .processing by InboxWatcherService
                job.InboxFile = inboxFilePath;
            else if (_inboxPath != null)
                // Manual MakePlan — write a .processing inbox file as a write-ahead log
                try
                {
                    Directory.CreateDirectory(_inboxPath);
                    var description = GetNamedArg(args, "-Description") ?? "New Plan";
                    var inboxProject = GetNamedArg(args, "-Project") ?? "[Auto]";
                    var pendingFile = Path.Combine(_inboxPath, $"pending-{id}.md.processing");
                    var content = $"---\nproject: {inboxProject}\n---\n{description}";
                    FileHelper.WriteAllText(pendingFile, content);
                    job.InboxFile = pendingFile;
                }
                catch
                {
                    /* Best-effort — don't fail the job if we can't write the recovery file */
                }
        }

        _jobs[id] = job;

        // Check dependencies for ExecutePlan jobs
        if (type == "ExecutePlan" && !skipDependencyCheck)
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
    ///     Creates a job in "Running" state without launching a real process.
    ///     Used by tests to exercise CompleteJob without background monitor races.
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
            TimeoutCts = new CancellationTokenSource()
        };
        _jobs[id] = job;
        _jobSlotSemaphore.Wait(0); // Acquire slot so CompleteJob can release it
        return id;
    }

    /// <summary>
    ///     Removes a job from the dictionary. Used by tests to simulate concurrent removal.
    /// </summary>
    internal bool RemoveJob(string id) => _jobs.TryRemove(id, out _);

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
        var processArgs = new List<string> { "-NoProfile", "-NonInteractive", "-File", scriptPath };
        processArgs.AddRange(args);

        var workingDirectory = Path.GetFullPath(
            Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

        var psi = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
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

        var process = new Process { StartInfo = psi };
        process.OutputDataReceived += (_, e) =>
        {
            try
            {
                if (e.Data != null)
                {
                    job.LastOutputAt = DateTime.UtcNow;
                    if (!e.Data.Contains("\"type\":\"heartbeat\"")) job.EnqueueOutput(e.Data);
                }
            }
            catch (Exception ex)
            {
                Program.WriteCrashLog($"[{DateTime.UtcNow:O}] OutputDataReceived exception for job {id}: {ex}");
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            try
            {
                if (e.Data != null)
                {
                    job.EnqueueOutput($"[stderr] {e.Data}");
                    job.LastOutputAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Program.WriteCrashLog($"[{DateTime.UtcNow:O}] ErrorDataReceived exception for job {id}: {ex}");
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
            try
            {
                if (await process.WaitForExitOrKillAsync(cts.Token))
                {
                    if (_jobs.TryGetValue(id, out var j) && j.StaleOutputDetected)
                        CompleteJob(id, null, timedOut: true, staleOutput: true);
                    else
                        CompleteJob(id, process.ExitCode);
                }
                else
                {
                    CompleteJob(id, null, timedOut: true);
                }
            }
            catch (Exception ex)
            {
                // CompleteJob failures are non-recoverable — the job will appear stuck
                // but the process survives.
                Program.WriteCrashLog($"[{DateTime.UtcNow:O}] JobService process monitor exception for job {id}: {ex}");
            }
        });

        // Start stale output watchdog
        if (_staleOutputTimeout > TimeSpan.Zero) _ = RunStaleOutputWatchdog(id, cts);

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
            try
            {
                // Evaluate condition if set
                if (!string.IsNullOrWhiteSpace(hook.Condition))
                {
                    var condPsi = new ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"-NoProfile -NonInteractive -EncodedCommand {EncodeForPowerShell(hook.Condition)}",
                        WorkingDirectory = string.IsNullOrEmpty(planFolder) ? "." : planFolder,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var condProc = Process.Start(condPsi);
                    var condOutput = condProc?.StandardOutput.ReadToEnd().Trim() ?? "";
                    condProc.WaitForExitOrKill(10000);

                    if (condProc?.ExitCode != 0 ||
                        condOutput.Equals("False", StringComparison.OrdinalIgnoreCase))
                    {
                        job.EnqueueOutput($"[hook:{hook.Name}] Condition not met, skipping");
                        continue;
                    }
                }

                // Run the action
                var actionPsi = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-NoProfile -NonInteractive -EncodedCommand {EncodeForPowerShell(hook.Action)}",
                    WorkingDirectory = string.IsNullOrEmpty(planFolder) ? "." : planFolder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                actionPsi.Environment["TENDRIL_JOB_ID"] = job.Id;
                actionPsi.Environment["TENDRIL_JOB_TYPE"] = jobType;
                actionPsi.Environment["TENDRIL_JOB_STATUS"] = job.Status.ToString();
                actionPsi.Environment["TENDRIL_PLAN_FOLDER"] = planFolder;
                actionPsi.Environment["TENDRIL_CONFIG"] = _configService.ConfigPath;

                using var actionProc = Process.Start(actionPsi);
                var output = actionProc?.StandardOutput.ReadToEnd().Trim() ?? "";
                var stderr = actionProc?.StandardError.ReadToEnd().Trim() ?? "";
                actionProc.WaitForExitOrKill(30000);

                if (!string.IsNullOrEmpty(output))
                    job.EnqueueOutput($"[hook:{hook.Name}] {output}");
                if (!string.IsNullOrEmpty(stderr))
                    job.EnqueueOutput($"[hook:{hook.Name}] [stderr] {stderr}");

                if (actionProc?.ExitCode != 0)
                    job.EnqueueOutput($"[hook:{hook.Name}] Hook failed with exit code {actionProc?.ExitCode}");
            }
            catch (Exception ex)
            {
                job.EnqueueOutput($"[hook:{hook.Name}] Error: {ex.Message}");
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
                    // Stale output detected — signal via flag and cancel the timeout CTS
                    // The main monitor (LaunchJob background task) will handle process kill and CompleteJob
                    job.StaleOutputDetected = true;
                    try { timeoutCts.Cancel(); } catch (ObjectDisposedException) { }
                    break;
                }
            }
        }
    }

    private void RetryBlockedJobs()
    {
        var blockedJobs = _jobs.Values
            .Where(j => j.Status == JobStatus.Blocked && j.Type == "ExecutePlan")
            .ToList();

        foreach (var blockedJob in blockedJobs)
        {
            var planFolder = blockedJob.Args.Length > 0 ? blockedJob.Args[0] : "";
            if (string.IsNullOrEmpty(planFolder)) continue;

            var (ok, _) = CheckDependencies(planFolder);
            if (ok)
            {
                // Remove the blocked job entry — only one thread succeeds
                if (!_jobs.TryRemove(blockedJob.Id, out _))
                    continue; // Another thread already handled this job

                // Skip if there's already an active job for this plan
                if (HasActiveJobForPlan(planFolder))
                    continue;

                // Transition plan to Building before re-launching (ExecutePlan.ps1 requires it)
                SetPlanStateByFolder(planFolder, "Building");

                // Re-start the job — skip dependency check since we just verified
                StartJobSkipDepCheck(blockedJob.Type, blockedJob.Args);

                var notification = new JobNotification(
                    "Job Unblocked",
                    $"{blockedJob.PlanFile}: dependencies now satisfied, auto-restarting",
                    true);
                RaiseNotification(notification);
            }
        }
    }

    private bool HasActiveJobForPlan(string planFolder)
    {
        return _jobs.Values.Any(j =>
            j.Type == "ExecutePlan" &&
            j.Status is JobStatus.Running or JobStatus.Queued or JobStatus.Pending &&
            j.Args.Length > 0 &&
            j.Args[0].Equals(planFolder, StringComparison.OrdinalIgnoreCase));
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
        var bytes = Encoding.Unicode.GetBytes(command);
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

        reason = SanitizeForDisplay(reason);

        return reason.Length > 200 ? reason[..200] + "..." : reason;
    }

    internal static string SanitizeForDisplay(string text)
    {
        // Strip ANSI escape codes (color, cursor, formatting)
        text = Regex.Replace(text, @"\x1B\[[0-9;]*[A-Za-z]", "");

        // Replace control characters (tabs, newlines, carriage returns, null bytes, etc.) with spaces
        text = Regex.Replace(text, @"[\x00-\x1F]", " ");

        // Collapse multiple consecutive spaces into one
        text = Regex.Replace(text, @" {2,}", " ");

        return text.Trim();
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
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            return deserializer.Deserialize<PlanYaml>(yaml);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Updates one or more fields in plan.yaml using Regex replacement.
    ///     Preserves YAML formatting and field order.
    /// </summary>
    internal static void UpdatePlanYamlFields(string planFolder, params (string field, string value)[] updates)
    {
        var content = ReadPlanYamlRaw(planFolder);
        if (content == null) return;

        foreach (var (field, value) in updates)
        {
            var pattern = $@"(?m)^{Regex.Escape(field)}:\s*.*$";
            var replacement = $"{field}: {value}";
            content = Regex.Replace(content, pattern, replacement);
        }

        var planYamlPath = Path.Combine(planFolder, "plan.yaml");
        FileHelper.WriteAllText(planYamlPath, content);
    }

    /// <summary>
    ///     Updates the state and updated timestamp in plan.yaml.
    /// </summary>
    internal static void SetPlanStateByFolder(string planFolder, string state)
    {
        UpdatePlanYamlFields(planFolder,
            ("state", state),
            ("updated", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")));
    }

    private static string? GetNamedArg(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    private void EnsurePlanStateTransitioned(JobItem job)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var planYaml = ReadPlanYaml(planFolder);
            if (planYaml == null) return;

            if (planYaml.State is "Executing" or "Building")
            {
                var hasIncomplete = planYaml.Verifications?
                    .Any(v => v.Status is "Pending" or "Fail") ?? false;
                var targetState = hasIncomplete ? "Failed" : "ReadyForReview";

                SetPlanStateByFolder(planFolder, targetState);
            }
        }
        catch
        {
            /* Don't let state transition failures crash job completion */
        }
    }

    private void SetPlanState(JobItem job, string state)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            SetPlanStateByFolder(planFolder, state);
        }
        catch
        {
            /* Don't let state transition failures crash job completion */
        }
    }

    private void VerifyMakePlanResult(JobItem job)
    {
        try
        {
            if (_planReaderService == null) return;
            var plansDir = _planReaderService.PlansDirectory;
            if (!Directory.Exists(plansDir)) return;

            var outputText = string.Join("\n", job.OutputLines);
            var createdMatch = Regex.Match(outputText, @"Plan created:\s*(\S+)");
            var duplicate = Regex.IsMatch(outputText, @"identified as duplicate:");

            if (createdMatch.Success)
            {
                job.PlanFile = createdMatch.Groups[1].Value;
            }
            else if (!duplicate)
            {
                // Agent exited 0 but didn't create a plan or detect a duplicate — flag it
                job.EnqueueOutput(
                    "[Tendril] WARNING: MakePlan completed but no plan folder or trash entry was found.");
                job.Status = JobStatus.Failed;
                job.StatusMessage = "No plan created";
            }
        }
        catch
        {
            /* Don't let verification failures crash job completion */
        }
    }

    private void ResetPlanState(JobItem job)
    {
        try
        {
            if (job.Type is "MakePlan" or "MakePr" or "CreateIssue") return;

            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            var newState = job.Type == "ExecutePlan" ? "Failed" : "Draft";
            SetPlanStateByFolder(planFolder, newState);
        }
        catch
        {
            /* Don't let state reset failures crash job completion */
        }
    }

    private static void ScheduleWorktreeCleanup(JobItem job)
    {
        if (job.Type != "ExecutePlan") return;

        var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
        if (string.IsNullOrEmpty(planFolder) || !Directory.Exists(planFolder)) return;

        var worktreesDir = Path.Combine(planFolder, "worktrees");
        if (!Directory.Exists(worktreesDir)) return;

        // Clean up immediately after failure — no grace period needed since the plan just failed
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            try
            {
                PlanReaderService.RemoveWorktrees(planFolder);

                if (Directory.Exists(worktreesDir) && Directory.GetDirectories(worktreesDir).Length == 0)
                    Directory.Delete(worktreesDir, false);
            }
            catch
            {
                // Best-effort: background cleanup service will catch it later
            }
        });
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

                foreach (var prUrl in depPlan.Prs.Where(PullRequestApp.IsValidUrl))
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "gh",
                            Arguments = $"pr view \"{prUrl}\" --json state -q .state",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using var proc = Process.Start(psi);
                        var output = proc?.StandardOutput.ReadToEnd().Trim() ?? "";
                        proc.WaitForExitOrKill(10000);

                        if (!output.Equals("MERGED", StringComparison.OrdinalIgnoreCase))
                            return (false, $"Dependency '{dep}' PR {prUrl} is '{output}', not MERGED");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Failed to check PR status for '{dep}': {ex.Message}");
                    }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Dependency check failed: {ex.Message}");
        }
    }

    private void RetryBlockedDependents(string completedPlanFolder)
    {
        try
        {
            var completedFolderName = Path.GetFileName(completedPlanFolder);
            var plansDir = _planReaderService?.PlansDirectory;
            if (string.IsNullOrEmpty(plansDir) || !Directory.Exists(plansDir)) return;

            foreach (var dir in Directory.GetDirectories(plansDir))
            {
                var planYaml = ReadPlanYaml(dir);
                if (planYaml == null) continue;
                if (!planYaml.State.Equals("Blocked", StringComparison.OrdinalIgnoreCase)) continue;
                if (!planYaml.DependsOn.Contains(completedFolderName, StringComparer.OrdinalIgnoreCase)) continue;

                // Skip if there's already a blocked or active job for this plan
                var hasExistingJob = _jobs.Values.Any(j =>
                    j.Type == "ExecutePlan" &&
                    j.Status is JobStatus.Blocked or JobStatus.Running or JobStatus.Queued or JobStatus.Pending &&
                    j.Args.Length > 0 &&
                    j.Args[0].Equals(dir, StringComparison.OrdinalIgnoreCase));
                if (hasExistingJob) continue;

                var (allMet, _) = CheckDependencies(dir);
                if (allMet)
                {
                    SetPlanStateByFolder(dir, "Building");
                    StartJobSkipDepCheck("ExecutePlan", dir);
                }
            }
        }
        catch
        {
            /* Don't let auto-retry failures crash job completion */
        }
    }

    private void CleanupInboxFile(JobItem job)
    {
        if (string.IsNullOrEmpty(job.InboxFile)) return;
        try
        {
            if (File.Exists(job.InboxFile))
                File.Delete(job.InboxFile);
        }
        catch
        {
            /* Best-effort cleanup */
        }
    }

    private void ResetPlanStateToBlocked(JobItem job)
    {
        try
        {
            var planFolder = job.Args.Length > 0 ? job.Args[0] : "";
            SetPlanStateByFolder(planFolder, "Blocked");
        }
        catch
        {
            /* Don't let state reset failures crash job completion */
        }
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
                var psi = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-NoProfile -NonInteractive -Command \"New-BurntToastNotification -Text '{title}', '{body}'\"",
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
            }
            catch
            {
                /* Notification is best-effort */
            }
        });
    }

    internal static void LogCostToCsv(string planFolder, string jobType, int tokens, double cost)
    {
        if (!Directory.Exists(planFolder)) return;

        var csvPath = Path.Combine(planFolder, "costs.csv");
        if (!File.Exists(csvPath)) FileHelper.WriteAllText(csvPath, "Promptware,Tokens,Cost\n");

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