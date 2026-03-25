using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Plans", icon: Icons.ClipboardList, group: new[] { "Tools" })]
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

        var plans = planService.GetPlans();

        if (selectedPlanState.Value is { } selected && !plans.Any(p => p.FileName == selected.FileName))
        {
            selectedPlanState.Set(null);
        }

        void RefreshPlans()
        {
            selectedPlanState.Set(null);
            refreshToken.Set(refreshToken.Value + 1);
        }

        return new SidebarLayout(
            mainContent: new ContentView(selectedPlanState.Value, plans, selectedPlanState, planService, taskService, RefreshPlans),
            sidebarContent: new SidebarView(plans, selectedPlanState, queueFilter, levelFilter, textFilter, description =>
            {
                taskService.StartTask("MakePlan", description);
                RefreshPlans();
            })
        );
    }
}
