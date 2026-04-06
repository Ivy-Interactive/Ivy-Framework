using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Review.Dialogs;

public class SuggestChangesDialog(
    IState<bool> dialogOpen,
    IState<string> suggestText,
    PlanFile selectedPlan,
    IJobService jobService,
    IPlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IState<string> _suggestText = suggestText;
    private readonly PlanFile _selectedPlan = selectedPlan;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => { _suggestText.Set(""); _dialogOpen.Set(false); },
            new DialogHeader($"Suggest Changes for Plan #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical()
                    | Text.P("Provide suggestions for changes to this plan before creating the PR.")
                    | _suggestText.ToTextareaInput("Enter your suggestions...").Rows(6).AutoFocus()
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => { _suggestText.Set(""); _dialogOpen.Set(false); }),
                new Button("Submit Suggestions").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(_suggestText.Value))
                    {
                        var currentContent = _planService.ReadLatestRevision(_selectedPlan.FolderName);
                        var comments = string.Join("\n", _suggestText.Value
                            .Split('\n')
                            .Select(line => $">> {line}"));
                        _planService.SavePlan(_selectedPlan.FolderName, currentContent + "\n\n" + comments + "\n");
                    }
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Updating);
                    _jobService.StartJob("UpdatePlan", _selectedPlan.FolderPath);
                    _refreshPlans();
                    _suggestText.Set("");
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
