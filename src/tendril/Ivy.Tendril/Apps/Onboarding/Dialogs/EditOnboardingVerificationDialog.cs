namespace Ivy.Tendril.Apps.Onboarding.Dialogs;

internal class EditOnboardingVerificationDialog(
    IState<int?> editIndex,
    IState<List<VerificationEntry>> verifications) : ViewBase
{
    public override object? Build()
    {
        var editName = UseState("");
        var editPrompt = UseState("");
        var editRequired = UseState(false);

        UseEffect(() =>
        {
            if (editIndex.Value == null)
            {
                editName.Set("");
                editPrompt.Set("");
                editRequired.Set(false);
            }
            else if (editIndex.Value >= 0)
            {
                editName.Set(verifications.Value[editIndex.Value.Value].Name);
                editPrompt.Set(verifications.Value[editIndex.Value.Value].Prompt);
                editRequired.Set(verifications.Value[editIndex.Value.Value].Required);
            }
        }, editIndex);

        if (editIndex.Value == -1) return null;

        var isNew = editIndex.Value == null;

        return new Dialog(
            _ => editIndex.Set(-1),
            new DialogHeader(isNew ? "Add Verification" : "Edit Verification"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | editName.ToTextInput("Verification name...").WithField().Label("Name")
                | editPrompt.ToTextareaInput("Verification prompt...").Rows(6).WithField().Label("Prompt")
                | editRequired.ToBoolInput("Required")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => editIndex.Set(-1)),
                new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                {
                    if (string.IsNullOrWhiteSpace(editName.Value)) return;
                    var list = new List<VerificationEntry>(verifications.Value);
                    if (isNew)
                        list.Add(new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value));
                    else
                        list[editIndex.Value!.Value] =
                            new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value);
                    verifications.Set(list);
                    editIndex.Set(-1);
                })
            )
        ).Width(Size.Rem(35));
    }
}
