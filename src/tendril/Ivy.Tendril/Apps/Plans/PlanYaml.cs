namespace Ivy.Tendril.Apps.Plans;

public class PlanVerificationEntry
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Pending";
}

public class PlanYaml
{
    public string State { get; set; } = "Draft";
    public string Project { get; set; } = "[Auto]";
    public string Level { get; set; } = "NiceToHave";
    public string Title { get; set; } = "";
    public List<string> Repos { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public string InitialPrompt { get; set; } = "";
    public List<string> Prs { get; set; } = new();
    public List<string> Commits { get; set; } = new();
    public List<PlanVerificationEntry> Verifications { get; set; } = new();
    public List<string> RelatedPlans { get; set; } = new();
    public List<string> DependsOn { get; set; } = new();
}
