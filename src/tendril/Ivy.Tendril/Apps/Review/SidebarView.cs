using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Review;

public class SidebarView(
    List<PlanFile> plans,
    IState<PlanFile?> selectedPlanState,
    IState<string?> textFilter,
    IConfigService config) : ViewBase
{
    private readonly List<PlanFile> _plans = plans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IState<string?> _textFilter = textFilter;
    private readonly IConfigService _config = config;

    public object BuildHeader()
    {
        return Layout.Vertical()
            | _textFilter.ToSearchInput().Placeholder("Search plans...")
            ;
    }

    public object BuildContent()
    {
        var filteredPlans = PlanFilters.ApplyFilters(_plans, null, null, _textFilter.Value);

        return new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            var verificationsPassed = plan.Verifications.Count > 0
                    && plan.Verifications.All(v => v.Status is "Pass" or "Skipped");

            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Project).Variant(BadgeVariant.Outline).Small().WithProjectColor(_config, plan.Project)
                    | (verificationsPassed
                        ? new Badge("Verified").Variant(BadgeVariant.Success).Small()
                        : new Badge("Unverified").Variant(BadgeVariant.Warning).Small())
                )
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));
    }

    public override object Build()
    {
        return BuildContent();
    }
}
