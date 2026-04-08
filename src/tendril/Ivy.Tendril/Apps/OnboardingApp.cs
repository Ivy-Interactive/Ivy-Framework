using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Ivy.Helpers;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

#if DEBUG
[App(title: "Onboarding", icon: Icons.Rocket, group: new[] { "Debug" }, isVisible: true, order: MenuOrder.Onboarding)]
#else
[App(icon: Icons.Rocket, isVisible: false, order: MenuOrder.Onboarding)]
#endif
public class OnboardingApp : ViewBase
{
    private static StepperItem[] GetSteps(int selectedIndex)
    {
        return
        [
            new("1", selectedIndex > 0 ? Icons.Check : null, "Welcome"),
            new("2", selectedIndex > 1 ? Icons.Check : null, "Software"),
            new("3", selectedIndex > 2 ? Icons.Check : null, "Coding Agent"),
            new("4", selectedIndex > 3 ? Icons.Check : null, "Storage"),
            new("5", selectedIndex > 4 ? Icons.Check : null, "Project"),
            new("6", selectedIndex > 5 ? Icons.Check : null, "Complete")
        ];
    }

    private static object GetStepViews(IState<int> stepperIndex, IState<Dictionary<string, bool>?> checkResults)
    {
        return stepperIndex.Value switch
        {
            0 => new WelcomeStepView(stepperIndex),
            1 => new SoftwareCheckStepView(stepperIndex, checkResults),
            2 => new CodingAgentStepView(stepperIndex, checkResults.Value ?? new Dictionary<string, bool>()),
            3 => new TendrilHomeStepView(stepperIndex),
            4 => new ProjectSetupStepView(stepperIndex),
            5 => new CompleteStepView(stepperIndex),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override object Build()
    {
        var stepperIndex = UseState(0);
        var checkResults = UseState<Dictionary<string, bool>?>(null);
        var steps = GetSteps(stepperIndex.Value);

        return Layout.TopCenter() |
               (Layout.Vertical().Margin(0, 20).Width(150)
                | new Image("/tendril/assets/Tendril.svg").Width(Size.Units(20)).Height(Size.Auto())
                | new Stepper(OnSelect, stepperIndex.Value, steps).Width(Size.Full())
                | GetStepViews(stepperIndex, checkResults)
               );

        ValueTask OnSelect(Event<Stepper, int> e)
        {
            stepperIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}

public class WelcomeStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        return Layout.Vertical()
               | Text.H1("Welcome to Ivy Tendril")
               | Text.Markdown(
                   """
                   >[!NOTE]
                   >Ivy Tendril is a coding orchestrator powered by agents like Claude Code, Codex, or Gemini. It’s designed to help you complete large amounts of work quickly. Be aware that Tendril will consume lots of tokens rapidly.
                   """)
               | new Button("Get Started").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1));
    }
}

