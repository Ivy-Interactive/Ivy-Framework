using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Ivy;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public record JobNotification(string Title, string Message, bool IsSuccess);

public class JobService
{
    private readonly ConcurrentDictionary<string, JobItem> _jobs = new();
    private int _counter;
    private PlanReaderService? _planReaderService;
    private readonly ConfigService? _configService;
    private readonly ModelPricingService? _modelPricingService;
    private TelemetryService? _telemetryService;
    private readonly TimeSpan _jobTimeout;
    private readonly TimeSpan _staleOutputTimeout;

    private readonly string? _inboxPath;

    public event Action? JobsChanged;
    public ConcurrentQueue<JobNotification> PendingNotifications { get; } = new();

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

    public JobService(ConfigService configService, ModelPricingService? modelPricingService = null)
    {
        _configService = configService;
        _modelPricingService = modelPricingService;
        _jobTimeout = TimeSpan.FromMinutes(configService.Settings.JobTimeout);
        _staleOutputTimeout = TimeSpan.FromMinutes(configService.Settings.StaleOutputTimeout);
        _inboxPath = Path.Combine(configService.TendrilHome, "Inbox");
    }

    public JobService(TimeSpan jobTimeout, TimeSpan staleOutputTimeout, string? inboxPath = null)
    {
        _jobTimeout = jobTimeout;
        _staleOutputTimeout = staleOutputTimeout;
        _inboxPath = inboxPath;
    }

    public void SetPlanReaderService(PlanReaderService planReaderService)
    {
        _planReaderService = planReaderService;
    }

