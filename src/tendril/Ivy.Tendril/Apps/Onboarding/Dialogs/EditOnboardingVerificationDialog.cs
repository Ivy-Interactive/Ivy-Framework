namespace Ivy.Tendril.Apps.Onboarding.Dialogs;

internal class EditOnboardingVerificationDialog(
    IState<int?> editIndex,
    IState<List<VerificationEntry>> verifications) : ViewBase
{
    private readonly IState<int?> _editIndex = editIndex;
    private readonly IState<List<VerificationEntry>> _verifications = verifications;

    public override object? Build()
    {
        var editName = UseState("");
        var editPrompt = UseState("");
        var editRequired = UseState(false);

        UseEffect(() =>
        {
            if (_editIndex.Value == null)
            {
                editName.Set("");
                editPrompt.Set("");
                editRequired.Set(false);
            }
            else if (_editIndex.Value >= 0)
            {
                editName.Set(_verifications.Value[_editIndex.Value.Value].Name);
                editPrompt.Set(_verifications.Value[_editIndex.Value.Value].Prompt);
                editRequired.Set(_verifications.Value[_editIndex.Value.Value].Required);
            }
        }, _editIndex);

        if (_editIndex.Value == -1) return null;

        var isNew = _editIndex.Value == null;

        return new Dialog(
            _ => _editIndex.Set(-1),
            new DialogHeader(isNew ? "Add Verification" : "Edit Verification"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | editName.ToTextInput("Verification name...").WithField().Label("Name")
                | editPrompt.ToTextareaInput("Verification prompt...").Rows(6).WithField().Label("Prompt")
                | editRequired.ToBoolInput("Required")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _editIndex.Set(-1)),
                new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                {
                    if (string.IsNullOrWhiteSpace(editName.Value)) return;
                    var list = new List<VerificationEntry>(_verifications.Value);
                    if (isNew)
                        list.Add(new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value));
                    else
                        list[_editIndex.Value!.Value] =
                            new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value);
                    _verifications.Set(list);
                    _editIndex.Set(-1);
                })
            )
        ).Width(Size.Rem(35));
    }
}
