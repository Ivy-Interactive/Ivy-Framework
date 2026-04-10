namespace Ivy.Tendril.Apps.PullRequest;

public record PrRow
{
    public string Id { get; init; } = "";
    public string PlanId { get; init; } = "";
    public string Repository { get; init; } = "";
    public string Pr { get; init; } = "";
    public string Plan { get; init; } = "";
    public string Cost { get; init; } = "";
    public string Status { get; init; } = "";
    public string Tokens { get; init; } = "";
    public string PlanFolderPath { get; init; } = "";
}