public class SoftwareCheckStepView(IState<int> stepperIndex, IState<Dictionary<string, bool>?> checkResults) : ViewBase
{
    public override object Build()
    {
        var isChecking = UseState(false);

        var hasAnyCodingAgent = checkResults.Value != null
                                && (checkResults.Value["claude"] || checkResults.Value["codex"] ||
                                    checkResults.Value["gemini"]);

        var allRequiredPassed = checkResults.Value != null
                                && checkResults.Value["gh"]
                                && hasAnyCodingAgent
                                && checkResults.Value["git"]
                                && checkResults.Value["powershell"];

        return Layout.Vertical()
               | Text.H2("Required Software")
               | Text.Markdown(
                   """
                   Tendril requires the following software to be installed:

                   **Required:**
                   - **Coding Agent** - At least one of: Claude Code CLI, Codex CLI, or Gemini CLI
                   - **GitHub CLI** - For PR creation and GitHub integration
                   - **Git** - For version control
                   - **PowerShell** - For running promptware and hooks

                   **Optional:**
                   - **Pandoc** - For PDF export functionality
                   """)
               | (checkResults.Value != null
                   ? Layout.Vertical()
                     | Text.H3("Results")
                     | new Table(
                         new TableRow(
                             new TableCell("Software").IsHeader(),
                             new TableCell("Status").IsHeader(),
                             new TableCell("Notes").IsHeader()
                         ).IsHeader(),
                         MakeSoftwareRow(checkResults.Value, "GitHub CLI", "gh", "https://cli.github.com/", true),
                         MakeSoftwareRow(checkResults.Value, "Claude CLI", "claude", "https://docs.anthropic.com/en/docs/claude-code", false),
                         MakeSoftwareRow(checkResults.Value, "Codex CLI", "codex", "https://openai.com/index/codex/", false),
                         MakeSoftwareRow(checkResults.Value, "Gemini CLI", "gemini", "https://github.com/google-gemini/gemini-cli", false),
                         MakeSoftwareRow(checkResults.Value, "Git", "git", "https://git-scm.com/downloads", true),
                         MakeSoftwareRow(checkResults.Value, "PowerShell", "powershell", "https://github.com/PowerShell/PowerShell", true),
                         MakeSoftwareRow(checkResults.Value, "Pandoc (Optional)", "pandoc", "https://pandoc.org/installing.html", false)
                     ).Width(Size.Full())
                   : null!)
               | (checkResults.Value == null
                   ? new Button("Check Software")
                       .Primary()
                       .Large()
                       .Icon(Icons.CheckCheck, Align.Right)
                       .Loading(isChecking.Value)
                       .Disabled(isChecking.Value)
                       .OnClick(async () => await CheckSoftware())
                   : allRequiredPassed
                       ? new Button("Continue")
                           .Primary()
                           .Large()
                           .Icon(Icons.ArrowRight, Align.Right)
                           .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                       : Layout.Vertical()
                         | Text.Warning(
                             "Please install all required software before continuing. At least one coding agent (Claude, Codex, or Gemini), GitHub CLI, Git, and PowerShell must be installed.")
                            | new Button("Check Again")
                                .Outline()
                                .Icon(Icons.CheckCheck, Align.Right)
                                .OnClick(async () => await CheckSoftware())
               );

        async Task CheckSoftware()
        {
            isChecking.Set(true);
            var results = new Dictionary<string, bool>
            {
                ["gh"] = await CheckCommand("gh", "--version"),
                ["claude"] = await CheckCommand("claude", "--version"),
                ["codex"] = await CheckCommand("codex", "--version"),
                ["gemini"] = await CheckCommand("gemini", "--version"),
                ["git"] = await CheckCommand("git", "--version"),
                ["powershell"] = await CheckCommand("pwsh", "-Version")
                                 || await CheckCommand("powershell", "-Version"),
                ["pandoc"] = await CheckCommand("pandoc", "--version")
            };

            checkResults.Set(results);
            isChecking.Set(false);
        }
    }

    private static TableRow MakeSoftwareRow(
        Dictionary<string, bool> results,
        string displayName,
        string key,
        string installUrl,
        bool isRequired)
    {
        var installed = results[key];
        var statusText = installed
            ? "✅ Installed"
            : isRequired ? "❌ Not Found" : "❌ Not Installed";

        return new TableRow(
            new TableCell(displayName),
            new TableCell(statusText),
            installed
                ? new TableCell("")
                : new TableCell(new Button("Install").Inline().Url(installUrl))
        );
    }

    private static async Task<bool> CheckCommand(string fileName, string arguments)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
                    Arguments = OperatingSystem.IsWindows() ? $"/c \"{fileName}\" {arguments}" : arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc.WaitForExitOrKill(10000);
                return proc?.ExitCode == 0;
            });
        }
        catch
        {
            return false;
        }
    }
}

public class CodingAgentStepView(IState<int> stepperIndex, IReadOnlyDictionary<string, bool> checkResults) : ViewBase
{
    private static readonly (string Label, string Name)[] AgentOptions = [("Claude Code", "claude"), ("Codex", "codex"), ("Gemini", "gemini")];

