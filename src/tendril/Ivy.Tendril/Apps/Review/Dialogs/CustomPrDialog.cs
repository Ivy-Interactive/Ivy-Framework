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
    QueryResult<string[]> assigneesQuery,
    IState<string?> assigneesError) : ViewBase
{
    private readonly IState<string?> _assigneesError = assigneesError;
    private readonly QueryResult<string[]> _assigneesQuery = assigneesQuery;
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly PlanFile _selectedPlan = selectedPlan;

    public override object? Build()
    {
        var customPrMerge = UseState(false);
        var customPrDeleteBranch = UseState(false);
        var customPrIncludeArtifacts = UseState(false);
        var customPrAssignee = UseState<string?>(null);
        var customPrComment = UseState("");

        UseEffect(() =>
        {
            if (!customPrMerge.Value) customPrDeleteBranch.Set(false);
        }, customPrMerge);

        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ =>
            {
                customPrMerge.Set(true);
                customPrDeleteBranch.Set(true);
                customPrIncludeArtifacts.Set(true);
                customPrAssignee.Set(null);
                customPrComment.Set("");
                _dialogOpen.Set(false);
            },
            new DialogHeader($"Custom PR for #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | customPrMerge.ToBoolInput("Merge").AutoFocus()
                | customPrDeleteBranch.ToBoolInput("Delete Branch")
                    .Disabled(!customPrMerge.Value)
                | customPrIncludeArtifacts.ToBoolInput("Include Artifacts")
                | customPrAssignee.ToSelectInput((_assigneesQuery.Value ?? Array.Empty<string>()).ToOptions())
                    .Nullable().WithField().Label("Assignee")
                | (_assigneesError.Value is { } err
                    ? Text.Danger(err).Small()
                    : null)
                | customPrComment.ToTextareaInput("Comment").Rows(3)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() =>
                {
                    customPrMerge.Set(true);
                    customPrDeleteBranch.Set(true);
                    customPrIncludeArtifacts.Set(true);
                    customPrAssignee.Set(null);
                    customPrComment.Set("");
                    _dialogOpen.Set(false);
                }),
                new Button("Create PR").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    var options = new Dictionary<string, object>
                    {
                        ["merge"] = customPrMerge.Value,
                        ["deleteBranch"] = customPrDeleteBranch.Value && customPrMerge.Value,
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
                    customPrMerge.Set(true);
                    customPrDeleteBranch.Set(true);
                    customPrIncludeArtifacts.Set(true);
                    customPrAssignee.Set(null);
                    customPrComment.Set("");
                    _dialogOpen.Set(false);
                }).WithConfetti(AnimationTrigger.Click)
            )
        ).Width(Size.Rem(30));
    }
}