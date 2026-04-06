using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public static class PlanFileExtensions
{
    /// <summary>
    /// Gets the effective repository paths for this plan.
    /// Returns the plan's explicit repos if set, otherwise falls back to the project's default repos from config.
    /// </summary>
    public static List<string> GetEffectiveRepoPaths(this PlanFile plan, IConfigService config)
    {
        if ((plan.Repos?.Count ?? 0) > 0)
            return plan.Repos;

        return config.GetProject(plan.Project)?.RepoPaths ?? [];
    }
}