    public override object Build()
    {
        var config = UseService<IConfigService>();

        var installedOptions = AgentOptions.Where(a => checkResults.ContainsKey(a.Name) && checkResults[a.Name]).ToArray();
        if (installedOptions.Length == 0) installedOptions = AgentOptions;

        var defaultLabel = installedOptions.Any(a => a.Name == config.Settings.CodingAgent)
            ? installedOptions.First(a => a.Name == config.Settings.CodingAgent).Label
            : installedOptions.First().Label;

        var selectedAgent = UseState(defaultLabel);

        return Layout.Vertical()
                | Text.H2("Choose Your Coding Agent")
                | selectedAgent.ToSelectInput(installedOptions.Select(a => a.Label).ToArray())
                   .Variant(SelectInputVariant.Toggle)
                   .WithField()
                   .Label("Coding Agent")
                | new Button("Continue").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .OnClick(() =>
                   {
                       config.Settings.CodingAgent = installedOptions.First(a => a.Label == selectedAgent.Value).Name;
                       stepperIndex.Set(stepperIndex.Value + 1);
                   });
    }
}

public record TendrilHomeDetails
{
    [Required]
    [Display(Name = "Where would you like to store Tendril data?")]
    public string? TendrilHome { get; set; }
}

public class TendrilHomeStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var details = UseState(new TendrilHomeDetails
        {
            TendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME")
                          ?? Path.Combine(
                              Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                              ".tendril"
                          )
        });
        var error = UseState<string?>(null);
        var config = UseService<IConfigService>();

        return Layout.Vertical()
               | Text.H2("Tendril Data Location")
               | Text.Muted("This folder will store your plans, inbox, trash, and other Tendril data.")
               | (error.Value != null ? Text.Danger(error.Value) : null!)
               | details.ToForm().Large()
                   .SubmitBuilder(saving => new Button("Next").Icon(Icons.ArrowRight, Align.Right).Disabled(saving))
                   .OnSubmit(OnSubmit)
            ;

        Task OnSubmit(TendrilHomeDetails? formDetails)
        {
            if (string.IsNullOrEmpty(formDetails?.TendrilHome))
            {
                error.Set("Please provide a valid path");
                return Task.CompletedTask;
            }

            try
            {
                var tendrilHome = formDetails.TendrilHome;

                // Expand environment variables (handles %VAR%)
                tendrilHome = Environment.ExpandEnvironmentVariables(tendrilHome);

                // Manual expansion for ~ and $HOME (Mac/Linux)
                if (tendrilHome.StartsWith("~"))
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    if (tendrilHome == "~") tendrilHome = home;
                    else if (tendrilHome.StartsWith("~/") || tendrilHome.StartsWith("~\\"))
                        tendrilHome = Path.Combine(home, tendrilHome.Substring(2));
                }
                else if (tendrilHome.StartsWith("$"))
                {
                    // Handle $HOME style expansion if missed by ExpandEnvironmentVariables
                    var match = Regex.Match(tendrilHome, @"^\$([A-Za-z_][A-Za-z0-9_]*)");
                    if (match.Success)
                    {
                        var varName = match.Groups[1].Value;
                        var varValue = Environment.GetEnvironmentVariable(varName);
                        if (!string.IsNullOrEmpty(varValue))
                            tendrilHome = varValue + tendrilHome.Substring(match.Length);
                    }
                }

                // Normalize and root
                if (!Path.IsPathRooted(tendrilHome)) tendrilHome = Path.GetFullPath(tendrilHome);

                // Final normalization
                tendrilHome = Path.GetFullPath(tendrilHome);

                // Store in config for next step
                config.SetPendingTendrilHome(tendrilHome);
                error.Set(null);
                stepperIndex.Set(stepperIndex.Value + 1);
            }
            catch (Exception ex)
            {
                error.Set($"Invalid path: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}

public class ProjectSetupStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var projectName = UseState("");
        var repoPaths = UseState(new List<string>());
        var newRepoPath = UseState<string?>(null);
        var projectContext = UseState("");
        var error = UseState<string?>(null);
        var verifications = UseState(new List<VerificationEntry>
        {
            new("CheckResult", "Verify the implementation matches the plan requirements.", true)
        });

        // Dialog state for editing verifications
        var editIndex = UseState<int?>(-1); // -1 = closed, null = new, >= 0 = editing index
        var editName = UseState("");
        var editPrompt = UseState("");
        var editRequired = UseState(false);

        var reposLayout = Layout.Vertical().Gap(2);
        var currentRepos = repoPaths.Value;
        for (var i = 0; i < currentRepos.Count; i++)
        {
            var ri = i;
            reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                           | Text.Block(currentRepos[ri]).Width(Size.Grow())
                           | new Button().Icon(Icons.Trash).Ghost().Small().OnClick(() =>
                           {
                               var list = new List<string>(repoPaths.Value);
                               list.RemoveAt(ri);
                               repoPaths.Set(list);
                           });
        }

        reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                       | newRepoPath.ToFolderInput("Select repository folder...", mode: FolderInputMode.FullPath)
                           .Width(Size.Grow())
                       | new Button("Add").Outline().Small().OnClick(() =>
                       {
                           if (!string.IsNullOrWhiteSpace(newRepoPath.Value))
                           {
                               var list = new List<string>(repoPaths.Value) { newRepoPath.Value };
                               repoPaths.Set(list);
                               newRepoPath.Set(null);
                           }
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
                                   | new Button().Icon(Icons.Pencil).Ghost().Small().OnClick(() =>
                                   {
                                       editIndex.Set(vi);
                                       editName.Set(verifications.Value[vi].Name);
                                       editPrompt.Set(verifications.Value[vi].Prompt);
                                       editRequired.Set(verifications.Value[vi].Required);
                                   })
                                   | new Button().Icon(Icons.Trash).Ghost().Small().OnClick(() =>
                                   {
                                       var list = new List<VerificationEntry>(verifications.Value);
                                       list.RemoveAt(vi);
                                       verifications.Set(list);
                                   });
        }

        var content = Layout.Vertical().Gap(4)
                      | Text.H2("Project Setup")
                      | Text.Muted("Set up your first project. You can add more projects later in Settings.")
                      | (error.Value != null ? Text.Danger(error.Value) : null!)
                      | projectName.ToTextInput("Project name...").WithField().Label("Project Name")
                      | projectContext.ToTextareaInput("Project context or prompt for AI agents (optional)...")
                          .Rows(4)
                          .WithField()
                          .Label("Context / Prompt (Optional)")
                      | (Layout.Vertical().Gap(2)
                         | Text.Block("Repositories").Bold()
                         | Text.Muted("Add at least one repository path for this project.")
                         | reposLayout)
                      | (Layout.Vertical().Gap(2)
                         | Text.Block("Verifications").Bold()
                         | Text.Muted("Define verifications to run for this project.")
                         | verificationsLayout
                         | new Button("Add Verification").Icon(Icons.Plus).Outline().Small().OnClick(() =>
                         {
                             editIndex.Set(null);
                             editName.Set("");
                             editPrompt.Set("");
                             editRequired.Set(false);
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

                                 if (repoPaths.Value.Count == 0)
                                 {
                                     error.Set("Please add at least one repository path.");
                                     return;
                                 }

                                 var validVerifications = verifications.Value
                                     .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                                     .ToList();

                                 var project = new ProjectConfig
                                 {
                                     Name = projectName.Value.Trim(),
                                     Color = "Green",
                                     Repos = repoPaths.Value.Select(p => new RepoRef { Path = p, PrRule = "default" })
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
                      );

        // Verification edit dialog
        if (editIndex.Value != -1)
        {
            var isNew = editIndex.Value == null;
            content |= new Dialog(
                _ => editIndex.Set(-1),
                new DialogHeader(isNew ? "Add Verification" : "Edit Verification"),
                new DialogBody(
                    Layout.Vertical().Gap(2)
                    | editName.ToTextInput("Verification name...").WithField().Label("Name")
                    | editPrompt.ToTextareaInput("Verification prompt...").Rows(6).WithField().Label("Prompt")
                    | editRequired.ToBoolInput("Required")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => editIndex.Set(-1)),
                    new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                    {
                        if (string.IsNullOrWhiteSpace(editName.Value)) return;
                        var list = new List<VerificationEntry>(verifications.Value);
                        if (isNew)
                            list.Add(new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value));
                        else
                            list[editIndex.Value!.Value] =
                                new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value);
                        verifications.Set(list);
                        editIndex.Set(-1);
                    })
                )
            ).Width(Size.Rem(35));
        }

        return content;
    }

    private record VerificationEntry(string Name, string Prompt, bool Required);
}

