using System.Drawing;
using Ivy;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public class ContentView : ViewBase
{
    private readonly PlanFile? _selectedPlan;
    private readonly List<PlanFile> _allPlans;
    private readonly IState<PlanFile?> _selectedPlanState;
    private readonly PlanReaderService _planService;
    private readonly Action _refreshPlans;

    public ContentView(
        PlanFile? selectedPlan,
        List<PlanFile> allPlans,
        IState<PlanFile?> selectedPlanState,
        PlanReaderService planService,
        Action refreshPlans)
    {
        _selectedPlan = selectedPlan;
        _allPlans = allPlans;
        _selectedPlanState = selectedPlanState;
        _planService = planService;
        _refreshPlans = refreshPlans;
    }

    public override object? Build()
    {
        var updateDialogOpen = UseState(false);
        var splitDialogOpen = UseState(false);
        var expandDialogOpen = UseState(false);
        var skipDialogOpen = UseState(false);
        var createIssueDialogOpen = UseState(false);
        var selectedRepoState = UseState<string?>(null);
        var issueAssigneeState = UseState<string?>(null);
        var issueLabelsState = UseState<string[]>(Array.Empty<string>());

        var updateText = UseState("");
        var splitText = UseState("");
        var expandText = UseState("");

        var isEditing = UseState(false);
        var editContent = UseState("");

        var lastPlanId = UseState(_selectedPlan?.Id ?? -1);
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
            | new Button(isEditing.Value ? "Preview" : "Edit")
                .Icon(isEditing.Value ? Icons.Eye : Icons.Pencil)
                .Outline()
                .OnClick(() =>
                {
                    if (isEditing.Value)
                    {
                        _planService.SavePlan(_selectedPlan.FileName, editContent.Value);
                        _refreshPlans();
                    }
                    else
                    {
                        editContent.Set(_planService.ReadRawPlan(_selectedPlan.FileName));
                    }
                    isEditing.Set(!isEditing.Value);
                })
            | new Spacer().Width(Size.Grow())
            | Text.Muted($"{currentIndex + 1} / {_allPlans.Count} plans");

        var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200)));

        if (isEditing.Value)
        {
            scrollableContent |= editContent.ToCodeInput()
                .Language(Languages.Markdown)
                .Height(Size.Full())
                .OnBlur(() =>
                {
                    _planService.SavePlan(_selectedPlan.FileName, editContent.Value);
                    _refreshPlans();
                });
        }
        else
        {
            scrollableContent |= new Markdown(_selectedPlan.Content);
        }

        var actionBar = Layout.Horizontal().Align(Align.Center).Gap(2).Padding(1)
            | new Button("Update").Icon(Icons.Pencil).Outline().OnClick(() => updateDialogOpen.Set(true))
            | new Button("Split").Icon(Icons.GitBranch).Outline().OnClick(() => splitDialogOpen.Set(true))
            | new Button("Expand").Icon(Icons.Maximize).Outline().OnClick(() => expandDialogOpen.Set(true))
            | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => skipDialogOpen.Set(true))
            | new Button("Create Issue").Icon(Icons.Github).Outline().OnClick(() => createIssueDialogOpen.Set(true))
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious())
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext())
            | new Button("Approve").Icon(Icons.Check).Primary().OnClick(() =>
            {
                _planService.ApprovePlan(_selectedPlan.FileName);
                _refreshPlans();
            });

        var mainContent = Layout.Vertical()
            | scrollableContent;

        var mainLayout = new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: mainContent
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full());

        var elements = new List<object>
        {
            mainLayout,
            new UpdatePlanDialog(updateDialogOpen, updateText, _selectedPlan),
            new SplitPlanDialog(splitDialogOpen, splitText, _selectedPlan),
            new ExpandPlanDialog(expandDialogOpen, expandText, _selectedPlan),
            new SkipPlanDialog(skipDialogOpen, _selectedPlan, _planService, _refreshPlans),
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
