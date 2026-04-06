using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Recommendations.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class ContentView(
    Recommendation? selectedRecommendation,
    List<Recommendation> allRecommendations,
    IState<Recommendation?> selectedState,
    IPlanReaderService planService,
    IJobService jobService,
    Action refresh) : ViewBase
{
    private readonly Recommendation? _selected = selectedRecommendation;
    private readonly List<Recommendation> _all = allRecommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;
    private readonly IPlanReaderService _planService = planService;
    private readonly IJobService _jobService = jobService;
    private readonly Action _refresh = refresh;

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var config = UseService<IConfigService>();
        var showPlan = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var showNotesDialog = UseState(false);
        var showDeclineDialog = UseState<bool>(false);
        var declineReason = UseState<string?>("");

        if (_selected is null)
        {
            if (_all.Count == 0)
            {
                return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full()).Gap(2)
                    | new Icon(Icons.Inbox).Large().Color(Colors.Gray)
                    | Text.Muted("No recommendations yet");
            }

            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                | Text.Muted("Select a recommendation from the sidebar");
        }

        var currentIndex = _all.FindIndex(r => r.PlanId == _selected.PlanId && r.Title == _selected.Title);

        // Header with Accept action at right edge
        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block(_selected.Title).Bold()
            | new Badge($"#{_selected.PlanId}").Variant(BadgeVariant.Outline)
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_all.Count}", word: true)
                .Muted("recommendations", word: true)
            | new Button("Accept").Icon(Icons.Check).Primary().ShortcutKey("a").OnClick(() =>
            {
                _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "Accepted");
                _jobService.StartJob("MakePlan", "-Description", _selected.Description, "-Project", _selected.Project);
                client.Toast($"Started MakePlan: {_selected.Title}", "Recommendation Accepted");
                _refresh();
                GoToNext();
            });

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

        // Action bar (secondary actions)
        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
            | new Button("Decline").Icon(Icons.X).Outline().ShortcutKey("x").OnClick(() =>
            {
                declineReason.Set("");
                showDeclineDialog.Set(true);
            })
            | new Button("Accept with Notes").Icon(Icons.CircleCheck).Outline().ShortcutKey("w").OnClick(() => showNotesDialog.Set(true))
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

        var notesDialog = new AcceptWithNotesDialog(
            showNotesDialog,
            _selected,
            notes =>
            {
                var description = $"[ORIGINAL RECOMMENDATION]\n{_selected.Description}\n\n[NOTES]\n{notes}";
                _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "AcceptedWithNotes");
                _jobService.StartJob("MakePlan", "-Description", description, "-Project", _selected.Project);
                client.Toast($"Started MakePlan: {_selected.Title}", "Recommendation Accepted with Notes");
                _refresh();
                GoToNext();
            });

        if (showDeclineDialog.Value)
        {
            var selectedForDecline = _selected;
            return new Fragment(
                mainLayout,
                new Dialog(
                    _ => { declineReason.Set(""); showDeclineDialog.Set(false); },
                    new DialogHeader("Decline Recommendation"),
                    new DialogBody(
                        Layout.Vertical()
                            | Text.P("Optionally provide a reason for declining this recommendation.")
                            | declineReason.ToTextareaInput("Enter reason (optional)...").Rows(4).AutoFocus()
                    ),
                    new DialogFooter(
                        new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() =>
                        {
                            declineReason.Set("");
                            showDeclineDialog.Set(false);
                        }),
                        new Button("Decline").Destructive().ShortcutKey("Enter").OnClick(() =>
                        {
                            _planService.UpdateRecommendationState(
                                selectedForDecline.PlanFolderName,
                                selectedForDecline.Title,
                                "Declined",
                                declineReason.Value
                            );
                            _refresh();
                            showDeclineDialog.Set(false);
                            declineReason.Set("");
                            GoToNext();
                        })
                    )
                ).Width(Size.Rem(40)),
                notesDialog
            );
        }

        if (showPlan.Value is { } planPath)
        {
            var folderName = Path.GetFileName(planPath);
            var content = _planService.ReadLatestRevision(folderName);
            var plan = _planService.GetPlanByFolder(planPath);

            var sheetContent = string.IsNullOrEmpty(content)
                ? Text.P("Plan not found or empty.")
                : (object)new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(content))
                    .DangerouslyAllowLocalFiles()
                    .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile));

            var planSheet = new Sheet(
                onClose: () => showPlan.Set(null),
                content: sheetContent,
                title: plan?.Title ?? folderName
            ).Width(Size.Half()).Resizable();

            var repoPaths = plan?.GetEffectiveRepoPaths(config) ?? [];
            var fileLinkSheet = FileLinkHelper.BuildFileLinkSheet(
                openFile.Value,
                () => openFile.Set(null),
                repoPaths,
                config.Editor.Command,
                config.Editor.Label);

            if (fileLinkSheet is not null)
            {
                return new Fragment(
                    mainLayout,
                    planSheet,
                    fileLinkSheet,
                    notesDialog
                );
            }

            return new Fragment(mainLayout, planSheet, notesDialog);
        }

        return new Fragment(mainLayout, notesDialog);
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
