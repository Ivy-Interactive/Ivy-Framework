using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Apps.Icebox;

public class SidebarView(
    List<PlanFile> plans,
    IState<PlanFile?> selectedPlanState,
    IState<string?> queueFilter,
    IState<string?> levelFilter,
    IState<string?> textFilter) : ViewBase
{
    private readonly List<PlanFile> _plans = plans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IState<string?> _queueFilter = queueFilter;
    private readonly IState<string?> _levelFilter = levelFilter;
    private readonly IState<string?> _textFilter = textFilter;

    public object BuildHeader()
    {
        var levelOptions = new[] { "Critical", "NiceToHave", "Nitpick" };

        // Apply level filter first to get the base set for queue counting
        var levelFilteredPlans = _plans.AsEnumerable();
        if (_levelFilter.Value is { } level)
            levelFilteredPlans = levelFilteredPlans.Where(p => p.Level == level);

        // Build dynamic queue options with counts from level-filtered plans
        var queueCounts = levelFilteredPlans
            .GroupBy(p => p.Queue)
            .OrderByDescending(g => g.Count())
            .Select(g => new Option<string>($"{g.Key} ({g.Count()})", g.Key))
            .ToArray<IAnyOption>();

        return Layout.Vertical()
            | _textFilter.ToSearchInput().Placeholder("Search plans...")
            | new Expandable(
                header: "Filters",
                content: Layout.Vertical()
                    | _queueFilter.ToSelectInput(queueCounts).Placeholder("All Queues").Nullable().WithField().Label("Queue")
                    | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable().WithField().Label("Level")
            ).Open(false).Ghost()
            ;
    }

    public object BuildContent()
    {
        // Apply all filters
        var filteredPlans = PlanFilters.ApplyFilters(_plans, _queueFilter.Value, _levelFilter.Value, _textFilter.Value);

        return new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Queue).Variant(BadgeVariant.Outline).Small()
                    | new Badge(plan.Level).Variant(plan.Level == "Critical" ? BadgeVariant.Warning : BadgeVariant.Outline).Small())
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));
    }

    public override object Build()
    {
        // For backward compatibility, return content
        return BuildContent();
    }
}
