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

        var rows = projects.Select((p, i) => new ProjectRow(p.Name, p.Color, p.Repos.Count, p.Verifications.Count, i)).ToList();

        var table = new TableBuilder<ProjectRow>(rows)
            .Header(t => t.Index, "Actions")
            .Builder(t => t.Index, f => f.Func<ProjectRow, int>(idx =>
                Layout.Horizontal().Gap(1)
                | new Button().Icon(Icons.Pencil).Outline().Small().OnClick(() =>
                {
                    editIndex.Set(idx);
                })
                | new Button().Icon(Icons.Trash).Outline().Small().OnClick(() => { deleteIndex.Set(idx); })
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

    private record ProjectRow(string Name, string Color, int RepoCount, int VerificationCount, int Index);
}
