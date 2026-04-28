# Steps

A group of sequential step indicators that display progress through a multi-step workflow. Steps can be clicked to navigate between stages and can be linked to a container to create a stepped container experience.

## Retool

```toolscript
// Steps with labels and captions
steps1.labels = ["Company", "Raise", "Founders"]
steps1.captionByIndex = ["Setup company", "Raise capital", "Add founders"]
steps1.selectedIndex = 1
steps1.showStepNumbers = true
steps1.indicateCompletedSteps = true
steps1.orientation = "horizontal"

// Navigate programmatically
steps1.setValue("Step 2")

// Reset to default
steps1.resetValue()

// Hide/show
steps1.setHidden(false)
```

## Ivy

```csharp
public class StepsDemo : ViewBase
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
                | new Button("Previous").Link().OnClick(() =>
                {
                    selectedIndex.Set(Math.Clamp(selectedIndex.Value - 1, 0, items.Length - 1));
                })
                | new Button("Next").Link().OnClick(() =>
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

## Parameters

| Parameter | Documentation | Ivy |
|---|---|---|
| `labels` | A list of labels for each step. | `StepperItem` label (3rd constructor arg) |
| `captionByIndex` | A list of captions for each step, by index. | `StepperItem` description (4th constructor arg) |
| `selectedIndex` | The currently selected step index. | `selectedIndex` constructor parameter (`int?`) |
| `showStepNumbers` | Whether to display step numbers for each item. | `StepperItem` symbol (1st constructor arg) |
| `indicateCompletedSteps` | Whether to show check marks for completed steps. | Manual via `StepperItem` icon (e.g. `Icons.Check`) |
| `orientation` | Display orientation: `horizontal` or `vertical`. | Not supported (horizontal only) |
| `horizontalAlign` | Horizontal alignment: `left`, `center`, or `right`. | Not supported |
| `tooltipByIndex` | A list of tooltips for each step, by index. | Not supported |
| `hiddenByIndex` | A list of hidden states for each step, by index. | Not supported |
| `values` | A list of possible values to select from. | `StepperItem[]` items array |
| `value` | The current value. | `SelectedIndex` property |
| `selectedLabel` | The label of the selected step. | Not supported (derive from items) |
| `selectedItem` | The selected step object. | Not supported (derive from items) |
| `data` | Custom data attached to the component. | Not supported |
| `heightType` | Height behavior: `auto`, `fixed`, or `fill`. | `Height` property (`Size`) |
| `targetContainerId` | The ID of a linked container for navigation. | Not supported (compose with layouts manually) |
| `navigateContainer` | Whether a container is linked for navigation. | Not supported |
| `style` | Custom style options. | `Scale` property |
| `isHiddenOnDesktop` | Whether hidden on desktop layout. | Not supported |
| `isHiddenOnMobile` | Whether hidden on mobile layout. | Not supported |
| `maintainSpaceWhenHidden` | Whether to take up space when hidden. | Not supported |
| `margin` | The amount of margin outside the component. | Not supported |
| `itemMode` | Options config mode: `dynamic` (mapped) or `static` (manual). | Not supported (always code-defined) |
| `tooltipText` | Tooltip text displayed on hover near the label. | Not supported |
| `AllowSelectForward` | — | `AllowSelectForward` (Ivy-only: allow clicking future steps) |
| `OnSelect` | Events table (no named events listed). | `OnSelect` event handler (`Event<Stepper, int>`) |
| `resetValue()` | Reset value to default. | Not supported (manage state manually) |
| `setValue()` | Set the current value programmatically. | Not supported (set `selectedIndex` via state) |
| `setHidden()` | Toggle component visibility. | `Visible` property (`bool`) |
| `scrollIntoView()` | Scroll the component into the visible area. | Not supported |
