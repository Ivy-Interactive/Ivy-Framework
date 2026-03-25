using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Apps.Icebox;

public class IceboxSidebarView : ViewBase
{
    private readonly List<PlanFile> _plans;
    private readonly IState<PlanFile?> _selectedPlanState;
    private readonly IState<string?> _queueFilter;
    private readonly IState<string?> _levelFilter;
    private readonly IState<string?> _textFilter;

    public IceboxSidebarView(
        List<PlanFile> plans,
        IState<PlanFile?> selectedPlanState,
        IState<string?> queueFilter,
        IState<string?> levelFilter,
        IState<string?> textFilter)
    {
        _plans = plans;
        _selectedPlanState = selectedPlanState;
        _queueFilter = queueFilter;
        _levelFilter = levelFilter;
        _textFilter = textFilter;
    }

    public override object Build()
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

        // Apply queue filter for the final list
        var filteredPlans = levelFilteredPlans;
        if (_queueFilter.Value is { } queue)
            filteredPlans = filteredPlans.Where(p => p.Queue == queue);

        // Apply text filter
        if (!string.IsNullOrWhiteSpace(_textFilter.Value))
        {
            var search = _textFilter.Value.ToLowerInvariant();
            filteredPlans = filteredPlans.Where(p =>
                p.Title.ToLowerInvariant().Contains(search) ||
                p.Id.ToString().Contains(search) ||
                p.Queue.ToLowerInvariant().Contains(search));
        }

        var header = Layout.Vertical()
            | _textFilter.ToSearchInput().Placeholder("Search plans...")
            | _queueFilter.ToSelectInput(queueCounts).Placeholder("All Queues").Nullable().WithField().Label("Queue")
            | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable().WithField().Label("Level");

        var content = new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Queue).Variant(BadgeVariant.Info).Small()
                    | new Badge(plan.Level).Variant(BadgeVariant.Warning).Small())
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));

        return new HeaderLayout(header, content).Height(Size.Full());
    }
}
