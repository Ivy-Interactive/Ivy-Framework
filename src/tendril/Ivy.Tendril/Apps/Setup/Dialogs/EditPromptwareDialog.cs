using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup.Dialogs;

public class EditPromptwareDialog(
    IState<string?> editKey,
    Dictionary<string, PromptwareConfig> promptwares,
    IConfigService config,
    IClientProvider client,
    RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var editName = UseState("");
        var editProfile = UseState("");
        var editAllowedTools = UseState("");

        UseEffect(() =>
        {
            if (editKey.Value == null)
            {
                editName.Set("");
                editProfile.Set("");
                editAllowedTools.Set("");
            }
            else if (editKey.Value != "__closed__" && promptwares.ContainsKey(editKey.Value))
            {
                var pw = promptwares[editKey.Value];
                editName.Set(editKey.Value);
                editProfile.Set(pw.Profile);
                editAllowedTools.Set(string.Join(", ", pw.AllowedTools));
            }
        }, editKey);

        if (editKey.Value == "__closed__") return null;

        var isNew = editKey.Value == null;

        return new Dialog(
            _ => editKey.Set("__closed__"),
            new DialogHeader(isNew ? "Add Promptware" : "Edit Promptware"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | editName.ToTextInput("Promptware name (e.g. MakePlan)...").WithField().Label("Name")
                | editProfile.ToTextInput("Profile name (e.g. deep, balanced, quick)...").WithField().Label("Profile")
                | editAllowedTools.ToTextInput("Comma-separated tools (e.g. Read, Write, Edit)...").WithField()
                    .Label("Allowed Tools")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => editKey.Set("__closed__")),
                new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                {
                    if (string.IsNullOrWhiteSpace(editName.Value)) return;

                    var tools = editAllowedTools.Value
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();

                    var pwConfig = new PromptwareConfig
                    {
                        Profile = editProfile.Value,
                        AllowedTools = tools
                    };

                    // If renaming, remove old key
                    if (!isNew && editKey.Value != editName.Value)
                        promptwares.Remove(editKey.Value!);

                    promptwares[editName.Value] = pwConfig;
                    config.SaveSettings();
                    editKey.Set("__closed__");
                    refreshToken.Refresh();
                    client.Toast("Promptware saved", "Saved");
                })
            )
        ).Width(Size.Rem(35));
    }
}
