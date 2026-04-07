using System.Reactive.Disposables;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using ContentView = Ivy.Tendril.Apps.Review.ContentView;
using SidebarView = Ivy.Tendril.Apps.Review.SidebarView;

namespace Ivy.Tendril.Apps;

[App(title: "Review", icon: Icons.ThumbsUp, group: new[] { "Tools" }, order: MenuOrder.Review,
    allowDuplicateTabs: true)]
public class ReviewApp : ViewBase
{
    public override object Build()
    {
        var planService = UseService<IPlanReaderService>();
        var jobService = UseService<IJobService>();
        var configService = UseService<IConfigService>();
        var gitService = UseService<IGitService>();
        var planWatcher = UseService<IPlanWatcherService>();
        var selectedPlanState = UseState<PlanFile?>(null);
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

        var plans = planService.GetPlans()
            .Where(p => p.Status is PlanStatus.ReadyForReview or PlanStatus.Failed)
            .ToList();
        var filteredPlans = PlanFilters.ApplyFilters(plans, null, null, textFilter.Value).ToList();

        if (selectedPlanState.Value == null && filteredPlans.Count > 0) selectedPlanState.Set(filteredPlans[0]);

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

        var sidebar = new SidebarView(plans, selectedPlanState, textFilter, configService);

        return new SidebarLayout(
            new ContentView(selectedPlanState.Value, filteredPlans, selectedPlanState, planService, jobService,
                RefreshPlans, configService, gitService),
            sidebar
        );
    }
}
