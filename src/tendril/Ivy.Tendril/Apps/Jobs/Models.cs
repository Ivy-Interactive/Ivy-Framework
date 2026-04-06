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
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string PlanFile { get; init; } = "";
    public string Project { get; init; } = "";
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string ScriptPath { get; init; } = "";
    public string[] Args { get; init; } = [];
    public bool CancellationRequested { get; set; }
    public string? SessionId { get; set; }
    public decimal? Cost { get; set; }

    // Process handle for non-interactive execution
    public System.Diagnostics.Process? Process { get; set; }
    public string? StatusMessage { get; set; }
    public System.Collections.Concurrent.ConcurrentQueue<string> OutputLines { get; set; } = new();
    public DateTime? LastOutputAt { get; set; }
    public CancellationTokenSource? TimeoutCts { get; set; }
    public bool StaleOutputDetected { get; set; }

    // Path to the .processing inbox file for MakePlan job recovery
    public string? InboxFile { get; set; }
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
    public string StatusMessage { get; init; } = "";
}
