using Ivy.Tendril.Apps.Setup.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup;

public class PromptwaresSetupView : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editKey = UseState<string?>("__closed__");

        var promptwares = config.Settings.Promptwares;

        var rows = promptwares.Select(kvp => new PromptwareRow(
            kvp.Key,
            kvp.Value.Model,
            kvp.Value.Effort,
            string.Join(", ", kvp.Value.AllowedTools)
        )).ToList();

        var table = new TableBuilder<PromptwareRow>(rows)
            .Header(t => t.Name, "")
            .Builder(t => t.Name, f => f.Func<PromptwareRow, string>(name =>
                Layout.Horizontal().Gap(1)
                | new Button().Icon(Icons.Pencil).Outline().Small().Tooltip("Edit this promptware").OnClick(() =>
                {
                    editKey.Set(name);
                })
                | new Button().Icon(Icons.Trash).Outline().Small().Tooltip("Delete this promptware").OnClick(() =>
                {
                    promptwares.Remove(name);
                    config.SaveSettings();
                    client.Toast($"Promptware '{name}' deleted", "Deleted");
                    refreshToken.Refresh();
                })
            ));

        return Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
               | Text.Block("Promptware Configuration").Bold()
               | Text.Block("Configure model, effort level, and tool permissions for each promptware.")
                   .Muted().Small()
               | table
               | new Button("Add Promptware").Icon(Icons.Plus).Outline().OnClick(() =>
               {
                   editKey.Set(null);
               })
               | new EditPromptwareDialog(editKey, promptwares, config, client, refreshToken);
    }

    private record PromptwareRow(string Name, string Model, string Effort, string AllowedTools);
}
