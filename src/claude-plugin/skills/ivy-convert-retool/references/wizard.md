# Wizard

A container for a series of steps with multiple branches and outcomes. Supports running queries on step change or reset, virtual steps that display a loading screen, and setting the initial step dynamically.

## Retool

```toolscript
// Wizard groups components into branching step sequences.
// Configure steps, branches, and queries in the Inspector.

// Navigate to a specific step:
wizard1.setStepId("step_confirmation");

// Reset to the initial step:
wizard1.reset();

// Set focus:
wizard1.focus();

// Properties:
wizard1.currentStep;    // Current step ID
wizard1.initialStep;    // First step ID
wizard1.steps;          // List of step names
```

## Ivy

```csharp
// Ivy closest equivalent: Stepper widget with state-driven branching
var (currentStep, setCurrentStep) = UseState(0);
var (branch, setBranch) = UseState("main");

new Stepper(
    onSelect: e => setCurrentStep(e.Value),
    selectedIndex: currentStep,
    items: new[]
    {
        new StepperItem(icon: Icons.User, label: "Account"),
        new StepperItem(icon: Icons.Settings, label: "Preferences"),
        new StepperItem(icon: Icons.Check, label: "Confirm"),
    }
);

// Branching logic via state:
if (currentStep == 0)
{
    new Text("Create your account");
    new Button("Next").OnClick(() => setCurrentStep(1));
}
else if (currentStep == 1)
{
    new Text("Choose your path");
    new Button("Standard").OnClick(() => { setBranch("standard"); setCurrentStep(2); });
    new Button("Advanced").OnClick(() => { setBranch("advanced"); setCurrentStep(2); });
}
else if (currentStep == 2 && branch == "standard")
{
    new Text("Standard confirmation");
}
else if (currentStep == 2 && branch == "advanced")
{
    new Text("Advanced confirmation");
}
```

## Parameters

| Parameter                       | Documentation                                         | Ivy                                                     |
|---------------------------------|-------------------------------------------------------|---------------------------------------------------------|
| currentStep                     | The current step ID                                   | `SelectedIndex` (int?) on Stepper                       |
| initialStep                     | The first step ID                                     | Initial `UseState` value                                |
| customInitialStepName           | Custom name for first step                            | `StepperItem(label: "...")` parameter                   |
| steps                           | List of step names                                    | `Items` (StepperItem[])                                 |
| hidden                          | Whether the component is hidden                       | `Visible` (bool)                                        |
| hideResetButton                 | Whether to hide the reset button                      | Not supported (build custom reset button)               |
| scroll                          | Whether to use scroll navigation                      | Not supported                                           |
| onStepChange                    | Query to run on step change                           | `OnSelect` event + `UseEffect`                          |
| onReset                         | Query to run on reset                                 | Custom button with `UseEffect`                          |
| spinWhenChildrenAreFetching     | Loading state when children fetch data                | Not supported                                           |
| margin                          | The amount of margin outside                          | Not supported                                           |
| style                           | Custom style options                                  | Not supported                                           |
| isHiddenOnMobile                | Whether to hide on mobile                             | Not supported                                           |
| isHiddenOnDesktop               | Whether to hide on desktop                            | Not supported                                           |
| maintainSpaceWhenHidden         | Whether to maintain space when hidden                 | Not supported                                           |
| showInEditor                    | Whether to show in editor when hidden                 | Not supported                                           |
| branching                       | Multiple branch paths and outcomes                    | State-driven conditionals (manual implementation)       |
| virtual steps (loading screen)  | Steps that show a loading state                       | Not supported                                           |
| AllowSelectForward              | Not available (linear + branches)                     | `AllowSelectForward` (bool)                             |
| Width                           | Not applicable (drag-to-resize)                       | `Width` (Size)                                          |
| Height                          | Not applicable (drag-to-resize)                       | `Height` (Size)                                         |
