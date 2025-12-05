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

The `Stepper` constructor takes three main parameters:

```mermaid
graph LR
    A[Stepper] --> B[onSelect<br/>Event Handler]
    A --> C[selectedIndex<br/>Active Step Index]
    A --> D[items<br/>StepperItem Array]
    B --> B1["null = disabled<br/>handler = enabled"]
    C --> C1["Zero-based index<br/>Controls highlighting"]
    D --> D1["Symbol, Icon<br/>Label, Description"]
```

## Configuration Options

### Allow Forward Selection

Enable `AllowSelectForward` to allow clicking on future steps:

```csharp demo-tabs
public class StepperForwardSelectionDemo : ViewBase
{
    public override object? Build()
    {
        var selectedIndex = UseState(1);
        
        var items = new[]
        {
            new StepperItem("1", null, "Step 1"),
            new StepperItem("2", null, "Step 2"),
            new StepperItem("3", null, "Step 3")
        };
        
        return new Stepper(OnSelect, selectedIndex.Value, items).AllowSelectForward();
        
        ValueTask OnSelect(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}
```

### Dynamic Step States

Update step icons and states based on the current selection:

```csharp demo-tabs
public class StepperDynamicStatesDemo : ViewBase
{
    StepperItem[] GetItems(int selectedIndex) =>
    [
        new("1", selectedIndex > 0 ? Icons.Check : null, "Company", "Setup company"),
        new("2", selectedIndex > 1 ? Icons.Check : null, "Raise", "Raise capital"),
        new("3", null, "Founders", "Add founders"),
    ];
    
    public override object? Build()
    {
        var selectedIndex = UseState(0);
        
        var items = GetItems(selectedIndex.Value);
        
        return Layout.Vertical()
            | new Stepper(OnSelect, selectedIndex.Value, items)
            | (Layout.Horizontal().Gap(0)
                | new Button("Previous").Link().HandleClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value - 1, 0, items.Length - 1));
                })
                | new Button("Next").Link().HandleClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value + 1, 0, items.Length - 1));
                })
            );
        
        ValueTask OnSelect(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}
```

<WidgetDocs Type="Ivy.Stepper" ExtensionTypes="Ivy.StepperExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Stepper.cs"/>
