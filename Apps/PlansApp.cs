using System.Reactive.Disposables;
using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Drafts", icon: Icons.Feather, group: new[] { "Tools" }, order: 10)]
public class PlansApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var jobService = UseService<JobService>();
        var configService = UseService<ConfigService>();
        var planWatcher = UseService<PlanWatcherService>();
        var selectedPlanState = UseState<PlanFile?>(null);
        var projectFilter = UseState<string?>(null);
        var levelFilter = UseState<string?>(null);
        var textFilter = UseState<string?>("");
        var refreshToken = UseRefreshToken();

        UseEffect(() =>
        {
            void OnChanged() => refreshToken.Refresh();
            planWatcher.PlansChanged += OnChanged;
            return Disposable.Create(() => planWatcher.PlansChanged -= OnChanged);
        });

        var previousPlans = UseRef<List<PlanFile>>(new List<PlanFile>());

        var plans = planService.GetPlans()
            .Where(p => p.Status is PlanStatus.Draft or PlanStatus.Failed)
            .ToList();
        var filteredPlans = PlanFilters.ApplyFilters(plans, projectFilter.Value, levelFilter.Value, textFilter.Value).ToList();

        if (selectedPlanState.Value is { } selected && !filteredPlans.Any(p => p.FolderName == selected.FolderName))
        {
            var oldIndex = previousPlans.Value.FindIndex(p => p.FolderName == selected.FolderName);

            if (filteredPlans.Count > 0 && oldIndex >= 0)
            {
                var newIndex = Math.Min(oldIndex, filteredPlans.Count - 1);
                selectedPlanState.Set(filteredPlans[newIndex]);
            }
            else
            {
                selectedPlanState.Set(null);
            }
        }

        previousPlans.Value = filteredPlans;

        void RefreshPlans()
        {
            refreshToken.Refresh();
        }

        var sidebar = new SidebarView(plans, selectedPlanState, projectFilter, levelFilter, textFilter, configService);

        return new SidebarLayout(
            mainContent: new ContentView(selectedPlanState.Value, filteredPlans, selectedPlanState, planService, jobService, RefreshPlans, configService),
            sidebarContent: sidebar,
            sidebarHeader: sidebar.BuildHeader()
        );
    }
}
