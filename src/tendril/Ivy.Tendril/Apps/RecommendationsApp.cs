using Ivy;
using Ivy.Tendril.Apps.Recommendations;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Recommendations", icon: Icons.Lightbulb, group: new[] { "Tools" }, order: 15)]
public class RecommendationsApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var jobService = UseService<JobService>();
        var refreshToken = UseRefreshToken();
        var selectedState = UseState<Recommendation?>(null);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromMinutes(1));

        var recommendations = planService.GetRecommendations();

        var filtered = recommendations.Where(r => r.State == "Pending").ToList();

        // If selected recommendation is no longer in filtered list, adjust selection
        if (selectedState.Value is { } selected && !filtered.Any(r => r.PlanId == selected.PlanId && r.Title == selected.Title))
        {
            selectedState.Set(filtered.Count > 0 ? filtered[0] : null);
        }

        void Refresh() => refreshToken.Refresh();

        var sidebar = new Recommendations.SidebarView(filtered, selectedState);

        return new SidebarLayout(
            mainContent: new Recommendations.ContentView(selectedState.Value, filtered, selectedState, planService, jobService, Refresh),
            sidebarContent: sidebar
        );
    }
}
