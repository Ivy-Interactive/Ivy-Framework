using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings.Dialogs;

public class EditLevelDialog(
    IState<int?> editIndex,
    List<LevelConfig> levels,
    IConfigService config,
    IClientProvider client,
    RefreshToken refreshToken) : ViewBase
{
    private readonly IState<int?> _editIndex = editIndex;
    private readonly List<LevelConfig> _levels = levels;
    private readonly IConfigService _config = config;
    private readonly IClientProvider _client = client;
    private readonly RefreshToken _refreshToken = refreshToken;

    public override object? Build()
    {
        var editName = UseState("");
        var editBadge = UseState("Outline");

        UseEffect(() =>
        {
            if (_editIndex.Value == null)
            {
                editName.Set("");
                editBadge.Set("Outline");
            }
            else if (_editIndex.Value >= 0)
            {
                editName.Set(_levels[_editIndex.Value.Value].Name);
                editBadge.Set(_levels[_editIndex.Value.Value].Badge);
            }
        }, _editIndex);

        if (_editIndex.Value == -1) return null;

        var badgeOptions = Enum.GetNames<BadgeVariant>().ToList();
        var isNew = _editIndex.Value == null;

        return new Dialog(
            _ => { _editIndex.Set(-1); },
            new DialogHeader(isNew ? "Add Level" : "Edit Level"),
            new DialogBody(
                Layout.Vertical().Gap(2)
                | editName.ToTextInput("Level name...").WithField().Label("Name")
                | editBadge.ToSelectInput(badgeOptions).WithField().Label("Badge Variant")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _editIndex.Set(-1)),
                new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                {
                    if (string.IsNullOrWhiteSpace(editName.Value)) return;
                    if (isNew)
                    {
                        _levels.Add(new LevelConfig { Name = editName.Value, Badge = editBadge.Value });
                    }
                    else
                    {
                        var level = _levels[_editIndex.Value!.Value];
                        level.Name = editName.Value;
                        level.Badge = editBadge.Value;
                    }

                    _config.SaveSettings();
                    _editIndex.Set(-1);
                    _refreshToken.Refresh();
                    _client.Toast("Level saved", "Saved");
                })
            )
        ).Width(Size.Rem(25));
    }
}
