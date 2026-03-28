using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public class SidebarView(
    List<PlanFile> plans,
    IState<PlanFile?> selectedPlanState,
    IState<string?> queueFilter,
    IState<string?> levelFilter,
    IState<string?> textFilter,
    ConfigService config) : ViewBase
{
    private readonly List<PlanFile> _plans = plans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IState<string?> _queueFilter = queueFilter;
    private readonly IState<string?> _levelFilter = levelFilter;
    private readonly IState<string?> _textFilter = textFilter;
    private readonly ConfigService _config = config;

    public object BuildHeader()
    {
        var levelOptions = _config.LevelNames;

        var levelFilteredPlans = _plans.AsEnumerable();
        if (_levelFilter.Value is { } level)
            levelFilteredPlans = levelFilteredPlans.Where(p => p.Level == level);

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
                    | _queueFilter.ToSelectInput(queueCounts).Placeholder("All Projects").Nullable().WithField().Label("Project")
                    | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable().WithField().Label("Level")
            ).Open(false).Ghost();
    }

    public override object Build()
    {
        var filteredPlans = PlanFilters.ApplyFilters(_plans, _queueFilter.Value, _levelFilter.Value, _textFilter.Value);

        return new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            var stateBadgeVariant = plan.Status switch
            {
                PlanStatus.Building or PlanStatus.Updating => BadgeVariant.Info,
                PlanStatus.ReadyForReview => BadgeVariant.Success,
                _ => BadgeVariant.Outline
            };

            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Status.ToString()).Variant(stateBadgeVariant).Small()
                    | new Badge(plan.Queue).Variant(BadgeVariant.Outline).Small()
                    | new Badge(plan.Level).Variant(_config.GetBadgeVariant(plan.Level)).Small())
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));
    }
}
