using System.Reactive.Disposables;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using ContentView = Ivy.Tendril.Apps.Icebox.ContentView;
using SidebarView = Ivy.Tendril.Apps.Icebox.SidebarView;

namespace Ivy.Tendril.Apps;

[App(title: "Icebox", icon: Icons.Snowflake, group: new[] { "Tools" }, order: MenuOrder.Icebox)]
public class IceboxApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<IPlanReaderService>();
        var jobService = UseService<IJobService>();
        var configService = UseService<IConfigService>();
        var planWatcher = UseService<IPlanWatcherService>();
        var selectedPlanState = UseState<PlanFile?>(null);
        var projectFilter = UseState<string?>(null);
        var levelFilter = UseState<string?>(null);
        var textFilter = UseState<string?>("");
        var refreshToken = UseRefreshToken();

        UseEffect(() =>
        {
            void OnChanged(string? _)
            {
                refreshToken.Refresh();
            }

            planWatcher.PlansChanged += OnChanged;
            return Disposable.Create(() => planWatcher.PlansChanged -= OnChanged);
        });

        var previousPlans = UseRef(new List<PlanFile>());
        var plans = planService.GetPlans(PlanStatus.Icebox);
        var filteredPlans = PlanFilters.ApplyFilters(plans, projectFilter.Value, levelFilter.Value, textFilter.Value)
            .ToList();

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
            new ContentView(selectedPlanState.Value, filteredPlans, selectedPlanState, planService, jobService,
                RefreshPlans, configService),
            sidebar
        );
    }
}