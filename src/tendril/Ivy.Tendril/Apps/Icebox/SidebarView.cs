using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox;

public class SidebarView(
    List<PlanFile> plans,
    IState<PlanFile?> selectedPlanState,
    IState<string?> projectFilter,
    IState<string?> levelFilter,
    IState<string?> textFilter,
    IConfigService config) : ViewBase
{
    private readonly List<PlanFile> _plans = plans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IState<string?> _projectFilter = projectFilter;
    private readonly IState<string?> _levelFilter = levelFilter;
    private readonly IState<string?> _textFilter = textFilter;
    private readonly IConfigService _config = config;

    public override object Build()
    {
        var filteredPlans = PlanFilters.ApplyFilters(_plans, _projectFilter.Value, _levelFilter.Value, _textFilter.Value);

        var levelOptions = _config.LevelNames;

        var levelFilteredPlans = _plans.AsEnumerable();
        if (_levelFilter.Value is { } level)
            levelFilteredPlans = levelFilteredPlans.Where(p => p.Level == level);

        var projectCounts = levelFilteredPlans
            .GroupBy(p => p.Project)
            .OrderByDescending(g => g.Count())
            .Select(g => new Option<string>($"{g.Key} ({g.Count()})", g.Key))
            .ToArray<IAnyOption>();

        var header = Layout.Vertical()
            | _textFilter.ToSearchInput().Placeholder("Search plans...")
            | new Expandable(
                header: "Filters",
                content: Layout.Vertical()
                    | _projectFilter.ToSelectInput(projectCounts).Placeholder("All Projects").Nullable().WithField().Label("Project")
                    | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable().WithField().Label("Level")
            ).Open(false).Ghost();

        var content = new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Project).Variant(BadgeVariant.Outline).Small().WithProjectColor(_config, plan.Project)
                    | new Badge(plan.Level).Variant(_config.GetBadgeVariant(plan.Level)).Small())
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));

        return new HeaderLayout(header, content);
    }
}
