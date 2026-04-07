using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public class SidebarView(
    List<PlanFile> plans,
    IState<PlanFile?> selectedPlanState,
    IState<string?> projectFilter,
    IState<string?> levelFilter,
    IState<string?> textFilter,
    IConfigService config) : ViewBase
{
    private readonly IConfigService _config = config;
    private readonly IState<string?> _levelFilter = levelFilter;
    private readonly List<PlanFile> _plans = plans;
    private readonly IState<string?> _projectFilter = projectFilter;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IState<string?> _textFilter = textFilter;

    public override object Build()
    {
        var filteredPlans =
            PlanFilters.ApplyFilters(_plans, _projectFilter.Value, _levelFilter.Value, _textFilter.Value);

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
                         "Filters",
                         Layout.Vertical()
                         | _projectFilter.ToSelectInput(projectCounts).Placeholder("All Projects").Nullable()
                             .WithField().Label("Project")
                         | _levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable()
                             .WithField().Label("Level")
                     ).Open(false).Ghost();

        var content = new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            var stateBadgeVariant = StatusMappings.PlanStatusBadgeVariants.TryGetValue(plan.Status, out var variant)
                ? variant
                : BadgeVariant.Outline;

            var badges = Layout.Horizontal().Gap(1);
            if (plan.Status != PlanStatus.Draft)
                badges |= new Badge(plan.Status.ToString()).Variant(stateBadgeVariant).Small();
            badges |= new Badge(plan.Project).Variant(BadgeVariant.Outline).Small()
                .WithProjectColor(_config, plan.Project);
            badges |= new Badge(plan.Level).Variant(_config.GetBadgeVariant(plan.Level)).Small();

            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(badges)
                .OnClick(() => _selectedPlanState.Set(clickablePlan));
        }));

        return new HeaderLayout(header, content);
    }
}