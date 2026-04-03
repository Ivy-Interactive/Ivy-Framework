using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class LevelsSettingsView : ViewBase
{
    public override object? Build()
    {
        var config = UseService<ConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editIndex = UseState<int?>(-1);
        var editName = UseState("");
        var editBadge = UseState("Outline");

        var badgeOptions = Enum.GetNames<BadgeVariant>().ToList();

        var levels = config.Settings.Levels;

        var table = new Table(
            new TableRow(
                new TableCell("Name").IsHeader(),
                new TableCell("Badge").IsHeader(),
                new TableCell("Actions").IsHeader()
            )
            { IsHeader = true }
        );

        for (var i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            var idx = i;
            table |= new TableRow(
                new TableCell(Text.Block(level.Name)),
                new TableCell(new Badge(level.Badge).Variant(
                    Enum.TryParse<BadgeVariant>(level.Badge, out var v) ? v : BadgeVariant.Outline)),
                new TableCell(Layout.Horizontal().Gap(1)
                    | new Button("Edit").Outline().Small().OnClick(() =>
                    {
                        editIndex.Set(idx);
                        editName.Set(level.Name);
                        editBadge.Set(level.Badge);
                    })
                    | new Button("Delete").Outline().Small().OnClick(() =>
                    {
                        levels.RemoveAt(idx);
                        config.SaveSettings();
                        client.Toast($"Level '{level.Name}' deleted", "Deleted");
                        refreshToken.Refresh();
                    })
                    | (idx > 0
                        ? (object)new Button().Icon(Icons.ChevronUp).Ghost().Small().OnClick(() =>
                        {
                            (levels[idx], levels[idx - 1]) = (levels[idx - 1], levels[idx]);
                            config.SaveSettings();
                            refreshToken.Refresh();
                        })
                        : new Spacer().Width(Size.Units(0)))
                    | (idx < levels.Count - 1
                        ? (object)new Button().Icon(Icons.ChevronDown).Ghost().Small().OnClick(() =>
                        {
                            (levels[idx], levels[idx + 1]) = (levels[idx + 1], levels[idx]);
                            config.SaveSettings();
                            refreshToken.Refresh();
                        })
                        : new Spacer().Width(Size.Units(0)))
                )
            );
        }

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
                _ =>
                {
                    editIndex.Set(-1);
                },
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
}
