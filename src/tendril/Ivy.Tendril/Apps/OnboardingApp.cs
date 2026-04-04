using Ivy;
using Ivy.Tendril.Services;
using System.ComponentModel.DataAnnotations;

namespace Ivy.Tendril.Apps;

#if DEBUG
[App(title: "Onboarding", icon: Icons.Rocket, group: new[] { "Debug" }, isVisible: true, order: 100)]
#else
[App(isVisible: false, icon: Icons.Rocket)]
#endif
public class OnboardingApp : ViewBase
{
    private StepperItem[] GetSteps(int selectedIndex) =>
    [
        new("1", selectedIndex > 0 ? Icons.Check : null, "Welcome"),
        new("2", selectedIndex > 1 ? Icons.Check : null, "Tendril Home"),
        new("3", selectedIndex > 2 ? Icons.Check : null, "Complete")
    ];

    private object GetStepViews(IState<int> stepperIndex) => stepperIndex.Value switch
    {
        0 => new WelcomeStepView(stepperIndex),
        1 => new TendrilHomeStepView(stepperIndex),
        2 => new CompleteStepView(stepperIndex),
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
                   .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1));
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
        var config = UseService<ConfigService>();

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

public class CompleteStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var isProcessing = UseState(false);
        var error = UseState<string?>(null);
        var config = UseService<ConfigService>();
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

                // Copy example.config.yaml to tendrilHome/config.yaml
                var exampleConfigPath = Path.Combine(System.AppContext.BaseDirectory, "example.config.yaml");
                var configPath = Path.Combine(tendrilHome, "config.yaml");

                if (File.Exists(exampleConfigPath))
                {
                    var exampleContent = await File.ReadAllTextAsync(exampleConfigPath);
                    await File.WriteAllTextAsync(configPath, exampleContent);
                }
                else
                {
                    // Create a basic config.yaml
                    var basicConfig = "agentCommand: claude\n" +
                                      "jobTimeout: 30\n" +
                                      "staleOutputTimeout: 10\n" +
                                      "projects: []\n" +
                                      "verifications: []\n";
                    await File.WriteAllTextAsync(configPath, basicConfig);
                }

                // Set environment variable for current session
                Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome);

                // Mark onboarding complete
                config.CompleteOnboarding(tendrilHome);

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
