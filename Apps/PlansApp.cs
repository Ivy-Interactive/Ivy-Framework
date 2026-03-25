using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Apps;

[App(title: "Plans", icon: Icons.ClipboardList, group: new[] { "Tools" })]
public class PlansApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var selectedPlanState = UseState<PlanFile?>(null);
        var queueFilter = UseState<string?>(null);
        var levelFilter = UseState<string?>(null);
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
            mainContent: new ContentView(selectedPlanState.Value, plans, selectedPlanState, planService, RefreshPlans),
            sidebarContent: new SidebarView(plans, selectedPlanState, queueFilter, levelFilter)
        );
    }
}
