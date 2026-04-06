using Ivy.Tendril.Services;
using System.ComponentModel.DataAnnotations;

namespace Ivy.Tendril.Apps;

#if DEBUG
[App(title: "Onboarding", icon: Icons.Rocket, group: new[] { "Debug" }, isVisible: true, order: MenuOrder.Onboarding)]
#else
[App(icon: Icons.Rocket, isVisible: false, order: MenuOrder.Onboarding)]
#endif
public class OnboardingApp : ViewBase
{
    private StepperItem[] GetSteps(int selectedIndex) =>
    [
        new("1", selectedIndex > 0 ? Icons.Check : null, "Welcome"),
        new("2", selectedIndex > 1 ? Icons.Check : null, "Software Check"),
        new("3", selectedIndex > 2 ? Icons.Check : null, "Tendril Home"),
        new("4", selectedIndex > 3 ? Icons.Check : null, "Project Setup"),
        new("5", selectedIndex > 4 ? Icons.Check : null, "Complete")
    ];

    private object GetStepViews(IState<int> stepperIndex) => stepperIndex.Value switch
    {
        0 => new WelcomeStepView(stepperIndex),
        1 => new SoftwareCheckStepView(stepperIndex),
        2 => new TendrilHomeStepView(stepperIndex),
        3 => new ProjectSetupStepView(stepperIndex),
        4 => new CompleteStepView(stepperIndex),
        _ => throw new ArgumentOutOfRangeException()
    };

    public override object? Build()
    {
        var stepperIndex = UseState(0);
        var steps = GetSteps(stepperIndex.Value);

        return Layout.TopCenter() |
               (Layout.Vertical().Margin(0, 20).Width(150)
                | new Image("/tendril/assets/Tendril.svg").Width(Size.Units(20)).Height(Size.Auto())
                | new Stepper(OnSelect, stepperIndex.Value, steps).Width(Size.Full())
                | GetStepViews(stepperIndex)
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
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H1("Welcome to Tendril")
               | Text.Markdown(
                   "Ivy Tendril is a coding orchestrator.\n\n" +
                   "To get started, we need to set up a few things:\n" +
                   "- Where to store your Tendril data\n" +
                   "- Create necessary folders and configuration\n\n" +
                   "Let's begin!")
               | new Button("Get Started").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1));
    }
}

