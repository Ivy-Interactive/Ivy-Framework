using Ivy.Tendril.Apps.Onboarding.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Onboarding;

public class ProjectSetupStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var projectName = UseState("");
        var repoPaths = UseState(new List<string> { "" });
        var projectContext = UseState("");
        var error = UseState<string?>(null);
        var verifications = UseState(new List<VerificationEntry>
        {
            new("CheckResult", "Verify the implementation matches the plan requirements.", true)
        });

        // Dialog state for editing verifications
        var editIndex = UseState<int?>(-1); // -1 = closed, null = new, >= 0 = editing index

        var reposLayout = Layout.Vertical().Gap(2);
        var currentRepos = repoPaths.Value;
        for (var i = 0; i < currentRepos.Count; i++)
        {
            var ri = i;
            reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                           | new FolderInput
                           {
                               Value = currentRepos[ri],
                               Placeholder = "Select repository folder...",
                               Mode = FolderInputMode.FullPath,
                               OnChange = new(e =>
                               {
                                   var list = new List<string>(repoPaths.Value);
                                   list[ri] = e.Value ?? "";
                                   repoPaths.Set(list);
                                   return ValueTask.CompletedTask;
                               })
                           }.Width(Size.Grow());
        }

        reposLayout |= new Button("Add").Outline().OnClick(() =>
        {
            var list = new List<string>(repoPaths.Value) { "" };
            repoPaths.Set(list);
        });

        // Verification list
        var verificationsLayout = Layout.Vertical().Gap(2);
        var currentVerifications = verifications.Value;
        for (var i = 0; i < currentVerifications.Count; i++)
        {
            var vi = i;
            var v = currentVerifications[vi];
            verificationsLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                                   | Text.Block(v.Name).Width(Size.Grow())
                                   | (v.Required ? new Badge("Required") : null!)
                                   | new Button().Icon(Icons.Pencil).Ghost().OnClick(() =>
                                   {
                                       editIndex.Set(vi);
                                   })
                                   | new Button().Icon(Icons.Trash).Ghost().OnClick(() =>
                                   {
                                       var list = new List<VerificationEntry>(verifications.Value);
                                       list.RemoveAt(vi);
                                       verifications.Set(list);
                                   });
        }

        return Layout.Vertical().Gap(4)
               | Text.H2("Project Setup")
               | Text.Muted("Set up your first project. You can add more projects later in Settings.")
               | (error.Value != null ? Text.Danger(error.Value) : null!)
               | projectName.ToTextInput().WithField().Required().Label("Project Name")
               | projectContext.ToTextareaInput()
                   .Rows(4)
                   .WithField()
                   .Label("Context (Optional)")
               | (Layout.Vertical().Gap(2)
                  | Text.Block("Repositories").Bold()
                  | Text.Muted("Add at least one repository path for this project.")
                  | reposLayout)
               | (Layout.Vertical().Gap(2)
                  | Text.Block("Verifications").Bold()
                  | Text.Muted("Define verifications to run for this project.")
                  | verificationsLayout
                  | new Button("Add Verification").Icon(Icons.Plus).Outline().OnClick(() =>
                  {
                      editIndex.Set(null);
                  }))
               | (Layout.Horizontal().Gap(2)
                  | new Button("Next").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                      .OnClick(() =>
                      {
                          if (string.IsNullOrWhiteSpace(projectName.Value))
                          {
                              error.Set("Please enter a project name.");
                              return;
                          }

                          var filledRepos = repoPaths.Value.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                          if (filledRepos.Count == 0)
                          {
                              error.Set("Please add at least one repository path.");
                              return;
                          }

                          var invalidRepo = filledRepos.FirstOrDefault(p =>
                          {
                              var expanded = Environment.ExpandEnvironmentVariables(p);
                              return !Directory.Exists(expanded) || !Path.Exists(Path.Combine(expanded, ".git"));
                          });
                          if (invalidRepo != null)
                          {
                              var expanded = Environment.ExpandEnvironmentVariables(invalidRepo);
                              error.Set(!Directory.Exists(expanded)
                                  ? $"Directory does not exist: {expanded}"
                                  : $"Directory is not a git repository: {expanded}");
                              return;
                          }

                          var validVerifications = verifications.Value
                              .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                              .ToList();

                          var project = new ProjectConfig
                          {
                              Name = projectName.Value.Trim(),
                              Color = "Green",
                              Repos = repoPaths.Value
                                  .Where(p => !string.IsNullOrWhiteSpace(p))
                                  .Select(p => new RepoRef { Path = p, PrRule = "default" })
                                  .ToList(),
                              Context = projectContext.Value.Trim(),
                              Verifications = validVerifications.Select(v => new ProjectVerificationRef
                              {
                                  Name = v.Name,
                                  Required = v.Required
                              }).ToList()
                          };

                          config.SetPendingProject(project);
                          config.SetPendingVerificationDefinitions(validVerifications
                              .Select(v => new VerificationConfig
                              {
                                  Name = v.Name,
                                  Prompt = v.Prompt
                              }).ToList());

                          error.Set(null);
                          stepperIndex.Set(stepperIndex.Value + 1);
                      })
               )
               | new EditOnboardingVerificationDialog(editIndex, verifications);
    }
}

internal record VerificationEntry(string Name, string Prompt, bool Required);
