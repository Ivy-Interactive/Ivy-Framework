namespace Ivy.Tendril.Apps.Plans;

public enum PlanStatus
{
    Draft,
    Building,
    Updating,
    ReadyForReview,
    Skipped,
    Icebox
}

public record PlanMetadata(int Id, string Project, string Level, string Title, PlanStatus State);

public record PlanFile(
    PlanMetadata Metadata,
    string LatestRevisionContent,
    string FolderPath,
    string PlanYamlRaw
)
{
    public int Id => Metadata.Id;
    public string Title => Metadata.Title;
    public string Queue => Metadata.Project;
    public string Level => Metadata.Level;
    public PlanStatus Status => Metadata.State;
    public string FolderName => Path.GetFileName(FolderPath);
}

public static class PlanFilters
{
    public static IEnumerable<PlanFile> ApplyFilters(
        IEnumerable<PlanFile> plans,
        string? queueFilter,
        string? levelFilter,
        string? textFilter)
    {
        var filtered = plans;

        if (levelFilter is { } level)
            filtered = filtered.Where(p => p.Level == level);

        if (queueFilter is { } queue)
            filtered = filtered.Where(p => p.Queue == queue);

        if (!string.IsNullOrWhiteSpace(textFilter))
        {
            var search = textFilter.ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Title.ToLowerInvariant().Contains(search) ||
                p.Id.ToString().Contains(search) ||
                p.Queue.ToLowerInvariant().Contains(search));
        }

        return filtered;
    }
}
