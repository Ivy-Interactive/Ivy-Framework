namespace Ivy.Tendril.Apps.Tasks;

public enum TaskStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public record TaskItem
{
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string PlanFile { get; init; } = "";
    public string Queue { get; init; } = "";
    public string Status { get; init; } = "Pending";
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? DurationSeconds { get; init; }
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
