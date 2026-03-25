using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Plans", icon: Icons.ClipboardList, group: new[] { "Tools" }, order: 10)]
public class PlansApp : ViewBase
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

        // Store previous plans list to track position
        var previousPlans = UseRef<List<PlanFile>>(new List<PlanFile>());

        var plans = planService.GetPlans();

        // Handle removed plan - auto-select next
        if (selectedPlanState.Value is { } selected && !plans.Any(p => p.FileName == selected.FileName))
        {
            // Find position of removed plan in old list
            var oldIndex = previousPlans.Value.FindIndex(p => p.FileName == selected.FileName);

            if (plans.Count > 0 && oldIndex >= 0)
            {
                // Select plan at same position (or last plan if index out of bounds)
                var newIndex = Math.Min(oldIndex, plans.Count - 1);
                selectedPlanState.Set(plans[newIndex]);
            }
            else
            {
                selectedPlanState.Set(null);
            }
        }

        // Update stored plans for next comparison
        previousPlans.Value = plans;

        void RefreshPlans()
        {
            selectedPlanState.Set(null);
            refreshToken.Set(refreshToken.Value + 1);
        }

        return new SidebarLayout(
            mainContent: new ContentView(selectedPlanState.Value, plans, selectedPlanState, planService, taskService, RefreshPlans),
            sidebarContent: new SidebarView(plans, selectedPlanState, queueFilter, levelFilter, textFilter)
        );
    }
}
