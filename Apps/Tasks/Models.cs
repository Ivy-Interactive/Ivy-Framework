using Ivy;

namespace Ivy.Tendril.Apps.Tasks;

public record TaskItem
{
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string PlanFile { get; init; } = "";
    public string Queue { get; init; } = "";
    public string Status { get; set; } = "Pending";
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string ScriptPath { get; init; } = "";
    public string[] Args { get; init; } = [];
    public bool CancellationRequested { get; set; }

    // PTY state for persistent terminal sessions
    public IWriteStream<byte[]>? PtyStream { get; set; }
    public Action<string>? PtyHandleInput { get; set; }
    public Action<int, int>? PtyHandleResize { get; set; }
    public Action? PtyKill { get; set; }
    public bool PtyClosed { get; set; }
    public int? PtyExitCode { get; set; }
}

public record TaskItemRow
{
    public string Id { get; init; } = "";
    public string Status { get; init; } = "";
    public string Plan { get; init; } = "";
    public string Type { get; init; } = "";
    public string Queue { get; init; } = "";
    public string Timer { get; init; } = "";
}
