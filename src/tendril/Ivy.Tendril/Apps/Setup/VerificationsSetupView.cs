using Ivy.Tendril.Apps.Setup.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup;

public class VerificationsSetupView : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editIndex = UseState<int?>(-1);

        var verifications = config.Settings.Verifications;

        var rows = verifications.Select((v, i) => new VerificationRow(v.Name, v.Prompt, i)).ToList();

        var table = new TableBuilder<VerificationRow>(rows)
            .Header(t => t.Index, "")
            .Builder(t => t.Index, f => f.Func<VerificationRow, int>(idx =>
                Layout.Horizontal().Gap(1)
                | new Button().Icon(Icons.Pencil).Outline().Small().Tooltip("Edit this verification").OnClick(() =>
                {
                    editIndex.Set(idx);
                })
                | new Button().Icon(Icons.Trash).Outline().Small().Tooltip("Delete this verification").OnClick(() =>
                {
                    var name = verifications[idx].Name;
                    verifications.RemoveAt(idx);
                    config.SaveSettings();
                    client.Toast($"Verification '{name}' deleted", "Deleted");
                    refreshToken.Refresh();
                })
            ));

        return Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
               | Text.Block("Verification Definitions").Bold()
               | table
               | new Button("Add Verification").Icon(Icons.Plus).Outline().OnClick(() =>
               {
                   editIndex.Set(null);
               })
               | new EditVerificationDialog(editIndex, verifications, config, client, refreshToken);
    }

    private record VerificationRow(string Name, string Prompt, int Index);
}