public class CompleteStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var isProcessing = UseState(false);
        var error = UseState<string?>(null);
        var config = UseService<IConfigService>();
        var navigator = UseNavigation();

        async Task OnComplete()
        {
            isProcessing.Set(true);
            error.Set(null);

            try
            {
                var tendrilHome = config.GetPendingTendrilHome();

                if (string.IsNullOrEmpty(tendrilHome))
                {
                    error.Set("Tendril home path not set");
                    isProcessing.Set(false);
                    return;
                }

                // Create directory structure
                Directory.CreateDirectory(tendrilHome);
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Inbox"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Plans"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Trash"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Promptwares"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Hooks"));

                // Copy template or create basic config
                var projectDir = Path.GetDirectoryName(System.AppContext.BaseDirectory); // Go up from bin/Debug/...
                while (projectDir != null && !File.Exists(Path.Combine(projectDir, "example.config.yaml")))
                    projectDir = Path.GetDirectoryName(projectDir);

                var exampleConfigPath = projectDir != null
                    ? Path.Combine(projectDir, "example.config.yaml")
                    : Path.Combine(System.AppContext.BaseDirectory, "example.config.yaml");

                var configPath = Path.Combine(tendrilHome, "config.yaml");

                if (File.Exists(exampleConfigPath))
                {
                    var exampleContent = await FileHelper.ReadAllTextAsync(exampleConfigPath);
                    await FileHelper.WriteAllTextAsync(configPath, exampleContent);
                }
                else if (!File.Exists(configPath))
                {
                    // Create a basic config.yaml only if it doesn't exist
                    var basicConfig = "codingAgent: claude\n" +
                                      "jobTimeout: 30\n" +
                                      "staleOutputTimeout: 10\n" +
                                      "projects: []\n" +
                                      "verifications: []\n";
                    await FileHelper.WriteAllTextAsync(configPath, basicConfig);
                }

                // Set environment variable for current session
                Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome);

                // Persist to shell for Mac users
                if (OperatingSystem.IsMacOS())
                    try
                    {
                        var zshrc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            ".zshrc");
                        var exportLine = $"export TENDRIL_HOME=\"{tendrilHome}\"";
                        if (File.Exists(zshrc))
                        {
                            var content = await FileHelper.ReadAllTextAsync(zshrc);
                            if (!content.Contains(exportLine))
                                await File.AppendAllLinesAsync(zshrc, new[] { "", "# Tendril Home", exportLine });
                        }
                    }
                    catch
                    {
                        /* Best effort */
                    }

                // Mark onboarding complete (this reloads config from the file we just wrote)
                config.CompleteOnboarding(tendrilHome);

                // Add pending verification definitions to global config
                var pendingDefinitions = config.GetPendingVerificationDefinitions();
                if (pendingDefinitions != null)
                    foreach (var def in pendingDefinitions)
                        if (!config.Settings.Verifications.Any(v => v.Name == def.Name))
                            config.Settings.Verifications.Add(def);

                // Add pending project if one was configured
                var pendingProject = config.GetPendingProject();
                if (pendingProject != null)
                {
                    config.Settings.Projects.Add(pendingProject);
                    config.SaveSettings();
                }

                // Navigate to SetupApp
                navigator.Navigate<SetupApp>();
            }
            catch (Exception ex)
            {
                error.Set($"Failed to complete setup: {ex.Message}");
                isProcessing.Set(false);
            }
        }

        return Layout.Vertical()
               | Text.H2("Ready to Go!")
               | Text.Markdown(
                   "We'll now:\n" +
                   "- Create the necessary folder structure\n" +
                   "- Set up your configuration file\n" +
                   "- Initialize Tendril with default settings\n\n" +
                   "Click 'Complete Setup' to finish.")
               | (error.Value != null ? Text.Danger(error.Value) : null!)
               | new Button("Complete Setup")
                   .Primary()
                   .Large()
                   .Icon(Icons.Check, Align.Right)
                   .Disabled(isProcessing.Value)
                   .Loading(isProcessing.Value)
                   .OnClick(async () => await OnComplete());
    }
}
