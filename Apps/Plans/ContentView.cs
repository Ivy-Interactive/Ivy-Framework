using System.Drawing;
using Ivy;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public class ContentView : ViewBase
{
    private readonly PlanFile? _selectedPlan;
    private readonly List<PlanFile> _allPlans;
    private readonly IState<PlanFile?> _selectedPlanState;
    private readonly PlanReaderService _planService;
    private readonly TaskService _taskService;
    private readonly Action _refreshPlans;

    public ContentView(
        PlanFile? selectedPlan,
        List<PlanFile> allPlans,
        IState<PlanFile?> selectedPlanState,
        PlanReaderService planService,
        TaskService taskService,
        Action refreshPlans)
    {
        _selectedPlan = selectedPlan;
        _allPlans = allPlans;
        _selectedPlanState = selectedPlanState;
        _planService = planService;
        _taskService = taskService;
        _refreshPlans = refreshPlans;
    }

    public override object? Build()
    {
        var downloadUrl = PlanDownloadHelper.UsePlanDownload(this, _planService, _selectedPlan);
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
                // Entering edit mode - load content
                if (plan != null)
                {
                    editContent.Set(_planService.ReadRawPlan(plan.FileName));
                }
                else
                {
                    isEditing.Set(false);
                }
            }
            else if (!isEditing.Value && isEditingPrev.Value)
            {
                // Leaving edit mode - save content
                if (plan != null)
                {
                    _planService.SavePlan(plan.FileName, editContent.Value);
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

        var currentIndex = _allPlans.FindIndex(p => p.FileName == _selectedPlan.FileName);

        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
            | new Badge(_selectedPlan.Queue).Variant(BadgeVariant.Info)
            | new Badge(_selectedPlan.Level).Variant(BadgeVariant.Warning)
            | isEditing.ToSwitchInput(Icons.Pencil).Label("Edit")
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                .Muted("plans", word: true)
            | new Button("Approve").Icon(Icons.Check).Primary().OnClick(() =>
            {
                _planService.ApprovePlan(_selectedPlan.FileName);
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
                        _planService.SavePlan(plan.FileName, editContent.Value);
                        _refreshPlans();
                    }
                });
        }
        else
        {
            scrollableContent |= new Markdown(_selectedPlan.Content).DangerouslyAllowLocalFiles();
        }

        var actionBar = Layout.Horizontal().Align(Align.Center).Gap(2).Padding(1)
            | new Button("Update").Icon(Icons.Pencil).Outline().OnClick(() => updateDialogOpen.Set(true))
            | new Button("Split").Icon(Icons.Scissors).Outline().OnClick(() => splitDialogOpen.Set(true))
            | new Button("Expand").Icon(Icons.UnfoldVertical).Outline().OnClick(() =>
            {
                var planPath = Path.Combine(_planService.PlansDirectory, _selectedPlan.FileName);
                _taskService.StartTask("ExpandPlan", planPath);
                _refreshPlans();
            })
            | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => deleteDialogOpen.Set(true))
            | new Button("Create Issue").Icon(Icons.Github).Outline().OnClick(() => createIssueDialogOpen.Set(true))
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Spacer().Width(Size.Grow())
            | new Button("Download").Icon(Icons.Download).Outline().Url(downloadUrl.Value ?? "")
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy).OnSelect(() =>
                {
                    var planPath = Path.Combine(_planService.PlansDirectory, _selectedPlan.FileName);
                    copyToClipboard(planPath);
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
            new UpdatePlanDialog(updateDialogOpen, updateText, _selectedPlan, _taskService, _planService.PlansDirectory),
            new SplitPlanDialog(splitDialogOpen, splitText, _selectedPlan, _taskService, _planService.PlansDirectory),
            new DeletePlanDialog(deleteDialogOpen, _selectedPlan, _planService, _refreshPlans),
            new CreateIssueDialog(createIssueDialogOpen, selectedRepoState, issueAssigneeState, issueLabelsState, _selectedPlan)
        };

        return new Fragment(elements.ToArray());
    }

    private void GoToNext()
    {
        if (_allPlans.Count == 0) return;
        var currentIndex = _allPlans.FindIndex(p => p.FileName == _selectedPlan?.FileName);
        var nextIndex = (currentIndex + 1) % _allPlans.Count;
        _selectedPlanState.Set(_allPlans[nextIndex]);
    }

    private void GoToPrevious()
    {
        if (_allPlans.Count == 0) return;
        var currentIndex = _allPlans.FindIndex(p => p.FileName == _selectedPlan?.FileName);
        var prevIndex = (currentIndex - 1 + _allPlans.Count) % _allPlans.Count;
        _selectedPlanState.Set(_allPlans[prevIndex]);
    }
}
