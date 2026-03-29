using System.Drawing;
using Ivy;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public class ContentView(
    PlanFile? selectedPlan,
    List<PlanFile> allPlans,
    IState<PlanFile?> selectedPlanState,
    PlanReaderService planService,
    JobService jobService,
    Action refreshPlans,
    ConfigService config) : ViewBase
{
    private readonly PlanFile? _selectedPlan = selectedPlan;
    private readonly List<PlanFile> _allPlans = allPlans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly PlanReaderService _planService = planService;
    private readonly JobService _jobService = jobService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly ConfigService _config = config;

    public override object? Build()
    {
        var downloadUrl = PlanDownloadHelper.UsePlanDownload(Context, _planService, _selectedPlan);
        var navigator = UseNavigation();
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();
        var updateDialogOpen = UseState(false);
        var splitDialogOpen = UseState(false);
        var deleteDialogOpen = UseState(false);
        var createIssueDialogOpen = UseState(false);
        var selectedRepoState = UseState<string?>(null);
        var issueAssigneeState = UseState<string?>(null);
        var issueLabelsState = UseState<string[]>(Array.Empty<string>());

        var updateText = UseState("");
        var splitText = UseState("");
        var isEditing = UseState(false);
        var editContent = UseState("");
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
                    editContent.Set(_planService.ReadRawPlan(plan.FolderName));
                }
                else
                {
                    isEditing.Set(false);
                }
            }
            else if (!isEditing.Value && isEditingPrev.Value)
            {
                if (plan != null)
                {
                    _planService.SavePlan(plan.FolderName, editContent.Value);
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
            return Layout.Vertical().Align(Align.Center).Height(Size.Full())
                | Text.Muted("Select a plan from the sidebar");
        }

        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan.FolderName);

        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
            | new Badge(_selectedPlan.Status.ToString()).Variant(BadgeVariant.Outline)
            | new Badge(_selectedPlan.Queue).Variant(BadgeVariant.Outline)
            | new Badge(_selectedPlan.Level).Variant(_config.GetBadgeVariant(_selectedPlan.Level))
            | isEditing.ToSwitchInput(Icons.Code)
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                .Muted("plans", word: true)
            | new Button("Execute").Icon(Icons.Hammer).Primary().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                _jobService.StartJob("ExecutePlan", _selectedPlan.FolderPath);
                _refreshPlans();
            });

        var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200)));

        if (isEditing.Value)
        {
            scrollableContent |= editContent.ToCodeInput()
                .Language(Languages.Markdown)
                .Width(Size.Full())
                .OnBlur(() =>
                {
                    var plan = selectedPlanRef.Value;
                    if (plan != null)
                    {
                        _planService.SavePlan(plan.FolderName, editContent.Value);
                        _refreshPlans();
                    }
                });
        }
        else
        {
            scrollableContent |= new Markdown(_selectedPlan.LatestRevisionContent)
                .DangerouslyAllowLocalFiles()
                .OnLinkClick(url =>
                {
                    if (url.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                    {
                        var filePath = url.Substring("file:///".Length);
                        navigator.Navigate<FileApp>(new FileAppArgs(filePath));
                    }
                });
        }

        var actionBar = Layout.Horizontal().Align(Align.Center).Gap(2).Padding(1)
            | new Button("Update").Icon(Icons.Pencil).Outline().OnClick(() => updateDialogOpen.Set(true))
            | new Button("Split").Icon(Icons.Scissors).Outline().OnClick(() => splitDialogOpen.Set(true))
            | new Button("Expand").Icon(Icons.UnfoldVertical).Outline().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                var planPath = _selectedPlan.FolderPath;
                _jobService.StartJob("ExpandPlan", planPath);
                _refreshPlans();
            })
            | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => deleteDialogOpen.Set(true))
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Create Issue", Icon: Icons.Github).OnSelect(() => createIssueDialogOpen.Set(true)),
                new MenuItem("Download", Icon: Icons.Download).OnSelect(() =>
                {
                    var url = downloadUrl.Value;
                    if (!string.IsNullOrEmpty(url)) client.OpenUrl(url);
                }),
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy).OnSelect(() =>
                {
                    copyToClipboard(_selectedPlan.FolderPath);
                    client.Toast("Copied path to clipboard", "Path Copied");
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
            new UpdatePlanDialog(updateDialogOpen, updateText, _selectedPlan, _jobService, _planService, _refreshPlans),
            new SplitPlanDialog(splitDialogOpen, splitText, _selectedPlan, _jobService, _planService, _refreshPlans),
            new DeletePlanDialog(deleteDialogOpen, _selectedPlan, _planService, _refreshPlans),
            new CreateIssueDialog(createIssueDialogOpen, selectedRepoState, issueAssigneeState, issueLabelsState, _selectedPlan, _jobService)
        };

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
