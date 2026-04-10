using Ivy.Tendril.Apps.Setup.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup;

public class ProjectsSetupView : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editIndex = UseState<int?>(-1);
        var deleteIndex = UseState<int?>(-1);

        var projects = config.Settings.Projects;
        var allVerifications = config.Settings.Verifications.Select(v => v.Name).ToList();

        var rows = projects.Select((p, i) => new ProjectRow(
            i, p.Name, p.Color, p.GetMeta("slackEmoji"), p.Repos.Count, p.Verifications.Count
        )).ToList();

        var table = new TableBuilder<ProjectRow>(rows)
            .Header(t => t.Index, "Actions")
            .Builder(t => t.Index, f => f.Func<ProjectRow, int>(idx =>
                Layout.Horizontal().Gap(1)
                | new Button().Icon(Icons.Pencil).Outline().Small().OnClick(() =>
                {
                    editIndex.Set(idx);
                })
                | new Button().Icon(Icons.Trash).Outline().Small().OnClick(() => { deleteIndex.Set(idx); })
            ))
            .Header(t => t.Icon, "Icon")
            .Builder(t => t.Icon, f => f.Func<ProjectRow, string?>(emoji =>
                !string.IsNullOrEmpty(emoji)
                    ? Text.Block(emoji)
                    : new Spacer()
            ));

        return Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
               | Text.Block("Projects").Bold()
               | table
               | new Button("Add Project").Icon(Icons.Plus).Outline().OnClick(() =>
               {
                   editIndex.Set(null);
               })
               | new EditProjectDialog(editIndex, projects, allVerifications, config, client, refreshToken)
               | new DeleteProjectDialog(deleteIndex, projects, config, client, refreshToken);
    }

    private record ProjectRow(int Index, string Name, string Color, string? Icon, int RepoCount, int VerificationCount);
}
