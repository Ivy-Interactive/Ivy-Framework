using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings.Dialogs;

public class DeleteProjectDialog(
    IState<int?> deleteIndex,
    List<ProjectConfig> projects,
    IConfigService config,
    IClientProvider client,
    RefreshToken refreshToken) : ViewBase
{
    private readonly IState<int?> _deleteIndex = deleteIndex;
    private readonly List<ProjectConfig> _projects = projects;
    private readonly IConfigService _config = config;
    private readonly IClientProvider _client = client;
    private readonly RefreshToken _refreshToken = refreshToken;

    public override object? Build()
    {
        if (_deleteIndex.Value is not { } di || di < 0 || di >= _projects.Count) return null;

        var projectName = _projects[di].Name;

        return new Dialog(
            _ => _deleteIndex.Set(-1),
            new DialogHeader("Delete Project"),
            new DialogBody(
                Text.P($"Are you sure you want to delete the project '{projectName}'? This cannot be undone.")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _deleteIndex.Set(-1)),
                new Button("Delete").Variant(ButtonVariant.Destructive).OnClick(() =>
                {
                    _projects.RemoveAt(di);
                    _config.SaveSettings();
                    _deleteIndex.Set(-1);
                    _refreshToken.Refresh();
                    _client.Toast($"Project '{projectName}' deleted", "Deleted");
                })
            )
        ).Width(Size.Rem(25));
    }
}
