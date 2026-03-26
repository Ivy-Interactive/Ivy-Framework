using Ivy;
using Ivy.Tendril.Apps.Icebox;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Icebox", icon: Icons.Snowflake, group: new[] { "Tools" }, order: 30)]
public class IceboxApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var taskService = UseService<TaskService>();
        var selectedPlanState = UseState<PlanFile?>(null);
        var queueFilter = UseState<string?>(null);
        var levelFilter = UseState<string?>(null);
        var textFilter = UseState<string?>("");
        var refreshToken = UseState(0);

        var previousPlans = UseRef<List<PlanFile>>(new List<PlanFile>());
        var plans = planService.GetIceboxPlans();
        var filteredPlans = PlanFilters.ApplyFilters(plans, queueFilter.Value, levelFilter.Value, textFilter.Value).ToList();

        // Handle removed plan - auto-select next
        if (selectedPlanState.Value is { } selected && !filteredPlans.Any(p => p.FileName == selected.FileName))
        {
            var oldIndex = previousPlans.Value.FindIndex(p => p.FileName == selected.FileName);
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
            refreshToken.Set(refreshToken.Value + 1);
        }

        var sidebar = new IceboxSidebarView(plans, selectedPlanState, queueFilter, levelFilter, textFilter);

        return new SidebarLayout(
            mainContent: new IceboxContentView(selectedPlanState.Value, filteredPlans, selectedPlanState, planService, taskService, RefreshPlans),
            sidebarContent: sidebar.BuildContent(),
            sidebarHeader: sidebar.BuildHeader()
        );
    }
}
