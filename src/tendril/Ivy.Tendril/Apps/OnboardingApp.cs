using Ivy.Tendril.Apps.Onboarding;

namespace Ivy.Tendril.Apps;

#if DEBUG
[App(title: "Onboarding", icon: Icons.Rocket, group: ["Debug"], isVisible: true, order: MenuOrder.Onboarding)]
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

    private static object GetStepViews(
        IState<int> stepperIndex,
        IState<Dictionary<string, bool>?> checkResults,
        IState<Dictionary<string, bool?>?> healthResults)
    {
        return stepperIndex.Value switch
        {
            0 => new WelcomeStepView(stepperIndex),
            1 => new SoftwareCheckStepView(stepperIndex, checkResults, healthResults),
            2 => new CodingAgentStepView(stepperIndex, checkResults.Value ?? new Dictionary<string, bool>(), healthResults.Value),
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
        var healthResults = UseState<Dictionary<string, bool?>?>(null);
        var steps = GetSteps(stepperIndex.Value);

        return Layout.TopCenter() |
               (Layout.Vertical().Margin(0, 20).Width(150)
                | new Image("/tendril/assets/Tendril.svg").Width(Size.Units(20)).Height(Size.Auto())
                | new Stepper(OnSelect, stepperIndex.Value, steps).Width(Size.Full())
                | GetStepViews(stepperIndex, checkResults, healthResults)
               );

        ValueTask OnSelect(Event<Stepper, int> e)
        {
            stepperIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}
