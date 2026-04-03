using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(isVisible: false, icon: Icons.Rocket, path: ["Hidden"])]
public class OnboardingApp : ViewBase
{
    private StepperItem[] GetSteps(int selectedIndex) =>
    [
        new("1", selectedIndex > 0 ? Icons.Check : null, "Welcome"),
        new("2", selectedIndex > 1 ? Icons.Check : null, "Tendril Data"),
        new("3", selectedIndex > 2 ? Icons.Check : null, "Repositories"),
        new("4", selectedIndex > 3 ? Icons.Check : null, "Complete")
    ];

    private object GetStepViews(IState<int> stepperIndex) => stepperIndex.Value switch
    {
        0 => new WelcomeStepView(stepperIndex),
        1 => new TendrilDataLocationStepView(stepperIndex),
        2 => new ReposLocationStepView(stepperIndex),
        3 => new CompleteStepView(stepperIndex),
        _ => throw new ArgumentOutOfRangeException()
    };

    public override object? Build()
    {
        var stepperIndex = UseState(0);
        var steps = GetSteps(stepperIndex.Value);

        return Layout.TopCenter() |
               (Layout.Vertical().Margin(0, 20).Width(150)
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
                   "Tendril helps you manage and execute plans for your development projects.\n\n" +
                   "To get started, we need to set up a few things:\n" +
                   "- Where to store your Tendril data\n" +
                   "- Create necessary folders and configuration\n\n" +
                   "Let's begin!")
               | new Button("Get Started").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .HandleClick(stepperIndex.Incr);
    }
}

public record TendrilDataLocationDetails
{
    [Required]
    [Display(Name = "Where would you like to store Tendril data?")]
    public string? TendrilHome { get; set; }
}

