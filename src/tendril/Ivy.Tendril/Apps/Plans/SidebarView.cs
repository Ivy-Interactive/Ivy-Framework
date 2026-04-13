using Ivy.Tendril.Helpers;
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
    public override object Build()
    {
        var filtersOpen = UseState(false);

        var filteredPlans =
            PlanFilters.ApplyFilters(plans, projectFilter.Value, levelFilter.Value, textFilter.Value);

        var levelOptions = config.LevelNames;

        var levelFilteredPlans = plans.AsEnumerable();
        if (levelFilter.Value is { } level)
            levelFilteredPlans = levelFilteredPlans.Where(p => p.Level == level);

        var projectCounts = levelFilteredPlans
            .GroupBy(p => p.Project)
            .OrderByDescending(g => g.Count())
            .Select(g => new Option<string>($"{g.Key} ({g.Count()})", g.Key))
            .ToArray<IAnyOption>();

        var searchInput = textFilter.ToSearchInput()
            .Placeholder("Search plans...")
            .Suffix(
                new Button()
                    .Icon(filtersOpen.Value ? Icons.ChevronUp : Icons.ChevronDown)
                    .Ghost()
                    .Small()
                    .OnClick(() => filtersOpen.Set(!filtersOpen.Value))
            );

        var header = Layout.Vertical() | searchInput;

        if (filtersOpen.Value)
        {
            header |= Layout.Vertical()
                      | projectFilter.ToSelectInput(projectCounts).Placeholder("All Projects").Nullable()
                          .WithField().Label("Project")
                      | levelFilter.ToSelectInput(levelOptions.ToOptions()).Placeholder("All Levels").Nullable()
                          .WithField().Label("Level");
        }

        var content = new List(filteredPlans.Select(plan =>
        {
            var clickablePlan = plan;
            var stateBadgeVariant = StatusMappings.PlanStatusBadgeVariants.TryGetValue(plan.Status, out var variant)
                ? variant
                : BadgeVariant.Outline;

            var badges = Layout.Horizontal().Gap(1);
            if (plan.Status != PlanStatus.Draft)
                badges |= new Badge(plan.Status.ToString()).Variant(stateBadgeVariant).Small();
            var projects = ProjectHelper.ParseProjects(plan.Project);
            foreach (var proj in projects)
            {
                badges |= new Badge(proj).Variant(BadgeVariant.Outline).Small()
                    .WithProjectColor(config, proj);
            }
            badges |= new Badge(plan.Level).Variant(config.GetBadgeVariant(plan.Level)).Small();

            return new ListItem($"#{plan.Id} {plan.Title}")
                .Content(badges)
                .OnClick(() => selectedPlanState.Set(clickablePlan));
        }));

        return new HeaderLayout(header, content);
    }
}