using System.Reactive.Disposables;
using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Review;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Review", icon: Icons.ThumbsUp, group: new[] { "Tools" }, order: 25)]
public class ReviewApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var jobService = UseService<JobService>();
        var configService = UseService<ConfigService>();
        var gitService = UseService<GitService>();
        var planWatcher = UseService<PlanWatcherService>();
        var selectedPlanState = UseState<PlanFile?>(null);
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
            .Where(p => p.Status is PlanStatus.ReadyForReview or PlanStatus.Failed)
            .ToList();
        var filteredPlans = PlanFilters.ApplyFilters(plans, null, null, textFilter.Value).ToList();

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

        var sidebar = new Review.SidebarView(plans, selectedPlanState, textFilter, configService);

        return new SidebarLayout(
            mainContent: new Review.ContentView(selectedPlanState.Value, filteredPlans, selectedPlanState, planService, jobService, RefreshPlans, configService, gitService),
            sidebarContent: sidebar.BuildContent(),
            sidebarHeader: sidebar.BuildHeader()
        );
    }
}
