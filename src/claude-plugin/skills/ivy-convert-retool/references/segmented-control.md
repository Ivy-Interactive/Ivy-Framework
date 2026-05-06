# Segmented Control

A segmented button to select an option from a list. Displays a horizontal set of mutually exclusive options as clickable segments, allowing users to pick one value at a time.

## Retool

```toolscript
// Basic segmented control with labels and values
segmentedControl1.setValue("Option 2");

// Reading selected value
const selected = segmentedControl1.value;
const selectedLabel = segmentedControl1.selectedLabel;
const selectedIndex = segmentedControl1.selectedIndex;

// Programmatic control
segmentedControl1.clearValue();
segmentedControl1.resetValue();
segmentedControl1.setDisabled(true);
segmentedControl1.setHidden(false);
```

## Ivy

```csharp
public class SegmentedControlDemo : ViewBase
{
    public override object? Build()
    {
        var selected = UseState("Option 1");
        var options = new[] { "Option 1", "Option 2", "Option 3" }.ToOptions();

        return selected.ToSelectInput(options)
                       .Variant(SelectInputs.Toggle)
                       .WithField()
                       .Label("Pick an option");
    }
}
```

## Parameters

| Parameter         | Retool Documentation                                                                 | Ivy                                                                                          |
|-------------------|--------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| value             | The current selected value. Read-only.                                               | `UseState<T>` bound to the `SelectInput`                                                     |
| values            | A list of possible values.                                                           | `IEnumerable<IAnyOption>` passed via `.ToOptions()` or constructor                           |
| labels            | A list of labels for each item. Falls back to value if no label is provided.         | Labels set via `Option<T>(value, label)` or `.ToOptions()` which uses value as label         |
| disabled          | Whether the entire control is disabled.                                              | `.Disabled(true)`                                                                            |
| disabledByIndex   | A list of per-item disabled states, by index.                                        | Not supported                                                                                |
| selectedIndex     | The selected value by index. Read-only.                                              | Not supported (use state value directly)                                                     |
| selectedItem      | The selected item object. Read-only.                                                 | Not supported (use state value directly)                                                     |
| selectedLabel     | The label of the selected value. Read-only.                                          | Not supported (use state value directly)                                                     |
| iconByIndex       | A list of icons for each item, by index.                                             | Not supported                                                                                |
| iconPositionByIndex | A list of icon positions for each item relative to labels.                         | Not supported                                                                                |
| required          | Whether a value is required to be selected.                                          | Not supported (handle via validation logic)                                                  |
| tooltipText       | Tooltip text displayed next to the label on hover.                                   | `.WithField().Label()` for label; use `Tooltip` widget for hover text                       |
| labelPosition     | Position of the label relative to the input (`top` or `left`).                       | Handled by `.WithField()` layout                                                            |
| placeholder       | Placeholder text when no value is selected.                                          | `.Placeholder("...")`                                                                       |
| style             | Custom style options.                                                                | Not supported (use theming)                                                                  |
| margin            | Margin around the component (`Normal` or `None`).                                    | Not supported (use layout spacing)                                                           |
| hidden            | Whether the component is hidden.                                                     | `.Visible(false)` or conditional rendering                                                   |
| events (Change)   | Fires when the value changes.                                                        | `OnChange` event handler or state setter callback                                            |
| clearValue()      | Clears the current value.                                                            | `state.Set(default)` or `state.Set("")`                                                      |
| resetValue()      | Resets to the default value.                                                         | Re-set state to initial value manually                                                       |
| setValue()        | Sets the current value programmatically.                                             | `state.Set(value)`                                                                           |
| setDisabled()     | Toggles disabled state programmatically.                                             | Bind `.Disabled()` to a `UseState<bool>`                                                     |
| setHidden()       | Toggles visibility programmatically.                                                 | Conditional rendering or bind `.Visible()` to state                                          |
| focus()           | Sets focus on the component.                                                         | Not supported                                                                                |
| invalid           | Validation error message.                                                            | `.Invalid("error message")`                                                                  |
