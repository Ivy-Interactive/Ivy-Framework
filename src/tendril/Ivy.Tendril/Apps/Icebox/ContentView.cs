using Ivy;
using Ivy.Core;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox;

public class ContentView(
    PlanFile? selectedPlan,
    List<PlanFile> allPlans,
    IState<PlanFile?> selectedPlanState,
    IPlanReaderService planService,
    IJobService jobService,
    Action refreshPlans,
    IConfigService config) : ViewBase
{
    private readonly PlanFile? _selectedPlan = selectedPlan;
    private readonly List<PlanFile> _allPlans = allPlans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly IPlanReaderService _planService = planService;
    private readonly IJobService _jobService = jobService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly IConfigService _config = config;

    public override object? Build()
    {
        var downloadUrl = PlanDownloadHelper.UsePlanDownload(Context, _planService, _selectedPlan);
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();
        var deleteDialogOpen = UseState(false);
        var openFile = UseState<string?>(null);

        var isEditing = UseState(false);
        var editContent = UseState("");
        var originalContent = UseState("");
        var isEditingPrev = UseState(false);
        var lastPlanId = UseState(_selectedPlan?.Id ?? -1);

        var selectedPlanRef = UseRef(_selectedPlan);

        UseEffect(() =>
        {
            var plan = selectedPlanRef.Value;
            if (isEditing.Value && !isEditingPrev.Value)
            {
                if (plan != null)
                {
                    var raw = _planService.ReadRawPlan(plan.FolderName);
                    editContent.Set(raw);
                    originalContent.Set(raw);
                }
                else
                {
                    isEditing.Set(false);
                }
            }
            else if (!isEditing.Value && isEditingPrev.Value)
            {
                if (plan != null && editContent.Value != originalContent.Value)
                {
                    _planService.SaveRevision(plan.FolderName, editContent.Value);
                    _refreshPlans();
                }
            }
            isEditingPrev.Set(isEditing.Value);
        }, isEditing);

#pragma warning disable CS8601
        selectedPlanRef.Value = _selectedPlan;
#pragma warning restore CS8601

        if (lastPlanId.Value != (_selectedPlan?.Id ?? -1))
        {
            lastPlanId.Set(_selectedPlan?.Id ?? -1);
            isEditing.Set(false);
        }

        if (_selectedPlan is null)
        {
            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                | Text.Muted("Select a plan from the sidebar");
        }

        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan.FolderName);

        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
            | new Badge(_selectedPlan.Project).Variant(BadgeVariant.Outline).WithProjectColor(_config, _selectedPlan.Project)
            | new Badge(_selectedPlan.Level).Variant(_config.GetBadgeVariant(_selectedPlan.Level))
            | isEditing.ToSwitchInput(Icons.Pencil).Label("Edit")
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                .Muted("plans", word: true)
            ;

        var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200)));

        if (isEditing.Value)
        {
            scrollableContent |= editContent.ToCodeInput()
                .Language(Languages.Markdown)
                .Width(Size.Full())
                .OnBlur(() =>
                {
                    var plan = selectedPlanRef.Value;
                    if (plan != null && editContent.Value != originalContent.Value)
                    {
                        _planService.SaveRevision(plan.FolderName, editContent.Value);
                        originalContent.Set(editContent.Value);
                        _refreshPlans();
                    }
                });
        }
        else
        {
            scrollableContent |= new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(_selectedPlan.LatestRevisionContent))
                .DangerouslyAllowLocalFiles()
                .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile));
        }

        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
            | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => deleteDialogOpen.Set(true))
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Button("Thaw").Icon(Icons.Flame).Primary().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Draft);
                _refreshPlans();
            })
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Download", Icon: Icons.Download, Tag: "Download").OnSelect(() =>
                {
                    var url = downloadUrl.Value;
                    if (!string.IsNullOrEmpty(url)) client.OpenUrl(url);
                }),
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy, Tag: "CopyPath").OnSelect(() =>
                {
                    copyToClipboard(_selectedPlan.FolderPath);
                    client.Toast("Copied path to clipboard", "Path Copied");
                }),
                new MenuItem("Open plan.yaml", Icon: Icons.FileText, Tag: "OpenPlanYaml").OnSelect(() =>
                {
                    var yamlPath = System.IO.Path.Combine(_selectedPlan.FolderPath, "plan.yaml");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _config.Editor.Command,
                        Arguments = yamlPath,
                        UseShellExecute = true
                    });
                })
            );

        var mainContent = Layout.Vertical()
            | scrollableContent;

        var mainLayout = new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: mainContent
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full()).Key(_selectedPlan.Id);

        var elements = new List<object>
        {
            mainLayout,
            new Icebox.Dialogs.DeletePlanDialog(deleteDialogOpen, _selectedPlan, _planService, _refreshPlans)
        };

        var repoPaths = _selectedPlan.GetEffectiveRepoPaths(_config);
        var fileLinkSheet = FileLinkHelper.BuildFileLinkSheet(
            openFile.Value, () => openFile.Set(null), repoPaths, _config.Editor.Command, _config.Editor.Label);
        if (fileLinkSheet is not null)
            elements.Add(fileLinkSheet);

        return new Fragment(elements.ToArray());
    }

    private void GoToNext()
    {
        if (_allPlans.Count == 0) return;
        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan?.FolderName);
        var nextIndex = (currentIndex + 1) % _allPlans.Count;
        _selectedPlanState.Set(_allPlans[nextIndex]);
    }

    private void GoToPrevious()
    {
        if (_allPlans.Count == 0) return;
        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan?.FolderName);
        var prevIndex = (currentIndex - 1 + _allPlans.Count) % _allPlans.Count;
        _selectedPlanState.Set(_allPlans[prevIndex]);
    }
}
