using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class LevelsSettingsView : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editIndex = UseState<int?>(-1);
        var editName = UseState("");
        var editBadge = UseState("Outline");

        var badgeOptions = Enum.GetNames<BadgeVariant>().ToList();

        // Use levels in config.yaml order (not alphabetically sorted).
        var levels = config.Settings.Levels;

        var rows = levels.Select((level, i) => new LevelRow(i, level.Name, level.Badge)).ToList();

        var table = new TableBuilder<LevelRow>(rows)
            .Builder(t => t.Badge, f => f.Func<LevelRow, string>(badge =>
                new Badge(badge).Variant(
                    Enum.TryParse<BadgeVariant>(badge, out var v) ? v : BadgeVariant.Outline
                )
            ))
            .Header(t => t.Index, "")
            .Builder(t => t.Index, f => f.Func<LevelRow, int>(idx =>
                Layout.Horizontal().Gap(1)
                | new Button().Icon(Icons.Pencil).Outline().Small().Tooltip("Edit this level").OnClick(() =>
                {
                    editIndex.Set(idx);
                    editName.Set(levels[idx].Name);
                    editBadge.Set(levels[idx].Badge);
                })
                | new Button().Icon(Icons.Trash).Outline().Small().Tooltip("Delete this level").OnClick(() =>
                {
                    var name = levels[idx].Name;
                    levels.RemoveAt(idx);
                    config.SaveSettings();
                    client.Toast($"Level '{name}' deleted", "Deleted");
                    refreshToken.Refresh();
                })
            ));

        var content = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
                      | Text.Block("Priority Levels").Bold()
                      | table
                      | new Button("Add Level").Icon(Icons.Plus).Outline().OnClick(() =>
                      {
                          editIndex.Set(null);
                          editName.Set("");
                          editBadge.Set("Outline");
                      });

        if (editIndex.Value != -1)
        {
            var isNew = editIndex.Value == null;
            content |= new Dialog(
                _ => { editIndex.Set(-1); },
                new DialogHeader(isNew ? "Add Level" : "Edit Level"),
                new DialogBody(
                    Layout.Vertical().Gap(2)
                    | editName.ToTextInput("Level name...").WithField().Label("Name")
                    | editBadge.ToSelectInput(badgeOptions).WithField().Label("Badge Variant")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => editIndex.Set(-1)),
                    new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                    {
                        if (string.IsNullOrWhiteSpace(editName.Value)) return;
                        if (isNew)
                        {
                            levels.Add(new LevelConfig { Name = editName.Value, Badge = editBadge.Value });
                        }
                        else
                        {
                            var level = levels[editIndex.Value!.Value];
                            level.Name = editName.Value;
                            level.Badge = editBadge.Value;
                        }

                        config.SaveSettings();
                        editIndex.Set(-1);
                        refreshToken.Refresh();
                        client.Toast("Level saved", "Saved");
                    })
                )
            ).Width(Size.Rem(25));
        }

        return content;
    }

    private record LevelRow(int Index, string Name, string Badge);
}
