using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class ContentView(
    Recommendation? selectedRecommendation,
    List<Recommendation> allRecommendations,
    IState<Recommendation?> selectedState,
    PlanReaderService planService,
    JobService jobService,
    Action refresh) : ViewBase
{
    private readonly Recommendation? _selected = selectedRecommendation;
    private readonly List<Recommendation> _all = allRecommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;
    private readonly PlanReaderService _planService = planService;
    private readonly JobService _jobService = jobService;
    private readonly Action _refresh = refresh;

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var showPlan = UseState<string?>(null);

        if (_selected is null)
        {
            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                | Text.Muted("Select a recommendation from the sidebar");
        }

        var currentIndex = _all.FindIndex(r => r.PlanId == _selected.PlanId && r.Title == _selected.Title);

        var stateBadgeVariant = _selected.State switch
        {
            "Accepted" => BadgeVariant.Success,
            "Declined" => BadgeVariant.Destructive,
            _ => BadgeVariant.Outline
        };

        // Header
        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block(_selected.Title).Bold()
            | new Badge(_selected.State).Variant(stateBadgeVariant)
            | new Badge($"#{_selected.PlanId}").Variant(BadgeVariant.Outline)
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_all.Count}", word: true)
                .Muted("recommendations", word: true);

        // Content
        var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200))).Gap(4).Padding(2);

        // Source plan info
        scrollableContent |= Layout.Vertical().Gap(1)
            | Text.Block("Source Plan").Bold()
            | Layout.Horizontal().Gap(2)
                | Text.Muted($"Plan #{_selected.PlanId}: {_selected.PlanTitle}")
                | Text.Muted($"Date: {_selected.Date:yyyy-MM-dd HH:mm}");

        // Description
        scrollableContent |= new Separator();
        scrollableContent |= new Markdown(_selected.Description);

        // Action bar
        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
            | new Button("Accept").Icon(Icons.Check).Primary().ShortcutKey("a").OnClick(() =>
            {
                _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "Accepted");
                _jobService.StartJob("MakePlan", "-Description", _selected.Description, "-Project", _selected.Project);
                client.Toast($"Started MakePlan: {_selected.Title}", "Recommendation Accepted");
                _refresh();
                GoToNext();
            })
            | new Button("Decline").Icon(Icons.X).Outline().ShortcutKey("x").OnClick(() =>
            {
                _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "Declined");
                _refresh();
                GoToNext();
            })
            | new Button("View Plan").Icon(Icons.ExternalLink).Outline().ShortcutKey("d").OnClick(() =>
            {
                var fullPath = Path.Combine(_planService.PlansDirectory, _selected.PlanFolderName);
                if (Directory.Exists(fullPath))
                    showPlan.Set(fullPath);
            })
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().ShortcutKey("p").OnClick(() => GoToPrevious())
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().ShortcutKey("n").OnClick(() => GoToNext());

        var mainLayout = new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: scrollableContent
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full());

        if (showPlan.Value is { } planPath)
        {
            var folderName = Path.GetFileName(planPath);
            var content = _planService.ReadLatestRevision(folderName);
            var plan = _planService.GetPlanByFolder(planPath);

            var sheetContent = string.IsNullOrEmpty(content)
                ? Text.P("Plan not found or empty.")
                : (object)new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(content))
                    .DangerouslyAllowLocalFiles();

            return new Fragment(
                mainLayout,
                new Sheet(
                    onClose: () => showPlan.Set(null),
                    content: sheetContent,
                    title: plan?.Title ?? folderName
                ).Width(Size.Half()).Resizable()
            );
        }

        return mainLayout;
    }

    private void GoToNext()
    {
        if (_all.Count == 0) return;
        var currentIndex = _all.FindIndex(r => r.PlanId == _selected?.PlanId && r.Title == _selected?.Title);
        var nextIndex = (currentIndex + 1) % _all.Count;
        _selectedState.Set(_all[nextIndex]);
    }

    private void GoToPrevious()
    {
        if (_all.Count == 0) return;
        var currentIndex = _all.FindIndex(r => r.PlanId == _selected?.PlanId && r.Title == _selected?.Title);
        var prevIndex = (currentIndex - 1 + _all.Count) % _all.Count;
        _selectedState.Set(_all[prevIndex]);
    }
}
