namespace Ivy.Tendril.Apps.Plans;

public enum PlanStatus
{
    Draft,
    Building,
    Updating,
    Executing,
    Completed,
    Failed,
    ReadyForReview,
    Skipped,
    Icebox
}

public record PlanMetadata(int Id, string Project, string Level, string Title, PlanStatus State, List<string> Commits, List<string> Prs, List<PlanVerificationEntry> Verifications, List<string> RelatedPlans);

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
    public List<string> Commits => Metadata.Commits;
    public List<string> Prs => Metadata.Prs;
    public List<PlanVerificationEntry> Verifications => Metadata.Verifications;
    public List<string> RelatedPlans => Metadata.RelatedPlans;
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