public class TendrilDataLocationStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var config = UseService<ConfigService>();
        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".tendril"
        );
        var details = UseState(new TendrilDataLocationDetails { TendrilHome = defaultPath });
        var error = UseState<string?>(null);

        return Layout.Vertical()
               | Text.H2("Tendril Data Location")
               | Text.Muted("This folder will store your plans, inbox, trash, and other Tendril data.")
               | (error.Value != null ? new Alert(error.Value).Destructive() : null!)
               | details.ToForm().Large()
                   .SubmitBuilder((saving) => new Button("Next").Icon(Icons.ArrowRight, Align.Right).Disabled(saving))
                   .HandleSubmit(OnSubmit)
            ;

        Task OnSubmit(TendrilDataLocationDetails? details)
        {
            if (details?.TendrilHome == null)
            {
                error.Set("Please provide a valid path");
                return Task.CompletedTask;
            }

            try
            {
                var tendrilHome = details.TendrilHome;

                // Expand environment variables and ~
                tendrilHome = Environment.ExpandEnvironmentVariables(tendrilHome);
                if (tendrilHome.StartsWith("~/") || tendrilHome.StartsWith("~\\"))
                {
                    tendrilHome = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        tendrilHome.Substring(2)
                    );
                }

                // Validate path
                if (!Path.IsPathRooted(tendrilHome))
                {
                    tendrilHome = Path.GetFullPath(tendrilHome);
                }

                // Store in config for next step
                config.SetPendingTendrilHome(tendrilHome);
                error.Set(null);
                stepperIndex.Incr();
            }
            catch (Exception ex)
            {
                error.Set($"Invalid path: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}

public record ReposLocationDetails
{
    [Display(Name = "Where are your repositories located?")]
    public string? ReposHome { get; set; }
}

public class ReposLocationStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var config = UseService<ConfigService>();
        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "repos"
        );
        var details = UseState(new ReposLocationDetails { ReposHome = defaultPath });
        var error = UseState<string?>(null);

        return Layout.Vertical()
               | Text.H2("Repositories Location (Optional)")
               | Text.Markdown(
                   "Specify the base folder where your code repositories are located.\n\n" +
                   "**This is optional.** If you keep all your repos in one place, setting this helps with project setup. " +
                   "Otherwise, you can skip this and specify individual repo paths when configuring projects.\n\n" +
                   "**Examples:**\n" +
                   "- `C:\\Users\\YourName\\repos` (Windows)\n" +
                   "- `/home/yourname/repos` (Linux)\n" +
                   "- `~/repos` (Any platform)")
               | (error.Value != null ? new Alert(error.Value).Destructive() : null!)
               | details.ToForm().Large()
                   .SubmitBuilder((saving) =>
                       Layout.Horizontal().Gap(2)
                       | new Button("Skip").Outline().HandleClick(() => { config.SetPendingReposHome(""); stepperIndex.Incr(); })
                       | new Button("Next").Primary().Icon(Icons.ArrowRight, Align.Right).Disabled(saving).Type(ButtonType.Submit))
                   .HandleSubmit(OnSubmit)
            ;

        Task OnSubmit(ReposLocationDetails? details)
        {
            try
            {
                var reposHome = details?.ReposHome ?? "";

                // Allow empty - user can configure later
                if (!string.IsNullOrWhiteSpace(reposHome))
                {
                    // Expand environment variables and ~
                    reposHome = Environment.ExpandEnvironmentVariables(reposHome);
                    if (reposHome.StartsWith("~/") || reposHome.StartsWith("~\\"))
                    {
                        reposHome = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            reposHome.Substring(2)
                        );
                    }

                    // Validate path if provided
                    if (!Path.IsPathRooted(reposHome))
                    {
                        reposHome = Path.GetFullPath(reposHome);
                    }

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(reposHome))
                    {
                        Directory.CreateDirectory(reposHome);
                    }
                }

                // Store in config for next step
                config.SetPendingReposHome(reposHome);
                error.Set(null);
                stepperIndex.Incr();
            }
            catch (Exception ex)
            {
                error.Set($"Invalid path: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}

public class CompleteStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var config = UseService<ConfigService>();
        var navigator = UseNavigation();
        var isProcessing = UseState(false);
        var error = UseState<string?>(null);

        async Task OnComplete()
        {
            isProcessing.Set(true);
            error.Set(null);

            try
            {
                var tendrilHome = config.GetPendingTendrilHome();
                var reposHome = config.GetPendingReposHome() ?? "";

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

                // Copy example.config.yaml to config.yaml
                var exampleConfigPath = Path.Combine(AppContext.BaseDirectory, "example.config.yaml");
                var configPath = Path.Combine(AppContext.BaseDirectory, "config.yaml");

                if (File.Exists(exampleConfigPath))
                {
                    var exampleContent = await File.ReadAllTextAsync(exampleConfigPath);

                    // Update the config with the tendrilData and reposHome paths
                    var configContent = $"tendrilData: {tendrilHome}\n";
                    if (!string.IsNullOrEmpty(reposHome))
                    {
                        configContent += $"reposHome: {reposHome}\n";
                    }
                    configContent += exampleContent;

                    await File.WriteAllTextAsync(configPath, configContent);
                }
                else
                {
                    // Create a basic config.yaml
                    var basicConfig = $"tendrilData: {tendrilHome}\n";
                    if (!string.IsNullOrEmpty(reposHome))
                    {
                        basicConfig += $"reposHome: {reposHome}\n";
                    }
                    basicConfig += $"planFolder: {Path.Combine(tendrilHome, "Plans")}\n" +
                                  "agentCommand: claude\n" +
                                  "jobTimeout: 30\n" +
                                  "staleOutputTimeout: 10\n";
                    await File.WriteAllTextAsync(configPath, basicConfig);
                }

                // Set environment variables for current session
                Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome);
                if (!string.IsNullOrEmpty(reposHome))
                {
                    Environment.SetEnvironmentVariable("REPOS_HOME", reposHome);
                }

                // Mark onboarding complete
                config.CompleteOnboarding(tendrilHome, reposHome);

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
               | (error.Value != null ? new Alert(error.Value).Destructive() : null!)
               | new Button("Complete Setup")
                   .Primary()
                   .Large()
                   .Icon(Icons.Check, Align.Right)
                   .Disabled(isProcessing.Value)
                   .Loading(isProcessing.Value)
                   .HandleClick(OnComplete);
    }
}
