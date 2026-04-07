using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Apps.Review.Dialogs;

public class CustomPrDialog(
    IState<bool> dialogOpen,
    PlanFile selectedPlan,
    IJobService jobService,
    IPlanReaderService planService,
    Action refreshPlans,
    QueryResult<string[]> assigneesQuery) : ViewBase
{
    private readonly QueryResult<string[]> _assigneesQuery = assigneesQuery;
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly PlanFile _selectedPlan = selectedPlan;

    public override object? Build()
    {
        var customPrApprove = UseState(true);
        var customPrMerge = UseState(true);
        var customPrDeleteBranch = UseState(true);
        var customPrIncludeArtifacts = UseState(true);
        var customPrAssignee = UseState<string?>(null);
        var customPrComment = UseState("");

        UseEffect(() =>
        {
            if (!customPrApprove.Value)
            {
                customPrMerge.Set(false);
                customPrDeleteBranch.Set(false);
            }
        }, customPrApprove);

        UseEffect(() =>
        {
            if (!customPrMerge.Value) customPrDeleteBranch.Set(false);
        }, customPrMerge);

        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader($"Custom PR for #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | customPrApprove.ToBoolInput("Approve").AutoFocus()
                | customPrMerge.ToBoolInput("Merge").Disabled(!customPrApprove.Value)
                | customPrDeleteBranch.ToBoolInput("Delete Branch")
                    .Disabled(!customPrMerge.Value || !customPrApprove.Value)
                | customPrIncludeArtifacts.ToBoolInput("Include Artifacts")
                | customPrAssignee.ToSelectInput((_assigneesQuery.Value ?? Array.Empty<string>()).ToOptions())
                    .Nullable().WithField().Label("Assignee")
                | customPrComment.ToTextareaInput("Comment").Rows(3)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => _dialogOpen.Set(false)),
                new Button("Create PR").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    var options = new Dictionary<string, object>
                    {
                        ["approve"] = customPrApprove.Value,
                        ["merge"] = customPrMerge.Value && customPrApprove.Value,
                        ["deleteBranch"] = customPrDeleteBranch.Value && customPrMerge.Value && customPrApprove.Value,
                        ["includeArtifacts"] = customPrIncludeArtifacts.Value,
                        ["assignee"] = customPrAssignee.Value ?? "",
                        ["comment"] = customPrComment.Value
                    };
                    var serializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    var optionsPath = Path.Combine(_selectedPlan.FolderPath, ".custom-pr-options.yaml");
                    FileHelper.WriteAllText(optionsPath, serializer.Serialize(options));
                    _jobService.StartJob("MakePr", _selectedPlan.FolderPath);
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                }).WithConfetti(AnimationTrigger.Click)
            )
        ).Width(Size.Rem(30));
    }
}