    public void SetTelemetryService(TelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
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
                job.Status = "Blocked";
                job.StatusMessage = blockReason;
                job.CompletedAt = DateTime.UtcNow;
                if (job.StartedAt.HasValue)
                    job.DurationSeconds = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;

                // Reset plan state back to Draft since we can't execute
                ResetPlanStateToBlocked(job);

                PendingNotifications.Enqueue(new JobNotification("Job Blocked", $"{planFile}: {blockReason}", false));
                JobsChanged?.Invoke();
                return id;
            }
        }

        // Run before-hooks
        var planFolderForHooks = type != "MakePlan" && args.Length > 0 ? args[0] : "";
        RunHooks("before", type, planFolderForHooks, project, job);

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
        // Capture session ID from environment for cost tracking after job completes
        job.SessionId = Environment.GetEnvironmentVariable("CLAUDE_SESSION_ID");

        psi.Environment["TENDRIL_JOB_ID"] = id;
        psi.Environment["TENDRIL_URL"] = "http://localhost:5010";
        psi.Environment["TENDRIL_SHARED"] = SharedRoot;

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
                    job.OutputLines.Add(e.Data);
                }
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
                        Arguments = $"-NoProfile -Command \"{hook.Condition}\"",
                        WorkingDirectory = string.IsNullOrEmpty(planFolder) ? "." : planFolder,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    var condProc = System.Diagnostics.Process.Start(condPsi);
                    var condOutput = condProc?.StandardOutput.ReadToEnd().Trim() ?? "";
                    condProc?.WaitForExit(10000);

                    if (condProc?.ExitCode != 0 ||
                        condOutput.Equals("False", StringComparison.OrdinalIgnoreCase))
                    {
                        job.OutputLines.Add($"[hook:{hook.Name}] Condition not met, skipping");
                        continue;
                    }
                }

                // Run the action
                var actionPsi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-NoProfile -Command \"{hook.Action}\"",
                    WorkingDirectory = string.IsNullOrEmpty(planFolder) ? "." : planFolder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                actionPsi.Environment["TENDRIL_JOB_ID"] = job.Id;
                actionPsi.Environment["TENDRIL_JOB_TYPE"] = jobType;
                actionPsi.Environment["TENDRIL_JOB_STATUS"] = job.Status;
                actionPsi.Environment["TENDRIL_PLAN_FOLDER"] = planFolder;

                var actionProc = System.Diagnostics.Process.Start(actionPsi);
                var output = actionProc?.StandardOutput.ReadToEnd().Trim() ?? "";
                var stderr = actionProc?.StandardError.ReadToEnd().Trim() ?? "";
                actionProc?.WaitForExit(30000);

                if (!string.IsNullOrEmpty(output))
                    job.OutputLines.Add($"[hook:{hook.Name}] {output}");
                if (!string.IsNullOrEmpty(stderr))
                    job.OutputLines.Add($"[hook:{hook.Name}] [stderr] {stderr}");

                if (actionProc?.ExitCode != 0)
                    job.OutputLines.Add($"[hook:{hook.Name}] Hook failed with exit code {actionProc?.ExitCode}");
            }
            catch (Exception ex)
            {
                job.OutputLines.Add($"[hook:{hook.Name}] Error: {ex.Message}");
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

        // Run after-hooks
        var planFolderForHooks = job.Args.Length > 0 ? job.Args[0] : "";
        RunHooks("after", job.Type, planFolderForHooks, job.Project, job);

        var isSuccess = job.Status == "Completed";
        var title = job.Status == "Timeout" ? $"{job.Type} Timed Out" : (isSuccess ? $"{job.Type} Completed" : $"{job.Type} Failed");
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
        {
            VerifyMakePlanResult(job);
            if (job.Status == "Completed")
                _telemetryService?.TrackPlanCreated();
        }

        if (isSuccess && job.Type == "MakePr")
            _telemetryService?.TrackPrCreated();

        _telemetryService?.TrackJobCompleted(job.Type, job.Status, job.DurationSeconds);

        CleanupInboxFile(job);
        WriteJobLog(job);

        // Calculate and log costs automatically (delayed to allow session to complete)
        if (isSuccess && _modelPricingService != null && !string.IsNullOrEmpty(job.SessionId))
        {
            var sessionId = job.SessionId;
            var jobArgs = job.Args;
            var jobType = job.Type;

            _ = Task.Run(async () =>
            {
                // Wait for Claude session to write final usage data
                await Task.Delay(TimeSpan.FromSeconds(30));

                try
                {
                    var costCalc = _modelPricingService.CalculateSessionCost(sessionId);
                    if (costCalc.TotalCost > 0 && jobArgs.Length > 0)
                    {
                        LogCostToCsv(jobArgs[0], jobType, costCalc.TotalTokens, costCalc.TotalCost);
                    }
                }
                catch { /* Best-effort cost tracking */ }
            });
        }

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

        CleanupInboxFile(job);
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
            .Where(j => j.Status is "Failed" or "Timeout" or "Blocked")
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
                // Check verification statuses before deciding target state
                var verificationStatuses = System.Text.RegularExpressions.Regex.Matches(content, @"(?m)^\s+status:\s*(.+)$")
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Groups[1].Value.Trim())
                    .ToList();

                var hasIncomplete = verificationStatuses.Any(s => s is "Pending" or "Fail");
                var targetState = hasIncomplete ? "Failed" : "ReadyForReview";

                content = System.Text.RegularExpressions.Regex.Replace(
                    content, @"(?m)^state:\s*.*$", $"state: {targetState}");
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

    private (bool Ok, string? BlockReason) CheckDependencies(string planFolder)
    {
        try
        {
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            if (!File.Exists(planYamlPath)) return (true, null);

            var yaml = File.ReadAllText(planYamlPath);
            var planYaml = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build()
                .Deserialize<PlanYaml>(yaml);

            if (planYaml?.DependsOn == null || planYaml.DependsOn.Count == 0)
                return (true, null);

            var plansDir = _planReaderService?.PlansDirectory;
            if (plansDir == null) return (true, null);

            foreach (var dep in planYaml.DependsOn)
            {
                var depFolder = Path.Combine(plansDir, dep);
                var depYamlPath = Path.Combine(depFolder, "plan.yaml");

                if (!File.Exists(depYamlPath))
                    return (false, $"Dependency '{dep}' not found");

                var depYaml = File.ReadAllText(depYamlPath);
                var stateMatch = Regex.Match(depYaml, @"(?m)^state:\s*(.+)$");
                if (!stateMatch.Success)
                    return (false, $"Dependency '{dep}' has no state");

                var state = stateMatch.Groups[1].Value.Trim();
                if (!state.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    return (false, $"Dependency '{dep}' is '{state}', not Completed");

                // Check that all PRs are actually merged
                var prMatches = Regex.Matches(depYaml, @"(?m)^- (https://github\.com/.+/pull/\d+)$");
                if (prMatches.Count == 0)
                {
                    // No PRs — completed without PR (e.g. CreateIssue), that's ok
                    continue;
                }

                foreach (System.Text.RegularExpressions.Match prMatch in prMatches)
                {
                    var prUrl = prMatch.Groups[1].Value.Trim();
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
            j.Status == "Running" &&
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
            var planYamlPath = Path.Combine(planFolder, "plan.yaml");
            if (!File.Exists(planYamlPath)) return;

            var content = File.ReadAllText(planYamlPath);
            content = Regex.Replace(content, @"(?m)^state:\s*.*$", "state: Draft");
            content = Regex.Replace(content, @"(?m)^updated:\s*.*$",
                $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            File.WriteAllText(planYamlPath, content);
        }
        catch { }
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

    internal static void LogCostToCsv(string planFolder, string jobType, int tokens, double cost)
    {
        if (!Directory.Exists(planFolder)) return;

        var csvPath = Path.Combine(planFolder, "costs.csv");
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath, "Promptware,Tokens,Cost\n");
        }

        var line = $"{jobType},{tokens},{cost:F4}\n";
        File.AppendAllText(csvPath, line);
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

            // Persist raw output for failed/timeout jobs
            if (job.Status is "Failed" or "Timeout" && job.OutputLines.Count > 0)
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
