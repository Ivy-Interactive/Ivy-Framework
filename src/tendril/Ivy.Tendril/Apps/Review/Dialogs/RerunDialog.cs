using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging;

namespace Ivy.Tendril.Apps.Review.Dialogs;

public class RerunDialog(
    IState<bool> dialogOpen,
    PlanFile selectedPlan,
    IJobService jobService,
    IPlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly PlanFile _selectedPlan = selectedPlan;

    public override object? Build()
    {
        var logger = UseService<ILogger<RerunDialog>>();

        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader($"Rerun Plan #{_selectedPlan.Id}"),
            new DialogBody(
                Text.P("How would you like to rerun this plan?")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => _dialogOpen.Set(false)),
                new Button("Rerun from Scratch").Warning().OnClick(() =>
                {
                    _dialogOpen.Set(false);
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                    _refreshPlans();

                    var folderPath = _selectedPlan.FolderPath;
                    Task.Run(() =>
                    {
                        CleanPlanState(folderPath, logger);
                        _jobService.StartJob("ExecutePlan", folderPath, "-Note",
                            "User requested a clean rerun. All artifacts, logs, and worktrees have been cleaned. Execute this plan from scratch.");
                    });
                }),
                new Button("Rerun with Current").Primary().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                {
                    _dialogOpen.Set(false);
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                    _jobService.StartJob("ExecutePlan", _selectedPlan.FolderPath, "-Note",
                        "User requested you to execute this plan another time. Go through all code, verifications and artifacts one more time.");
                    _refreshPlans();
                })
            )
        ).Width(Size.Rem(40));
    }

    internal static void CleanPlanState(string planFolderPath, ILogger? logger = null)
    {
        var artifactsDir = Path.Combine(planFolderPath, "artifacts");
        if (Directory.Exists(artifactsDir))
        {
            logger?.LogInformation("Cleaning artifacts directory: {Path}", artifactsDir);
            WorktreeCleanupService.ForceDeleteDirectory(artifactsDir, logger);
        }

        var logsDir = Path.Combine(planFolderPath, "logs");
        if (Directory.Exists(logsDir))
        {
            logger?.LogInformation("Cleaning logs directory: {Path}", logsDir);
            WorktreeCleanupService.ForceDeleteDirectory(logsDir, logger);
        }

        PlanReaderService.RemoveWorktrees(planFolderPath, logger);
    }
}
