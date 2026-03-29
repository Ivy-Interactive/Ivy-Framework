using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Review;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Review", icon: Icons.FileText, group: new[] { "Tools" }, order: 25)]
public class ReviewApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var jobService = UseService<JobService>();
        var configService = UseService<ConfigService>();
        var selectedPlanState = UseState<PlanFile?>(null);
        var queueFilter = UseState<string?>(null);
        var textFilter = UseState<string?>("");
        var refreshToken = UseState(0);

        var previousPlans = UseRef<List<PlanFile>>(new List<PlanFile>());

        var plans = planService.GetPlans()
            .Where(p => p.Status is PlanStatus.ReadyForReview or PlanStatus.Failed)
            .ToList();
        var filteredPlans = PlanFilters.ApplyFilters(plans, queueFilter.Value, null, textFilter.Value).ToList();

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
            refreshToken.Set(refreshToken.Value + 1);
        }

        var sidebar = new Review.SidebarView(plans, selectedPlanState, queueFilter, textFilter, configService);

        return new SidebarLayout(
            mainContent: new Review.ContentView(selectedPlanState.Value, filteredPlans, selectedPlanState, planService, jobService, RefreshPlans, configService),
            sidebarContent: sidebar.BuildContent(),
            sidebarHeader: sidebar.BuildHeader()
        );
    }
}
