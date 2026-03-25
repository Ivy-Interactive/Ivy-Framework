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
