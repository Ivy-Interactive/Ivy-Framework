using System.Collections.Concurrent;
using System.Diagnostics;

namespace Ivy.Tendril.Apps.Jobs;

public enum JobStatus
{
    Pending,
    Queued,
    Running,
    Completed,
    Failed,
    Timeout,
    Stopped,
    Blocked
}

public record JobItem
{
    private const int MaxOutputLines = 10_000;
    private int _completionGuard;

    public bool TryClaimCompletion() =>
        Interlocked.CompareExchange(ref _completionGuard, 1, 0) == 0;

    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string PlanFile { get; set; } = "";
    public string Project { get; init; } = "";
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string ScriptPath { get; init; } = "";
    public string[] Args { get; init; } = [];
    public bool CancellationRequested { get; set; }
    public string? SessionId { get; set; }
    public string Provider { get; init; } = "claude";
    public decimal? Cost { get; set; }
    public int? Tokens { get; set; }

    // Process handle for non-interactive execution
    public Process? Process { get; set; }
    public string? StatusMessage { get; set; }
    public ConcurrentQueue<string> OutputLines { get; set; } = new();
    public DateTime? LastOutputAt { get; set; }
    public CancellationTokenSource? TimeoutCts { get; set; }
    private volatile bool _staleOutputDetected;
    public bool StaleOutputDetected
    {
        get => _staleOutputDetected;
        set => _staleOutputDetected = value;
    }

    // Path to the .processing inbox file for MakePlan job recovery
    public string? InboxFile { get; set; }

    public void EnqueueOutput(string line)
    {
        OutputLines.Enqueue(line);
        while (OutputLines.Count > MaxOutputLines)
            OutputLines.TryDequeue(out _);
    }

    public void DisposeResources()
    {
        try { Process?.Dispose(); } catch { }
        try { TimeoutCts?.Dispose(); } catch { }
        Process = null;
        TimeoutCts = null;
    }

    /// <summary>
    ///     Trims the output buffer to the most recent lines to free memory after completion.
    ///     Called after all completion logic (failure reason extraction, log writing, hooks)
    ///     has finished. Retains a small tail for UI inspection of recent hook/status output.
    /// </summary>
    public void TrimOutput()
    {
        const int keepLines = 50;
        if (OutputLines.Count <= keepLines) return;
        var trimmed = new ConcurrentQueue<string>(OutputLines.Skip(OutputLines.Count - keepLines));
        OutputLines = trimmed;
    }
}

public record JobItemRow
{
    public string Id { get; init; } = "";
    public JobStatus Status { get; init; }
    public string PlanId { get; init; } = "";
    public string Plan { get; init; } = "";
    public string Type { get; init; } = "";
    public string Project { get; init; } = "";
    public string Timer { get; init; } = "";
    public string LastOutput { get; init; } = "";
    public DateTime? LastOutputTimestamp { get; init; }
    public string Cost { get; init; } = "";
    public string Tokens { get; init; } = "";
    public string StatusMessage { get; init; } = "";
}