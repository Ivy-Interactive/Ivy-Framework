using Ivy.Tendril.Apps.Recommendations;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Recommendations", icon: Icons.Lightbulb, group: ["Apps"], order: MenuOrder.Recommendations)]
public class RecommendationsApp : ViewBase
{
    public override object Build()
    {
        var planService = UseService<IPlanReaderService>();
        var jobService = UseService<IJobService>();
        var refreshToken = UseRefreshToken();
        var selectedState = UseState<Recommendation?>(null);
        var projectFilter = UseState<string?>(null);
        var planStatusFilter = UseState<string?>(null);
        var textFilter = UseState<string?>("");

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromMinutes(1));

        var recommendations = planService.GetRecommendations();

        var allPending = recommendations.Where(r => r.State == "Pending").ToList();

        var filtered = allPending
            .Where(r => projectFilter.Value == null || r.Project == projectFilter.Value)
            .Where(r => planStatusFilter.Value == null || r.SourcePlanStatus.ToString() == planStatusFilter.Value)
            .Where(r =>
            {
                if (string.IsNullOrWhiteSpace(textFilter.Value)) return true;
                var search = textFilter.Value.ToLowerInvariant();
                return r.Title.ToLowerInvariant().Contains(search) ||
                       r.Description.ToLowerInvariant().Contains(search) ||
                       r.PlanId.Contains(search) ||
                       r.PlanTitle.ToLowerInvariant().Contains(search);
            })
            .ToList();

        if (selectedState.Value == null && filtered.Count > 0) selectedState.Set(filtered[0]);

        // If selected recommendation is no longer in filtered list, adjust selection
        if (selectedState.Value is { } selected &&
            !filtered.Any(r => r.PlanId == selected.PlanId && r.Title == selected.Title))
            selectedState.Set(filtered.Count > 0 ? filtered[0] : null);

        void Refresh()
        {
            refreshToken.Refresh();
        }

        var totalPendingCount = allPending.Count;
        var hasActiveFilters = projectFilter.Value != null || planStatusFilter.Value != null ||
                               !string.IsNullOrWhiteSpace(textFilter.Value);

        var sidebar = new SidebarView(
            allPending,
            selectedState,
            projectFilter,
            planStatusFilter,
            totalPendingCount,
            hasActiveFilters,
            textFilter
        );

        return new SidebarLayout(
            new ContentView(selectedState.Value, filtered, selectedState, planService, jobService, Refresh),
            sidebar
        );
    }
}
