---
searchHints:
  - stepper
  - steps
  - wizard
  - progress
  - sequence
  - multi-step
---

# Stepper

<Ingress>
Display a step-by-step progress indicator with visual feedback. Perfect for wizards, multi-step forms, and sequential workflows.
</Ingress>

The `Stepper` widget displays a horizontal sequence of steps with visual indicators showing the current position, completed steps, and upcoming steps. Each step can have a symbol, icon, label, and description.

## Basic Usage

Create a simple stepper with steps:

```csharp demo-below
new Stepper(
    null,
    1,
    new StepperItem("1", null, "Step 1", "First step"),
    new StepperItem("2", null, "Step 2", "Second step"),
    new StepperItem("3", null, "Step 3", "Third step")
)
```

## Configuration Options

### Step Items

Each step is defined using `StepperItem` with optional symbol, icon, label, and description:

```csharp demo-tabs
Layout.Vertical().Gap(4)
| Text.H4("With Icons")
| new Stepper(
    null,
    1,
    new StepperItem("1", Icons.Check, "Completed", "This step is done"),
    new StepperItem("2", null, "Current", "This is the active step"),
    new StepperItem("3", null, "Upcoming", "This step is pending")
)
| Text.H4("With Symbols Only")
| new Stepper(
    null,
    0,
    new StepperItem("A"),
    new StepperItem("B"),
    new StepperItem("C")
)
```

### Handling Step Selection

Use the `OnSelect` event handler to respond when a step is clicked:

```csharp demo-tabs
public class StepperSelectionDemo : ViewBase
{
    public override object? Build()
    {
        var selectedIndex = UseState(0);
        
        ValueTask OnStepSelected(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
        
        return Layout.Vertical().Gap(4)
            | new Stepper(
                OnStepSelected,
                selectedIndex.Value,
                new StepperItem("1", null, "Company", "Setup company"),
                new StepperItem("2", null, "Raise", "Raise capital"),
                new StepperItem("3", null, "Deck", "Create pitch deck"),
                new StepperItem("4", null, "Founders", "Add founders")
            )
            | Text.Block($"Selected step: {selectedIndex.Value}");
    }
}
```

### Allow Forward Selection

By default, users can only select completed steps or the current step. Enable `AllowSelectForward` to allow clicking on future steps:

```csharp demo-tabs
public class StepperForwardSelectionDemo : ViewBase
{
    public override object? Build()
    {
        var selectedIndex = UseState(1);
        
        ValueTask OnStepSelected(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
        
        return Layout.Vertical().Gap(4)
            | Text.H4("Default (no forward selection)")
            | new Stepper(
                OnStepSelected,
                selectedIndex.Value,
                new StepperItem("1", null, "Step 1"),
                new StepperItem("2", null, "Step 2"),
                new StepperItem("3", null, "Step 3")
            )
            | Text.H4("With AllowSelectForward")
            | new Stepper(
                OnStepSelected,
                selectedIndex.Value,
                new StepperItem("1", null, "Step 1"),
                new StepperItem("2", null, "Step 2"),
                new StepperItem("3", null, "Step 3")
            ).AllowSelectForward();
    }
}
```

### Dynamic Step States

Update step icons and states based on the current selection:

```csharp demo-tabs
public class StepperDynamicStatesDemo : ViewBase
{
    public override object? Build()
    {
        var selectedIndex = UseState(0);
        
        StepperItem[] GetItems(int currentIndex) =>
        [
            new("1", currentIndex > 0 ? Icons.Check : null, "Company", "Setup company"),
            new("2", currentIndex > 1 ? Icons.Check : null, "Raise", "Raise capital"),
            new("3", currentIndex > 2 ? Icons.Check : null, "Deck", "Create pitch deck"),
            new("4", null, "Founders", "Add founders"),
        ];
        
        ValueTask OnStepSelected(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
        
        return Layout.Vertical().Gap(4)
            | new Stepper(
                OnStepSelected,
                selectedIndex.Value,
                GetItems(selectedIndex.Value)
            )
            | Layout.Horizontal().Gap(2)
                | new Button("Previous").Link().HandleClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value - 1, 0, 3));
                })
                | new Button("Next").Link().HandleClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value + 1, 0, 3));
                });
    }
}
```

<WidgetDocs Type="Ivy.Stepper" ExtensionTypes="Ivy.StepperExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Stepper.cs"/>
