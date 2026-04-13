using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations.Dialogs;

public class DeclineRecommendationDialog(
    IState<bool> showDialog,
    Recommendation? recommendation,
    IPlanReaderService planService,
    Action refresh,
    Action goToNext) : ViewBase
{
    private readonly IState<bool> _showDialog = showDialog;
    private readonly Recommendation? _recommendation = recommendation;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refresh = refresh;
    private readonly Action _goToNext = goToNext;

    public override object? Build()
    {
        var declineReason = UseState<string?>("");

        if (!_showDialog.Value || _recommendation is null) return null;

        return new Dialog(
            _ =>
            {
                declineReason.Set("");
                _showDialog.Set(false);
            },
            new DialogHeader("Decline Recommendation"),
            new DialogBody(
                Layout.Vertical()
                | Text.P("Optionally provide a reason for declining this recommendation.")
                | declineReason.ToTextareaInput("Enter reason (optional)...").Rows(4)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() =>
                {
                    declineReason.Set("");
                    _showDialog.Set(false);
                }),
                new Button("Decline").Destructive().ShortcutKey("Enter").OnClick(() =>
                {
                    _planService.UpdateRecommendationState(
                        _recommendation.PlanFolderName,
                        _recommendation.Title,
                        "Declined",
                        declineReason.Value
                    );
                    _refresh();
                    _showDialog.Set(false);
                    declineReason.Set("");
                    _goToNext();
                })
            )
        ).Width(Size.Rem(40));
    }
}
