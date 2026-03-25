using Ivy;

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
        var tokenStrategyState = UseState<string?>(null);
        var securityFeaturesState = UseState<string[]>(Array.Empty<string>());

        var updateDialogOpen = UseState(false);
        var splitDialogOpen = UseState(false);
        var expandDialogOpen = UseState(false);
        var skipDialogOpen = UseState(false);
        var approveDialogOpen = UseState(false);
        var createIssueDialogOpen = UseState(false);
        var selectedRepoState = UseState<string?>(null);
        var issueAssigneeState = UseState<string?>(null);

        var updateText = UseState("");
        var splitText = UseState("");
        var expandText = UseState("");

        if (_selectedPlan is null)
        {
            return Layout.Vertical().Align(Align.Center).Height(Size.Full())
                | Text.Muted("Select a plan from the sidebar");
        }

        var tokenStrategyOptions = new[] { "In-Memory Cache", "Encrypted Database", "Secure Cookie" };
        var securityFeaturesOptions = new[] { "Rate Limiting", "CORS Policy", "Input Validation", "Audit Logging" };
        var assignees = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve" };

        var currentIndex = _allPlans.FindIndex(p => p.FileName == _selectedPlan.FileName);

        var header = Layout.Horizontal().Align(Align.Left).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
            | new Badge(_selectedPlan.Queue).Variant(BadgeVariant.Info).Small()
            | new Badge(_selectedPlan.Level).Variant(BadgeVariant.Warning).Small()
            | new Spacer()
            | Text.Muted($"{currentIndex + 1} / {_allPlans.Count} plans").Small();

        var scrollableContent = Layout.Vertical()
            | new Markdown(_selectedPlan.Content);

        var aiPanel = new Expandable("AI Detected Options",
            Layout.Vertical().Gap(2).Padding(4).Background(Colors.Info).BorderRadius(BorderRadius.Rounded)
                | tokenStrategyState.ToSelectInput(tokenStrategyOptions.ToOptions())
                    .Variant(SelectInputVariant.Radio)
                    .WithField().Label("Token Storage Strategy")
                | securityFeaturesState.ToSelectInput(securityFeaturesOptions.ToOptions())
                    .Variant(SelectInputVariant.List)
                    .WithField().Label("Additional Security Features")
        ).Icon(Icons.Sparkles);

        var actionBar = Layout.Horizontal().Align(Align.Center).Gap(2).Padding(2).BorderRadius(BorderRadius.Rounded)
            | new Button("Update").Icon(Icons.Pencil).Outline().OnClick(() => updateDialogOpen.Set(true))
            | new Button("Split").Icon(Icons.GitBranch).Outline().OnClick(() => splitDialogOpen.Set(true))
            | new Button("Expand").Icon(Icons.Maximize).Outline().OnClick(() => expandDialogOpen.Set(true))
            | new Button("Skip").Icon(Icons.SkipForward).Outline().OnClick(() => skipDialogOpen.Set(true))
            | new Button("Create Issue").Icon(Icons.Github).Outline().OnClick(() => createIssueDialogOpen.Set(true))
            | new Spacer()
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious())
            | new Button("Next").Icon(Icons.ChevronRight).Outline().OnClick(() => GoToNext())
            | new Button("Approve").Icon(Icons.Check).Primary().OnClick(() => approveDialogOpen.Set(true));

        var mainContent = Layout.Vertical()
            | scrollableContent
            | aiPanel;

        var mainLayout = new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: mainContent
            )
        ).Height(Size.Full()).Width(Size.Full().Max(Size.Units(300)));

        var elements = new List<object> { mainLayout };

        if (updateDialogOpen.Value)
        {
            elements.Add(new Dialog(
                _ => updateDialogOpen.Set(false),
                new DialogHeader($"Update Plan #{_selectedPlan.Id}"),
                new DialogBody(
                    Layout.Vertical()
                        | Text.P("Provide instructions for updating this plan.")
                        | updateText.ToTextareaInput("Enter update instructions...").Rows(6)
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => updateDialogOpen.Set(false)),
                    new Button("Submit Update").Primary().OnClick(() => updateDialogOpen.Set(false))
                )
            ).Width(Size.Rem(30)));
        }

        if (splitDialogOpen.Value)
        {
            elements.Add(new Dialog(
                _ => splitDialogOpen.Set(false),
                new DialogHeader($"Split Plan #{_selectedPlan.Id}"),
                new DialogBody(
                    Layout.Vertical()
                        | Text.P("Describe how to split this plan into multiple plans.")
                        | splitText.ToTextareaInput("Enter split instructions...").Rows(6)
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => splitDialogOpen.Set(false)),
                    new Button("Split Plan").Primary().OnClick(() => splitDialogOpen.Set(false))
                )
            ).Width(Size.Rem(30)));
        }

        if (expandDialogOpen.Value)
        {
            elements.Add(new Dialog(
                _ => expandDialogOpen.Set(false),
                new DialogHeader($"Expand Plan #{_selectedPlan.Id}"),
                new DialogBody(
                    Layout.Vertical()
                        | Text.P("Provide instructions for expanding this plan.")
                        | expandText.ToTextareaInput("Enter expansion instructions...").Rows(6)
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => expandDialogOpen.Set(false)),
                    new Button("Expand Plan").Primary().OnClick(() => expandDialogOpen.Set(false))
                )
            ).Width(Size.Rem(30)));
        }

        if (skipDialogOpen.Value)
        {
            elements.Add(new Dialog(
                _ => skipDialogOpen.Set(false),
                new DialogHeader("Skip Plan"),
                new DialogBody(
                    Text.P($"Move plan #{_selectedPlan.Id} to skipped/ directory?")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => skipDialogOpen.Set(false)),
                    new Button("Skip").Primary().OnClick(() =>
                    {
                        _planService.SkipPlan(_selectedPlan.FileName);
                        _refreshPlans();
                        skipDialogOpen.Set(false);
                    })
                )
            ).Width(Size.Rem(30)));
        }

        if (approveDialogOpen.Value)
        {
            elements.Add(new Dialog(
                _ => approveDialogOpen.Set(false),
                new DialogHeader("Approve Plan"),
                new DialogBody(
                    Text.P($"Approve plan #{_selectedPlan.Id} and move to approved/?")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => approveDialogOpen.Set(false)),
                    new Button("Approve").Primary().OnClick(() =>
                    {
                        _planService.ApprovePlan(_selectedPlan.FileName);
                        _refreshPlans();
                        approveDialogOpen.Set(false);
                    })
                )
            ).Width(Size.Rem(30)));
        }

        if (createIssueDialogOpen.Value)
        {
            var repositoryOptions = new[] { "Ivy-Framework", "Ivy-Agent", "Ivy-Mcp", "Ivy" };

            elements.Add(new Dialog(
                _ => createIssueDialogOpen.Set(false),
                new DialogHeader("Create GitHub Issue"),
                new DialogBody(
                    Layout.Vertical().Gap(3)
                        | Text.P($"Create a GitHub issue for plan #{_selectedPlan.Id}.")
                        | Text.Muted("Labels will be added automatically based on the plan metadata.")
                        | selectedRepoState.ToSelectInput(repositoryOptions.ToOptions())
                            .WithField().Label("Repository")
                        | issueAssigneeState.ToSelectInput(assignees.ToOptions())
                            .Nullable().WithField().Label("Assignee (optional)")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => createIssueDialogOpen.Set(false)),
                    new Button("Create Issue").Primary().OnClick(() => createIssueDialogOpen.Set(false))
                )
            ).Width(Size.Rem(30)));
        }

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
