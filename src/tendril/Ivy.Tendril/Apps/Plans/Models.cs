using Ivy.Tendril.Helpers;

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
    Icebox,
    Blocked
}

public record PlanMetadata(
    int Id,
    string Project,
    string Level,
    string Title,
    PlanStatus State,
    List<string> Repos,
    List<string> Commits,
    List<string> Prs,
    List<PlanVerificationEntry> Verifications,
    List<string> RelatedPlans,
    List<string> DependsOn,
    DateTime Created,
    DateTime Updated,
    string? InitialPrompt,
    string? SourceUrl);

public record PlanFile(
    PlanMetadata Metadata,
    string LatestRevisionContent,
    string FolderPath,
    string PlanYamlRaw,
    int RevisionCount = 1
)
{
    public int Id => Metadata.Id;
    public string Title => Metadata.Title;
    public string Project => Metadata.Project;
    public string Level => Metadata.Level;
    public PlanStatus Status => Metadata.State;
    public List<string> Repos => Metadata.Repos;
    public List<string> Commits => Metadata.Commits;
    public List<string> Prs => Metadata.Prs;
    public List<PlanVerificationEntry> Verifications => Metadata.Verifications;
    public List<string> RelatedPlans => Metadata.RelatedPlans;
    public List<string> DependsOn => Metadata.DependsOn;
    public DateTime Created => Metadata.Created;
    public DateTime Updated => Metadata.Updated;
    public string? InitialPrompt => Metadata.InitialPrompt;
    public string? SourceUrl => Metadata.SourceUrl;
    public string FolderName => Path.GetFileName(FolderPath);
}

public class RecommendationYaml
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string State { get; set; } = "Pending";
    public string? DeclineReason { get; set; }
}

public static class PlanFilters
{
    public static IEnumerable<PlanFile> ApplyFilters(
        IEnumerable<PlanFile> plans,
        string? projectFilter,
        string? levelFilter,
        string? textFilter)
    {
        var filtered = plans;

        if (levelFilter is { } level)
            filtered = filtered.Where(p => p.Level == level);

        if (projectFilter is { } project)
            filtered = filtered.Where(p => ProjectHelper.ContainsProject(p.Project, project));

        if (!string.IsNullOrWhiteSpace(textFilter))
        {
            var search = textFilter.ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Title.ToLowerInvariant().Contains(search) ||
                p.Id.ToString().Contains(search) ||
                p.Project.ToLowerInvariant().Contains(search) ||
                p.LatestRevisionContent.ToLowerInvariant().Contains(search));
        }

        return filtered;
    }
}