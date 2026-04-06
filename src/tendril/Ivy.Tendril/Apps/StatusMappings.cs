using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Apps;

/// <summary>
/// Shared mappings for status to badge variant styling across Tendril views.
/// </summary>
internal static class StatusMappings
{
    /// <summary>
    /// Maps plan status to badge variant for consistent styling.
    /// </summary>
    public static readonly Dictionary<PlanStatus, BadgeVariant> PlanStatusBadgeVariants = new()
    {
        [PlanStatus.Building] = BadgeVariant.Info,
        [PlanStatus.Updating] = BadgeVariant.Info,
        [PlanStatus.Executing] = BadgeVariant.Info,
        [PlanStatus.ReadyForReview] = BadgeVariant.Success,
        [PlanStatus.Failed] = BadgeVariant.Destructive,
        [PlanStatus.Draft] = BadgeVariant.Outline,
        [PlanStatus.Completed] = BadgeVariant.Success,
        [PlanStatus.Skipped] = BadgeVariant.Outline,
        [PlanStatus.Icebox] = BadgeVariant.Outline
    };

    /// <summary>
    /// Maps verification status strings to badge variants for consistent styling.
    /// </summary>
    public static readonly Dictionary<string, BadgeVariant> VerificationStatusBadgeVariants = new()
    {
        ["Pass"] = BadgeVariant.Success,
        ["Fail"] = BadgeVariant.Destructive,
        ["Pending"] = BadgeVariant.Outline,
        ["Skipped"] = BadgeVariant.Outline
    };
}
