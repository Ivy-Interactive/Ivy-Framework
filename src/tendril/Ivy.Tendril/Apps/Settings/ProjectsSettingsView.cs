using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class ProjectsSettingsView : ViewBase
{
    public override object? Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var editIndex = UseState<int?>(-1);
        var deleteIndex = UseState<int?>(-1);

        // Edit form state — must be declared unconditionally (Ivy hook rules)
        var editName = UseState("");
        var editColor = UseState<Colors?>(null);
        var editSlackEmoji = UseState("");
        var editContext = UseState("");
        var editRepos = UseState(new List<RepoRef>());
        var editVerifications = UseState(new List<ProjectVerificationRef>());
        var newRepoPath = UseState("");
        var newRepoPrRule = UseState("default");
        var repoPathError = UseState<string?>(null);
        var editingRepoIndex = UseState<int?>(-1);
        var editingRepoPath = UseState("");
        var editingRepoError = UseState<string?>(null);

        var projects = config.Settings.Projects;
        var allVerifications = config.Settings.Verifications.Select(v => v.Name).ToList();

        var rows = projects.Select((p, i) => new ProjectRow(
            i, p.Name, p.Color, p.GetMeta("slackEmoji"), p.Repos.Count, p.Verifications.Count
        )).ToList();

        var table = new TableBuilder<ProjectRow>(rows)
            .Header(t => t.Index, "Actions")
            .Builder(t => t.Index, f => f.Func<ProjectRow, int>(idx =>
                Layout.Horizontal().Gap(1)
                    | new Button("Edit").Outline().Small().OnClick(() =>
                    {
                        LoadProjectIntoEditor(projects[idx], idx);
                    })
                    | new Button("Delete").Outline().Small().OnClick(() =>
                    {
                        deleteIndex.Set(idx);
                    })
            ))
            .Header(t => t.Icon, "Icon")
            .Builder(t => t.Icon, f => f.Func<ProjectRow, string?>(emoji =>
                !string.IsNullOrEmpty(emoji)
                    ? (object)Text.Block(emoji)
                    : new Spacer()
            ));

        var content = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
            | Text.Block("Projects").Bold()
            | table
            | new Button("Add Project").Icon(Icons.Plus).Outline().OnClick(() =>
            {
                editIndex.Set(null);
                editName.Set("");
                editColor.Set(null);
                editSlackEmoji.Set("");
                editContext.Set("");
                editRepos.Set(new List<RepoRef>());
                editVerifications.Set(new List<ProjectVerificationRef>());
            });

        // Edit dialog
        if (editIndex.Value != -1)
        {
            var isNew = editIndex.Value == null;

            var reposLayout = Layout.Vertical().Gap(2);
            var currentRepos = editRepos.Value;
            for (var i = 0; i < currentRepos.Count; i++)
            {
                var ri = i;
                var repo = currentRepos[ri];
                var expandedPath = Environment.ExpandEnvironmentVariables(repo.Path);
                var pathExists = Directory.Exists(expandedPath);
                var isEditing = editingRepoIndex.Value == ri;

                if (isEditing)
                {
                    reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                        | (!pathExists
                            ? (object)new Icon(Icons.TriangleAlert, Colors.Warning).Small()
                                .WithTooltip($"Path does not exist: {expandedPath}")
                            : new Spacer().Width(Size.Units(4)))
                        | editingRepoPath.ToTextInput("Repository path...").Width(Size.Grow())
                        | new Badge(repo.PrRule).Variant(BadgeVariant.Outline)
                        | new Button().Icon(Icons.Check).Ghost().Small().OnClick(() =>
                        {
                            var newPath = editingRepoPath.Value;
                            if (string.IsNullOrWhiteSpace(newPath))
                            {
                                editingRepoError.Set("Path cannot be empty");
                                return;
                            }

                            var expandedNewPath = Environment.ExpandEnvironmentVariables(newPath);
                            if (!Directory.Exists(expandedNewPath))
                            {
                                editingRepoError.Set($"Directory does not exist: {expandedNewPath}");
                                return;
                            }

                            var list = new List<RepoRef>(editRepos.Value);
                            list[ri] = new RepoRef { Path = newPath, PrRule = repo.PrRule };
                            editRepos.Set(list);
                            editingRepoIndex.Set(-1);
                            editingRepoError.Set(null);
                        })
                        | new Button().Icon(Icons.X).Ghost().Small().OnClick(() =>
                        {
                            editingRepoIndex.Set(-1);
                            editingRepoError.Set(null);
                        });
                }
                else
                {
                    var pathText = Text.Block(repo.Path).Width(Size.Grow());
                    if (!pathExists) pathText = pathText.Color(Colors.Red);

                    reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                        | (!pathExists
                            ? (object)new Icon(Icons.TriangleAlert, Colors.Warning).Small()
                                .WithTooltip($"Path does not exist: {expandedPath}")
                            : new Spacer().Width(Size.Units(4)))
                        | pathText
                        | new Badge(repo.PrRule).Variant(BadgeVariant.Outline)
                        | new Button().Icon(Icons.Pencil).Ghost().Small()
                            .OnClick(() =>
                            {
                                editingRepoIndex.Set(ri);
                                editingRepoPath.Set(repo.Path);
                                editingRepoError.Set(null);
                            })
                            .WithTooltip("Edit path")
                        | new Button().Icon(Icons.Trash).Ghost().Small().OnClick(() =>
                        {
                            var list = new List<RepoRef>(editRepos.Value);
                            list.RemoveAt(ri);
                            editRepos.Set(list);
                        });
                }
            }

            if (editingRepoError.Value != null)
            {
                reposLayout |= Text.Danger(editingRepoError.Value);
            }

            if (repoPathError.Value != null)
            {
                reposLayout |= Text.Danger(repoPathError.Value);
            }

            reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                | newRepoPath.ToTextInput("Repo path...").Width(Size.Grow())
                | newRepoPrRule.ToSelectInput(new List<string> { "default", "yolo" }).Width(Size.Units(20))
                | new Button("Add").Outline().Small().OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(newRepoPath.Value))
                    {
                        var expandedPath = Environment.ExpandEnvironmentVariables(newRepoPath.Value);

                        if (!Directory.Exists(expandedPath))
                        {
                            repoPathError.Set($"Directory does not exist: {expandedPath}");
                            return;
                        }

                        var list = new List<RepoRef>(editRepos.Value)
                        {
                            new() { Path = newRepoPath.Value, PrRule = newRepoPrRule.Value }
                        };
                        editRepos.Set(list);
                        newRepoPath.Set("");
                        newRepoPrRule.Set("default");
                        repoPathError.Set(null);
                    }
                });

            // Verifications checklist
            var verificationsLayout = Layout.Vertical().Gap(1);
            foreach (var vName in allVerifications)
            {
                var existing = editVerifications.Value.FirstOrDefault(v => v.Name == vName);
                var isChecked = existing != null;
                var isRequired = existing?.Required ?? false;
                var capturedName = vName;

                verificationsLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                    | new Button(isChecked ? "x" : " ")
                        .Ghost().Small().OnClick(() =>
                        {
                            var list = new List<ProjectVerificationRef>(editVerifications.Value);
                            if (isChecked)
                                list.RemoveAll(v => v.Name == capturedName);
                            else
                                list.Add(new ProjectVerificationRef { Name = capturedName, Required = false });
                            editVerifications.Set(list);
                        })
                    | Text.Block(capturedName).Width(Size.Grow())
                    | (isChecked
                        ? (object)new Button(isRequired ? "Required" : "Optional")
                            .Small()
                            .Variant(isRequired ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .OnClick(() =>
                            {
                                var list = new List<ProjectVerificationRef>(editVerifications.Value);
                                var item = list.First(v => v.Name == capturedName);
                                item.Required = !item.Required;
                                editVerifications.Set(list);
                            })
                        : new Spacer());
            }

            content |= new Dialog(
                _ => { editIndex.Set(-1); editingRepoIndex.Set(-1); editingRepoError.Set(null); },
                new DialogHeader(isNew ? "Add Project" : $"Edit Project: {editName.Value}"),
                new DialogBody(
                    Layout.Vertical().Gap(4)
                        | editName.ToTextInput("Project name...").WithField().Label("Name")
                        | editColor.ToSelectInput().WithField().Label("Color")
                        | editSlackEmoji.ToTextInput(":emoji:").WithField().Label("Slack Emoji")
                        | editContext.ToTextareaInput("Project context or prompt for AI agents (optional)...").Rows(4).WithField().Label("Context / Prompt (Optional)")
                        | (Layout.Vertical().Gap(2)
                            | Text.Block("Repositories").Bold()
                            | reposLayout)
                        | (Layout.Vertical().Gap(2)
                            | Text.Block("Verifications").Bold()
                            | verificationsLayout)
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => { editIndex.Set(-1); editingRepoIndex.Set(-1); editingRepoError.Set(null); }),
                    new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                    {
                        if (string.IsNullOrWhiteSpace(editName.Value)) return;
                        var project = isNew ? new ProjectConfig() : projects[editIndex.Value!.Value];
                        project.Name = editName.Value;
                        project.Color = editColor.Value?.ToString() ?? "";
                        project.Meta["slackEmoji"] = editSlackEmoji.Value;
                        project.Context = editContext.Value;
                        project.Repos = new List<RepoRef>(editRepos.Value);
                        project.Verifications = new List<ProjectVerificationRef>(editVerifications.Value);
                        if (isNew) projects.Add(project);
                        config.SaveSettings();
                        editIndex.Set(-1);
                        editingRepoIndex.Set(-1);
                        editingRepoError.Set(null);
                        refreshToken.Refresh();
                        client.Toast($"Project '{editName.Value}' saved", "Saved");
                    })
                )
            ).Width(Size.Rem(40));
        }

        // Delete confirmation
        if (deleteIndex.Value is { } di && di >= 0 && di < projects.Count)
        {
            var projectName = projects[di].Name;
            content |= new Dialog(
                _ => deleteIndex.Set(-1),
                new DialogHeader("Delete Project"),
                new DialogBody(
                    Text.P($"Are you sure you want to delete the project '{projectName}'? This cannot be undone.")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => deleteIndex.Set(-1)),
                    new Button("Delete").Variant(ButtonVariant.Destructive).OnClick(() =>
                    {
                        projects.RemoveAt(di);
                        config.SaveSettings();
                        deleteIndex.Set(-1);
                        refreshToken.Refresh();
                        client.Toast($"Project '{projectName}' deleted", "Deleted");
                    })
                )
            ).Width(Size.Rem(25));
        }

        return content;

        void LoadProjectIntoEditor(ProjectConfig project, int idx)
        {
            editIndex.Set(idx);
            editName.Set(project.Name);
            editColor.Set(Enum.TryParse<Colors>(project.Color, out var c) ? c : null);
            editSlackEmoji.Set(project.GetMeta("slackEmoji") ?? "");
            editContext.Set(project.Context);
            editRepos.Set(new List<RepoRef>(project.Repos.Select(r => new RepoRef { Path = r.Path, PrRule = r.PrRule })));
            editVerifications.Set(new List<ProjectVerificationRef>(
                project.Verifications.Select(v => new ProjectVerificationRef { Name = v.Name, Required = v.Required })));
        }
    }

    private record ProjectRow(int Index, string Name, string Color, string? Icon, int RepoCount, int VerificationCount);
}
