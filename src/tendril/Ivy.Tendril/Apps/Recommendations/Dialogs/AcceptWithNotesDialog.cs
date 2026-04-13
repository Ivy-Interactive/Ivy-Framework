using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations.Dialogs;

public class AcceptWithNotesDialog(
    IState<bool> dialogOpen,
    Recommendation recommendation,
    Action<string> onAccept) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly Action<string> _onAccept = onAccept;
    private readonly Recommendation _recommendation = recommendation;

    public override object? Build()
    {
        var notesText = UseState("");

        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ =>
            {
                notesText.Set("");
                _dialogOpen.Set(false);
            },
            new DialogHeader("Accept with Notes"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | Text.Block("Add notes to include with this recommendation:").Muted()
                | new Markdown(_recommendation.Description)
                | notesText.ToTextareaInput("Enter your notes...").Rows(6).AutoFocus()
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() =>
                {
                    notesText.Set("");
                    _dialogOpen.Set(false);
                }),
                new Button("Accept").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    _onAccept(notesText.Value);
                    notesText.Set("");
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(40));
    }
}