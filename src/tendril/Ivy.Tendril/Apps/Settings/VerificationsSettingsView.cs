using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class VerificationsSettingsView : ViewBase
{
    public override object? Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editIndex = UseState<int?>(-1);
        var editName = UseState("");
        var editPrompt = UseState("");

        var verifications = config.Settings.Verifications;

        var rows = verifications.Select((v, i) => new VerificationRow(i, v.Name, v.Prompt)).ToList();

        var table = new TableBuilder<VerificationRow>(rows)
            .Header(t => t.Index, "Actions")
            .Builder(t => t.Index, f => f.Func<VerificationRow, int>(idx =>
                Layout.Horizontal().Gap(1)
                    | new Button("Edit").Outline().Small().OnClick(() =>
                    {
                        editIndex.Set(idx);
                        editName.Set(verifications[idx].Name);
                        editPrompt.Set(verifications[idx].Prompt);
                    })
                    | new Button("Delete").Outline().Small().OnClick(() =>
                    {
                        var name = verifications[idx].Name;
                        verifications.RemoveAt(idx);
                        config.SaveSettings();
                        client.Toast($"Verification '{name}' deleted", "Deleted");
                        refreshToken.Refresh();
                    })
            ));

        var content = Layout.Vertical().Gap(4).Padding(4)
            | Text.Block("Verification Definitions").Bold()
            | table
            | new Button("Add Verification").Icon(Icons.Plus).Outline().OnClick(() =>
            {
                editIndex.Set(null);
                editName.Set("");
                editPrompt.Set("");
            });

        if (editIndex.Value != -1)
        {
            var isNew = editIndex.Value == null;
            content |= new Dialog(
                _ => editIndex.Set(-1),
                new DialogHeader(isNew ? "Add Verification" : "Edit Verification"),
                new DialogBody(
                    Layout.Vertical().Gap(2)
                        | editName.ToTextInput("Verification name...").WithField().Label("Name")
                        | editPrompt.ToTextareaInput("Verification prompt...").Rows(8).WithField().Label("Prompt")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => editIndex.Set(-1)),
                    new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                    {
                        if (string.IsNullOrWhiteSpace(editName.Value)) return;
                        if (isNew)
                        {
                            verifications.Add(new VerificationConfig
                            {
                                Name = editName.Value,
                                Prompt = editPrompt.Value
                            });
                        }
                        else
                        {
                            verifications[editIndex.Value!.Value].Name = editName.Value;
                            verifications[editIndex.Value!.Value].Prompt = editPrompt.Value;
                        }
                        config.SaveSettings();
                        editIndex.Set(-1);
                        refreshToken.Refresh();
                        client.Toast("Verification saved", "Saved");
                    })
                )
            ).Width(Size.Rem(35));
        }

        return content;
    }

    private record VerificationRow(int Index, string Name, string Prompt);
}
