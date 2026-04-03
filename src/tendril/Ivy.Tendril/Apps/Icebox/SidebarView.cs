using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox;

public class SidebarView(
    List<PlanFile> plans,
    IState<PlanFile?> selectedPlanState,
    IState<string?> projectFilter,
    IState<string?> levelFilter,
    IState<string?> textFilter,
    ConfigService config) : ViewBase
{
    private readonly List<PlanFile> _plans = plans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IState<string?> _projectFilter = projectFilter;
    private readonly IState<string?> _levelFilter = levelFilter;
    private readonly IState<string?> _textFilter = textFilter;
    private readonly ConfigService _config = config;

    public object BuildHeader()
    {
        var levelOptions = _config.LevelNames;

        var levelFilteredPlans = _plans.AsEnumerable();
        if (_levelFilter.Value is { } level)
            levelFilteredPlans = levelFilteredPlans.Where(p => p.Level == level);

        var projectCounts = levelFilteredPlans
            .GroupBy(p => p.Project)
            .OrderByDescending(g => g.Count())
            .Select(g => new Option<string>($"{g.Key} ({g.Count()})", g.Key))
            .ToArray<IAnyOption>();

        return Layout.Vertical()
            | _textFilter.ToSearchInput().Placeholder("Search plans...")
            | new Expandable(
                header: "Filters",
                content: Layout.Vertical()
                    | _projectFilter.ToSelectInput(projectCounts).Placeholder("All Projects").Nullable().WithField().Label("Project")
                    | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable().WithField().Label("Level")
            ).Open(false).Ghost()
            ;
    }

    public object BuildContent()
    {
        var filteredPlans = PlanFilters.ApplyFilters(_plans, _projectFilter.Value, _levelFilter.Value, _textFilter.Value);

        return new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(plan.Project).Variant(BadgeVariant.Outline).Small().WithProjectColor(_config, plan.Project)
                    | new Badge(plan.Level).Variant(_config.GetBadgeVariant(plan.Level)).Small())
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));
    }

    public override object Build()
    {
        return BuildContent();
    }
}
