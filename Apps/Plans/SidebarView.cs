using Ivy;
using Ivy.Tendril.Apps.Plans.Dialogs;

namespace Ivy.Tendril.Apps.Plans;

public class SidebarView : ViewBase
{
    private readonly List<PlanFile> _plans;
    private readonly IState<PlanFile?> _selectedPlanState;
    private readonly IState<string?> _queueFilter;
    private readonly IState<string?> _levelFilter;
    private readonly IState<string?> _textFilter;
    private readonly Action<string> _onCreatePlan;

    public SidebarView(
        List<PlanFile> plans,
        IState<PlanFile?> selectedPlanState,
        IState<string?> queueFilter,
        IState<string?> levelFilter,
        IState<string?> textFilter,
        Action<string> onCreatePlan)
    {
        _plans = plans;
        _selectedPlanState = selectedPlanState;
        _queueFilter = queueFilter;
        _levelFilter = levelFilter;
        _textFilter = textFilter;
        _onCreatePlan = onCreatePlan;
    }

    public override object Build()
    {
        var createDialogOpen = UseState(false);

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

        var createButton = new Button("Create Plan")
            .Primary()
            .Width(Size.Full())
            .Icon(Icons.Plus)
            .OnClick(() => createDialogOpen.Set(true));

        var header = Layout.Vertical().Padding(2)
            | createButton
            | _textFilter.ToTextInput().Placeholder("Search plans...").Small().WithField().Label("Search")
            | _queueFilter.ToSelectInput(queueCounts).Placeholder("All Queues").Nullable().Small().WithField().Label("Queue")
            | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable().Small().WithField().Label("Level");

        var content = new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Queue).Variant(BadgeVariant.Info).Small()
                    | new Badge(plan.Level).Variant(BadgeVariant.Warning).Small())
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));

        var elements = new List<object>
        {
            new HeaderLayout(header, content).Height(Size.Full())
        };

        if (createDialogOpen.Value)
        {
            elements.Add(new CreatePlanDialog(
                onCreatePlan: _onCreatePlan,
                onClose: () => createDialogOpen.Set(false)
            ));
        }

        return new Fragment(elements.ToArray());
    }
}
