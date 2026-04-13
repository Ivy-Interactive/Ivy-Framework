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
    private readonly List<Recommendation> _all = allRecommendations;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refresh = refresh;
    private readonly Recommendation? _selected = selectedRecommendation;
    private readonly IState<Recommendation?> _selectedState = selectedState;

    public override object Build()
    {
        var client = UseService<IClientProvider>();
        var config = UseService<IConfigService>();
        var copyToClipboard = UseClipboard();
        var showPlan = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var showNotesDialog = UseState(false);
        var showDeclineDialog = UseState<bool>();

        if (_selected is null)
        {
            if (_all.Count == 0)
                return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full()).Gap(2)
                       | new Icon(Icons.Inbox).Large().Color(Colors.Gray)
                       | Text.Muted("No recommendations yet");

            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                   | Text.Muted("Select a recommendation from the sidebar");
        }

        var currentIndex = _all.FindIndex(r => r.PlanId == _selected.PlanId && r.Title == _selected.Title);

        // Header with Accept action at right edge
        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
                     | Text.Block($"#{_selected.PlanId} {_selected.Title}").Bold()
                     | new Badge(_selected.Project).Variant(BadgeVariant.Outline)
                         .WithProjectColor(config, _selected.Project)
                     | new Spacer().Width(Size.Grow())
                     | Text.Rich()
                         .Bold($"{currentIndex + 1}/{_all.Count}", word: true)
                         .Muted("recommendations", word: true)
                     | new Button("Accept").Icon(Icons.Check).Primary().ShortcutKey("a").OnClick(() =>
                     {
                         _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "Accepted");
                         _jobService.StartJob("MakePlan", "-Description", _selected.Description, "-Project",
                             _selected.Project);
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
                            showDeclineDialog.Set(true);
                        })
                        | new Button("Accept with Notes").Icon(Icons.CircleCheck).Outline().ShortcutKey("w")
                            .OnClick(() => showNotesDialog.Set(true))
                        | new Button("View Plan").Icon(Icons.ExternalLink).Outline().ShortcutKey("d").OnClick(() =>
                        {
                            var fullPath = Path.Combine(_planService.PlansDirectory, _selected.PlanFolderName);
                            if (Directory.Exists(fullPath))
                                showPlan.Set(fullPath);
                        })
                        | new Button("Previous").Icon(Icons.ChevronLeft).Outline().ShortcutKey("p")
                            .OnClick(() => GoToPrevious())
                        | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().ShortcutKey("n")
                            .OnClick(() => GoToNext())
                        | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                            new MenuItem("Open in File Manager", Icon: Icons.FolderOpen, Tag: "OpenInExplorer")
                                .OnSelect(() =>
                                {
                                    var fullPath = Path.Combine(_planService.PlansDirectory, _selected.PlanFolderName);
                                    if (Directory.Exists(fullPath))
                                        PlatformHelper.OpenInFileManager(fullPath);
                                }),
                            new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy, Tag: "CopyPath")
                                .OnSelect(() =>
                                {
                                    var fullPath = Path.Combine(_planService.PlansDirectory, _selected.PlanFolderName);
                                    copyToClipboard(fullPath);
                                    client.Toast("Copied path to clipboard", "Path Copied");
                                }),
                            new MenuItem("Open plan.yaml", Icon: Icons.FileText, Tag: "OpenPlanYaml").OnSelect(() =>
                            {
                                var fullPath = Path.Combine(_planService.PlansDirectory, _selected.PlanFolderName);
                                var yamlPath = Path.Combine(fullPath, "plan.yaml");
                                config.OpenInEditor(yamlPath);
                            })
                        );

        var mainLayout = new HeaderLayout(
            header,
            new FooterLayout(
                actionBar,
                scrollableContent
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

        var declineDialog = new DeclineRecommendationDialog(
            showDeclineDialog, _selected, _planService, _refresh, GoToNext);

        if (showPlan.Value is { } planPath)
        {
            var folderName = Path.GetFileName(planPath);
            var content = _planService.ReadLatestRevision(folderName);
            var plan = _planService.GetPlanByFolder(planPath);

            var sheetContent = string.IsNullOrEmpty(content)
                ? Text.P("Plan not found or empty.")
                : (object)new Markdown(MarkdownHelper.AnnotateBrokenPlanLinks(
                        MarkdownHelper.AnnotateBrokenFileLinks(content),
                        _planService.PlansDirectory))
                    .DangerouslyAllowLocalFiles()
                    .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile));

            var planSheet = new Sheet(
                () => showPlan.Set(null),
                sheetContent,
                plan?.Title ?? folderName
            ).Width(Size.Half()).Resizable();

            var repoPaths = plan?.GetEffectiveRepoPaths(config) ?? [];
            var fileLinkSheet = FileLinkHelper.BuildFileLinkSheet(
                openFile.Value,
                () => openFile.Set(null),
                repoPaths,
                config);

            if (fileLinkSheet is not null)
                return new Fragment(
                    mainLayout,
                    planSheet,
                    fileLinkSheet,
                    notesDialog,
                    declineDialog
                );

            return new Fragment(mainLayout, planSheet, notesDialog, declineDialog);
        }

        return new Fragment(mainLayout, notesDialog, declineDialog);
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
