using System.Drawing;
using Ivy;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Tendril.Apps.Icebox.Dialogs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox;

public class ContentView : ViewBase
{
    private readonly PlanFile? _selectedPlan;
    private readonly List<PlanFile> _allPlans;
    private readonly IState<PlanFile?> _selectedPlanState;
    private readonly PlanReaderService _planService;
    private readonly JobService _jobService;
    private readonly Action _refreshPlans;

    public ContentView(
        PlanFile? selectedPlan,
        List<PlanFile> allPlans,
        IState<PlanFile?> selectedPlanState,
        PlanReaderService planService,
        JobService jobService,
        Action refreshPlans)
    {
        _selectedPlan = selectedPlan;
        _allPlans = allPlans;
        _selectedPlanState = selectedPlanState;
        _planService = planService;
        _jobService = jobService;
        _refreshPlans = refreshPlans;
    }

    public override object? Build()
    {
        var downloadUrl = PlanDownloadHelper.UsePlanDownload(Context, _planService, _selectedPlan);
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();
        var deleteDialogOpen = UseState(false);

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
            | new Button("Delete").Icon(Icons.Trash).Outline().OnClick(() => deleteDialogOpen.Set(true))
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Button("Thaw").Icon(Icons.Flame).Primary().OnClick(() =>
            {
                _planService.ThawPlan(_selectedPlan.FileName);
                _refreshPlans();
            })
            | new Spacer().Width(Size.Grow())
            | new Button("Download").Icon(Icons.Download).Outline().Url(downloadUrl.Value ?? "")
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy).OnSelect(() =>
                {
                    var planPath = Path.Combine(_planService.PlansDirectory, _selectedPlan.FileName);
                    copyToClipboard(planPath);
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
            new Icebox.Dialogs.DeletePlanDialog(deleteDialogOpen, _selectedPlan, _planService, _refreshPlans)
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