public class SoftwareCheckStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var checkResults = UseState<Dictionary<string, bool>?>(null);
        var isChecking = UseState(false);

        async Task CheckSoftware()
        {
            isChecking.Set(true);
            var results = new Dictionary<string, bool>();

            results["gh"] = await CheckCommand("gh", "--version");
            results["claude"] = await CheckCommand("claude", "--version");
            results["git"] = await CheckCommand("git", "--version");
            results["powershell"] = await CheckCommand("pwsh", "-Version")
                                    || await CheckCommand("powershell", "-Version");
            results["pandoc"] = await CheckCommand("pandoc", "--version");

            checkResults.Set(results);
            isChecking.Set(false);
        }

        var allRequiredPassed = checkResults.Value != null
            && checkResults.Value["gh"]
            && checkResults.Value["claude"]
            && checkResults.Value["git"]
            && checkResults.Value["powershell"];

        return Layout.Vertical()
               | Text.H2("Required Software")
               | Text.Markdown(
                   "Tendril requires the following software to be installed:\n\n" +
                   "- **Claude CLI** - For AI agent orchestration\n" +
                   "- **GitHub CLI** - For PR creation and GitHub integration\n" +
                   "- **Git** - For version control\n" +
                   "- **PowerShell** - For running scripts and hooks\n\n" +
                   "**Optional:**\n" +
                   "- **pandoc** - For PDF export functionality\n\n" +
                   "Click 'Check Software' to verify your system.")
               | (checkResults.Value != null
                   ? (Layout.Vertical()
                      | Text.H3("Results")
                      | (checkResults.Value["gh"]
                          ? Text.Success("\u2713 GitHub CLI is installed")
                          : Text.Danger("\u2717 GitHub CLI not found - Install from https://cli.github.com/"))
                      | (checkResults.Value["claude"]
                          ? Text.Success("\u2713 Claude CLI is installed")
                          : Text.Danger("\u2717 Claude CLI not found - Install from https://docs.anthropic.com/en/docs/claude-code"))
                      | (checkResults.Value["git"]
                          ? Text.Success("\u2713 Git is installed")
                          : Text.Danger("\u2717 Git not found - Install from https://git-scm.com/downloads"))
                      | (checkResults.Value["powershell"]
                          ? Text.Success("\u2713 PowerShell is installed")
                          : Text.Danger("\u2717 PowerShell not found - Install PowerShell Core from https://github.com/PowerShell/PowerShell"))
                      | Text.H3("Optional")
                      | (checkResults.Value["pandoc"]
                          ? Text.Success("\u2713 Pandoc is installed")
                          : Text.Muted("\u24d8 Pandoc not found - Install from https://pandoc.org/installing.html for PDF export"))
                     )
                   : null!)
               | (checkResults.Value == null
                   ? new Button("Check Software")
                       .Primary()
                       .Large()
                       .Icon(Icons.CirclePlay, Align.Right)
                       .Loading(isChecking.Value)
                       .Disabled(isChecking.Value)
                       .OnClick(async () => await CheckSoftware())
                   : (allRequiredPassed
                       ? new Button("Continue")
                           .Primary()
                           .Large()
                           .Icon(Icons.ArrowRight, Align.Right)
                           .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                       : Layout.Vertical()
                         | Text.Warning("Please install missing required software before continuing.")
                         | (Layout.Horizontal().Gap(2)
                           | new Button("Check Again")
                               .Outline()
                               .Icon(Icons.CheckCheck, Align.Right)
                               .OnClick(async () => await CheckSoftware())
                           | new Button("Skip Anyway")
                               .Destructive()
                               .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                         )
                     )
                  );
    }

    private static async Task<bool> CheckCommand(string fileName, string arguments)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc?.WaitForExit();
                return proc?.ExitCode == 0;
            });
        }
        catch
        {
            return false;
        }
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
    public override object? Build()
    {
        var details = UseState(new TendrilHomeDetails
        {
            TendrilHome = Path.Combine(
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
                   .SubmitBuilder((saving) => new Button("Next").Icon(Icons.ArrowRight, Align.Right).Disabled(saving))
                   .OnSubmit(OnSubmit)
            ;

        Task OnSubmit(TendrilHomeDetails? details)
        {
            if (string.IsNullOrEmpty(details?.TendrilHome))
            {
                error.Set("Please provide a valid path");
                return Task.CompletedTask;
            }

            try
            {
                var tendrilHome = details.TendrilHome;

                // Expand environment variables (handles %VAR%)
                tendrilHome = Environment.ExpandEnvironmentVariables(tendrilHome);

                // Manual expansion for ~ and $HOME (Mac/Linux)
                if (tendrilHome.StartsWith("~"))
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    if (tendrilHome == "~") tendrilHome = home;
                    else if (tendrilHome.StartsWith("~/") || tendrilHome.StartsWith("~\\"))
                    {
                        tendrilHome = Path.Combine(home, tendrilHome.Substring(2));
                    }
                }
                else if (tendrilHome.StartsWith("$"))
                {
                    // Handle $HOME style expansion if missed by ExpandEnvironmentVariables
                    var match = System.Text.RegularExpressions.Regex.Match(tendrilHome, @"^\$([A-Za-z_][A-Za-z0-9_]*)");
                    if (match.Success)
                    {
                        var varName = match.Groups[1].Value;
                        var varValue = Environment.GetEnvironmentVariable(varName);
                        if (!string.IsNullOrEmpty(varValue))
                        {
                            tendrilHome = varValue + tendrilHome.Substring(match.Length);
                        }
                    }
                }

                // Normalize and root
                if (!Path.IsPathRooted(tendrilHome))
                {
                    tendrilHome = Path.GetFullPath(tendrilHome);
                }

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
    public override object? Build()
    {
        var config = UseService<IConfigService>();
        var projectName = UseState("");
        var projectColor = UseState("");
        var repoPaths = UseState(new List<string>());
        var newRepoPath = UseState("");
        var error = UseState<string?>(null);

        var colorOptions = new List<string>
        {
            "", "Red", "Orange", "Amber", "Yellow", "Lime", "Green", "Emerald", "Teal",
            "Cyan", "Sky", "Blue", "Indigo", "Violet", "Purple", "Fuchsia", "Pink", "Rose",
            "Slate", "Gray", "Zinc", "Neutral", "Stone"
        };

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
            | newRepoPath.ToTextInput("Repository path...").Width(Size.Grow())
            | new Button("Add").Outline().Small().OnClick(() =>
            {
                if (!string.IsNullOrWhiteSpace(newRepoPath.Value))
                {
                    var list = new List<string>(repoPaths.Value) { newRepoPath.Value };
                    repoPaths.Set(list);
                    newRepoPath.Set("");
                }
            });

        return Layout.Vertical().Gap(4)
               | Text.H2("Project Setup")
               | Text.Muted("Set up your first project. You can add more projects later in Settings.")
               | (error.Value != null ? Text.Danger(error.Value) : null!)
               | projectName.ToTextInput("Project name...").WithField().Label("Project Name")
               | projectColor.ToSelectInput(colorOptions).Nullable().WithField().Label("Color")
               | (Layout.Vertical().Gap(2)
                   | Text.Block("Repositories").Bold()
                   | Text.Muted("Add at least one repository path for this project.")
                   | reposLayout)
               | (Layout.Horizontal().Gap(2)
                   | new Button("Skip for now").Outline().Large()
                       .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
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

                           var project = new ProjectConfig
                           {
                               Name = projectName.Value.Trim(),
                               Color = projectColor.Value,
                               Repos = repoPaths.Value.Select(p => new RepoRef { Path = p, PrRule = "default" }).ToList()
                           };
                           config.SetPendingProject(project);
                           error.Set(null);
                           stepperIndex.Set(stepperIndex.Value + 1);
                       })
                 );
    }
}

public class CompleteStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
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
                {
                    projectDir = Path.GetDirectoryName(projectDir);
                }

                var exampleConfigPath = projectDir != null
                    ? Path.Combine(projectDir, "example.config.yaml")
                    : Path.Combine(System.AppContext.BaseDirectory, "example.config.yaml");

                var configPath = Path.Combine(tendrilHome, "config.yaml");

                if (File.Exists(exampleConfigPath))
                {
                    var exampleContent = await File.ReadAllTextAsync(exampleConfigPath);
                    await File.WriteAllTextAsync(configPath, exampleContent);
                }
                else if (!File.Exists(configPath))
                {
                    // Create a basic config.yaml only if it doesn't exist
                    var basicConfig = "agentCommand: claude\n" +
                                      "jobTimeout: 30\n" +
                                      "staleOutputTimeout: 10\n" +
                                      "projects: []\n" +
                                      "verifications: []\n";
                    await File.WriteAllTextAsync(configPath, basicConfig);
                }

                // Set environment variable for current session
                Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome);

                // Persist to shell for Mac users
                if (OperatingSystem.IsMacOS())
                {
                    try
                    {
                        var zshrc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zshrc");
                        var exportLine = $"export TENDRIL_HOME=\"{tendrilHome}\"";
                        if (File.Exists(zshrc))
                        {
                            var content = await File.ReadAllTextAsync(zshrc);
                            if (!content.Contains(exportLine))
                            {
                                await File.AppendAllLinesAsync(zshrc, new[] { "", "# Tendril Home", exportLine });
                            }
                        }
                    }
                    catch { /* Best effort */ }
                }

                // Mark onboarding complete (this reloads config from the file we just wrote)
                config.CompleteOnboarding(tendrilHome);

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
