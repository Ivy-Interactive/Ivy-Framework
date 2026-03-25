namespace Ivy.Tendril.Apps.PlanReviewer;

public enum PlanStatus
{
    Draft,
    Review,
    Approved,
    Skipped
}

public record PlanMetadata(int Id, string Queue, string Level, string Title);

public record PlanFile(PlanMetadata Metadata, string Content, string RawFrontmatter, string FileName, PlanStatus Status)
{
    public int Id => Metadata.Id;
    public string Title => Metadata.Title;
    public string Queue => Metadata.Queue;
    public string Level => Metadata.Level;
}
