using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup.Dialogs;

public class EditProjectDialog(
    IState<int?> editIndex,
    List<ProjectConfig> projects,
    List<string> allVerifications,
    IConfigService config,
    IClientProvider client,
    RefreshToken refreshToken) : ViewBase
{
    private readonly IState<int?> _editIndex = editIndex;
    private readonly List<ProjectConfig> _projects = projects;
    private readonly List<string> _allVerifications = allVerifications;
    private readonly IConfigService _config = config;
    private readonly IClientProvider _client = client;
    private readonly RefreshToken _refreshToken = refreshToken;

    public override object? Build()
    {
        var editName = UseState("");
        var editColor = UseState<Colors?>(null);
        var editContext = UseState("");
        var editRepos = UseState(new List<RepoRef>());
        var editVerifications = UseState(new List<ProjectVerificationRef>());
        var newRepoPath = UseState<string?>(null);
        var newRepoPrRule = UseState("default");
        var repoPathError = UseState<string?>(null);
        var editingRepoIndex = UseState<int?>(-1);
        var editingRepoPath = UseState<string?>(null);
        var editingRepoError = UseState<string?>(null);

        UseEffect(() =>
        {
            if (_editIndex.Value == null)
            {
                editName.Set("");
                editColor.Set(null);
                editContext.Set("");
                editRepos.Set(new List<RepoRef>());
                editVerifications.Set(new List<ProjectVerificationRef>());
            }
            else if (_editIndex.Value >= 0)
            {
                var project = _projects[_editIndex.Value.Value];
                editName.Set(project.Name);
                editColor.Set(Enum.TryParse<Colors>(project.Color, out var c) ? c : null);
                editContext.Set(project.Context);
                editRepos.Set(
                    new List<RepoRef>(project.Repos.Select(r => new RepoRef { Path = r.Path, PrRule = r.PrRule })));
                editVerifications.Set(new List<ProjectVerificationRef>(
                    project.Verifications.Select(v => new ProjectVerificationRef
                    { Name = v.Name, Required = v.Required })));
            }

            newRepoPath.Set(null);
            newRepoPrRule.Set("default");
            repoPathError.Set(null);
            editingRepoIndex.Set(-1);
            editingRepoPath.Set(null);
            editingRepoError.Set(null);
        }, _editIndex);

        if (_editIndex.Value == -1) return null;

        var isNew = _editIndex.Value == null;

        var reposLayout = Layout.Vertical().Gap(2);
        var currentRepos = editRepos.Value;
        for (var i = 0; i < currentRepos.Count; i++)
        {
            var ri = i;
            var repo = currentRepos[ri];
            var expandedPath = VariableExpansion.ExpandVariables(repo.Path, _config.TendrilHome);
            var pathExists = Directory.Exists(expandedPath);
            var isGitRepo = pathExists && Path.Exists(Path.Combine(expandedPath, ".git"));
            var isEditing = editingRepoIndex.Value == ri;

            if (isEditing)
            {
                reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                               | (!isGitRepo
                                   ? (object)new Icon(Icons.TriangleAlert, Colors.Warning).Small()
                                       .WithTooltip(!pathExists
                                           ? $"Path does not exist: {expandedPath}"
                                           : $"Not a git repository: {expandedPath}")
                                   : new Spacer().Width(Size.Units(4)))
                               | editingRepoPath
                                   .ToTextInput("Select repository folder...")
                                   .Width(Size.Grow())
                               | new Badge(repo.PrRule).Variant(BadgeVariant.Outline)
                               | new Button().Icon(Icons.Check).Ghost().Small().OnClick(() =>
                               {
                                   var newPath = editingRepoPath.Value;
                                   if (string.IsNullOrWhiteSpace(newPath))
                                   {
                                       editingRepoError.Set("Path cannot be empty");
                                       return;
                                   }

                                   var expandedNewPath = VariableExpansion.ExpandVariables(newPath, _config.TendrilHome);
                                   if (!Directory.Exists(expandedNewPath))
                                   {
                                       editingRepoError.Set($"Directory does not exist: {expandedNewPath}");
                                       return;
                                   }

                                   if (!Path.Exists(Path.Combine(expandedNewPath, ".git")))
                                   {
                                       editingRepoError.Set($"Directory is not a git repository: {expandedNewPath}");
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
                if (!isGitRepo) pathText = pathText.Color(Colors.Red);

                reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                               | (!isGitRepo
                                   ? (object)new Icon(Icons.TriangleAlert, Colors.Warning).Small()
                                       .WithTooltip(!pathExists
                                           ? $"Path does not exist: {expandedPath}"
                                           : $"Not a git repository: {expandedPath}")
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

        if (editingRepoError.Value != null) reposLayout |= Text.Danger(editingRepoError.Value);

        if (repoPathError.Value != null) reposLayout |= Text.Danger(repoPathError.Value);

        reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                       | newRepoPath.ToTextInput("Select repository folder...")
                           .Width(Size.Grow())
                       | newRepoPrRule.ToSelectInput(new List<string> { "default", "yolo" }).Width(Size.Units(20))
                       | new Button("Add").Outline().Small().OnClick(() =>
                       {
                           if (!string.IsNullOrWhiteSpace(newRepoPath.Value))
                           {
                               var expandedNewPath = VariableExpansion.ExpandVariables(newRepoPath.Value, _config.TendrilHome);

                               if (!Directory.Exists(expandedNewPath))
                               {
                                   repoPathError.Set($"Directory does not exist: {expandedNewPath}");
                                   return;
                               }

                               if (!Path.Exists(Path.Combine(expandedNewPath, ".git")))
                               {
                                   repoPathError.Set($"Directory is not a git repository: {expandedNewPath}");
                                   return;
                               }

                               var list = new List<RepoRef>(editRepos.Value)
                               {
                                   new() { Path = newRepoPath.Value, PrRule = newRepoPrRule.Value }
                               };
                               editRepos.Set(list);
                               newRepoPath.Set(null);
                               newRepoPrRule.Set("default");
                               repoPathError.Set(null);
                           }
                       });

        // Verifications checklist
        var verificationsLayout = Layout.Vertical().Gap(1);
        foreach (var vName in _allVerifications)
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
                                               list.Add(new ProjectVerificationRef
                                               { Name = capturedName, Required = false });
                                           editVerifications.Set(list);
                                       })
                                   | Text.Block(capturedName).Width(Size.Grow())
                                   | (isChecked
                                       ? new Button(isRequired ? "Required" : "Optional")
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

        return new Dialog(
            _ =>
            {
                _editIndex.Set(-1);
                editingRepoIndex.Set(-1);
                editingRepoError.Set(null);
            },
            new DialogHeader(isNew ? "Add Project" : $"Edit Project: {editName.Value}"),
            new DialogBody(
                Layout.Vertical().Gap(4)
                | editName.ToTextInput("Project name...").WithField().Label("Name")
                | editColor.ToSelectInput().WithField().Label("Color")
                | editContext.ToTextareaInput("Project context or prompt for AI agents (optional)...").Rows(4)
                    .WithField().Label("Context / Prompt (Optional)")
                | (Layout.Vertical().Gap(2)
                   | Text.Block("Repositories").Bold()
                   | reposLayout)
                | (Layout.Vertical().Gap(2)
                   | Text.Block("Verifications").Bold()
                   | verificationsLayout)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() =>
                {
                    _editIndex.Set(-1);
                    editingRepoIndex.Set(-1);
                    editingRepoError.Set(null);
                }),
                new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                {
                    if (string.IsNullOrWhiteSpace(editName.Value)) return;
                    var project = isNew ? new ProjectConfig() : _projects[_editIndex.Value!.Value];
                    project.Name = editName.Value;
                    project.Color = editColor.Value?.ToString() ?? "";
                    project.Context = editContext.Value;
                    project.Repos = new List<RepoRef>(editRepos.Value);
                    project.Verifications = new List<ProjectVerificationRef>(editVerifications.Value);
                    if (isNew) _projects.Add(project);
                    _config.SaveSettings();
                    _editIndex.Set(-1);
                    editingRepoIndex.Set(-1);
                    editingRepoError.Set(null);
                    _refreshToken.Refresh();
                    _client.Toast($"Project '{editName.Value}' saved", "Saved");
                })
            )
        ).Width(Size.Rem(40));
    }
}
