namespace Ivy.Tendril.Apps.Plans;

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

public static class PlanFilters
{
    public static IEnumerable<PlanFile> ApplyFilters(
        IEnumerable<PlanFile> plans,
        string? queueFilter,
        string? levelFilter,
        string? textFilter)
    {
        var filtered = plans;

        // Apply level filter
        if (levelFilter is { } level)
            filtered = filtered.Where(p => p.Level == level);

        // Apply queue filter
        if (queueFilter is { } queue)
            filtered = filtered.Where(p => p.Queue == queue);

        // Apply text filter
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
