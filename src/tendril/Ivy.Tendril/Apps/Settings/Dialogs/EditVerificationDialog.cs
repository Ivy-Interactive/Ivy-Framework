using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings.Dialogs;

public class EditVerificationDialog(
    IState<int?> editIndex,
    List<VerificationConfig> verifications,
    IConfigService config,
    IClientProvider client,
    RefreshToken refreshToken) : ViewBase
{
    private readonly IState<int?> _editIndex = editIndex;
    private readonly List<VerificationConfig> _verifications = verifications;
    private readonly IConfigService _config = config;
    private readonly IClientProvider _client = client;
    private readonly RefreshToken _refreshToken = refreshToken;

    public override object? Build()
    {
        var editName = UseState("");
        var editPrompt = UseState("");

        UseEffect(() =>
        {
            if (_editIndex.Value == null)
            {
                editName.Set("");
                editPrompt.Set("");
            }
            else if (_editIndex.Value >= 0)
            {
                editName.Set(_verifications[_editIndex.Value.Value].Name);
                editPrompt.Set(_verifications[_editIndex.Value.Value].Prompt);
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
                | editPrompt.ToTextareaInput("Verification prompt...").Rows(8).WithField().Label("Prompt")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _editIndex.Set(-1)),
                new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                {
                    if (string.IsNullOrWhiteSpace(editName.Value)) return;
                    if (isNew)
                    {
                        _verifications.Add(new VerificationConfig
                        {
                            Name = editName.Value,
                            Prompt = editPrompt.Value
                        });
                    }
                    else
                    {
                        _verifications[_editIndex.Value!.Value].Name = editName.Value;
                        _verifications[_editIndex.Value!.Value].Prompt = editPrompt.Value;
                    }

                    _config.SaveSettings();
                    _editIndex.Set(-1);
                    _refreshToken.Refresh();
                    _client.Toast("Verification saved", "Saved");
                })
            )
        ).Width(Size.Rem(35));
    }
}
