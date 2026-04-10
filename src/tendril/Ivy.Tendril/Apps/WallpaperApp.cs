using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(isVisible: false)]
public class WallpaperApp : ViewBase
{
    public override object Build()
    {
        var navigator = UseNavigation();
        var countsService = UseService<IPlanCountsService>();
        var counts = countsService.Current;

        var hasActivity = counts.Drafts > 0 || counts.ActiveJobs > 0 || counts.Reviews > 0;

        var heading = hasActivity ? "What are we making next?" : "Welcome to Ivy Tendril";
        var subtitle = hasActivity ? BuildSummary(counts) : "Manage your plans, track jobs, and review pull requests.";
        var buttonLabel = hasActivity ? "New Plan" : "Create your first Plan";

        return Layout.Center()
               | (Layout.Vertical().Gap(2).AlignContent(Align.Center)
                  | new Image("/tendril/assets/Tendril.svg").Width(Size.Units(30)).Height(Size.Auto())
                  | Text.H2(heading)
                  | Text.Muted(subtitle)
                  | new Button(buttonLabel, () => navigator.Navigate<PlansApp>())
                      .Variant(ButtonVariant.Primary)
               );
    }

    private static string BuildSummary(PlanCounts counts)
    {
        var parts = new List<string>();

        if (counts.Drafts > 0)
            parts.Add($"{counts.Drafts} {(counts.Drafts == 1 ? "draft" : "drafts")}");
        if (counts.ActiveJobs > 0)
            parts.Add($"{counts.ActiveJobs} {(counts.ActiveJobs == 1 ? "job" : "jobs")} running");
        if (counts.Reviews > 0)
            parts.Add($"{counts.Reviews} {(counts.Reviews == 1 ? "review" : "reviews")} waiting");

        return "You have " + string.Join(", ", parts) + ".";
    }
}
