# Checkbox Group

A component that allows users to toggle multiple boolean values through a collection of checkboxes, enabling multiple selections from a predefined set of options. Supports layouts like multi-column, single-column, wrap, and tree hierarchy.

## Retool

```toolscript
// Define options and default values
checkboxGroup1.values = ["option1", "option2", "option3"]
checkboxGroup1.labels = ["Option 1", "Option 2", "Option 3"]

// Read selected values
const selected = checkboxGroup1.value

// Programmatic control
checkboxGroup1.setValue(["option1", "option3"])
checkboxGroup1.clearValue()
checkboxGroup1.resetValue()
checkboxGroup1.setDisabled(true)
checkboxGroup1.setHidden(false)
```

## Ivy

In Ivy, the `SelectInput` widget with `variant: SelectInputs.List` and `selectMany: true` provides equivalent checkbox group functionality.

```csharp
var selected = UseState(new[] { "option1" });

selected.ToSelectInput(new[]
    {
        new Option("option1", "Option 1"),
        new Option("option2", "Option 2"),
        new Option("option3", "Option 3")
    })
    .Variant(SelectInputs.List);
```

## Parameters

| Parameter             | Retool Documentation                                                               | Ivy                                                                |
|-----------------------|------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| `value`               | The current selected values (array)                                                | `Value` property on `SelectInput<TValue>` / bound via `IAnyState`  |
| `values`              | A list of possible values from which to select                                     | `Options` — provided as `IEnumerable<IAnyOption>`                  |
| `labels`              | Display labels for each checkbox item                                              | Label is set per `Option` object (e.g. `new Option(value, label)`) |
| `disabled`            | Whether input/interaction is disabled                                              | `Disabled` property / `.Disabled(bool)`                            |
| `required`            | Whether at least one selection is required                                         | `Invalid` — set validation message manually                        |
| `groupLayout`         | Layout mode: `multiColumn`, `singleColumn`, `wrap`, `tree`                         | Not supported (layout is list-only)                                |
| `heightType`          | Height behavior: `auto`, `fixed`, `fill`                                           | `Height` — set via `Size`                                          |
| `labelPosition`       | Label alignment: `top`, `left`                                                     | `.Label()` via `.WithField()` (field-level label positioning)      |
| `maxCount`            | Maximum number of selectable items                                                 | Not supported                                                      |
| `minCount`            | Minimum number of required selections                                              | Not supported                                                      |
| `checkStrictly`       | Independent parent/child checking in tree mode                                     | Not supported                                                      |
| `disabledByIndex`     | List of disabled status per item by index                                          | Not supported (only global `Disabled`)                             |
| `disabledValues`      | List of specific values not available for selection                                 | Not supported                                                      |
| `hiddenByIndex`       | List of hidden status per item by index                                            | Not supported                                                      |
| `tooltipByIndex`      | List of tooltips for each item by index                                            | Not supported                                                      |
| `tooltipText`         | Tooltip text on hover for the whole component                                      | Not supported                                                      |
| `captionByIndex`      | List of captions for each item by index                                            | Not supported                                                      |
| `placeholder`         | Placeholder text when no value is selected                                         | `Placeholder` property / `.Placeholder(string)`                    |
| `selectedIndexes`     | List of selected values by index                                                   | Not supported (selection is value-based)                           |
| `selectedLabels`      | Labels for the selected values                                                     | Not supported                                                      |
| `selectedItems`       | List of selected item objects                                                      | Not supported (use `Value` to get selected values)                 |
| `margin`              | Component margin: Normal (`4px 8px`) or None (`0`)                                 | Not supported                                                      |
| `minColumnWidth`      | Minimum column width in multi-column layout                                        | Not supported                                                      |
| `isHiddenOnDesktop`   | Whether hidden in desktop layout                                                   | `Visible` property                                                 |
| `isHiddenOnMobile`    | Whether hidden in mobile layout                                                    | `Visible` property (no desktop/mobile distinction)                 |
| `style`               | Custom style options                                                               | Not supported                                                      |
| `delimiter`           | Delimiter used in `checkedPathStrings`                                              | `Separator` property                                               |
| **Events**            |                                                                                    |                                                                    |
| `Change`              | Triggered when selections are modified                                             | `OnChange` event handler                                           |
| **Methods**           |                                                                                    |                                                                    |
| `setValue(value)`     | Set the current selected values                                                    | Set via bound `IAnyState`                                          |
| `clearValue()`        | Clear all selections                                                               | Set state to empty array                                           |
| `resetValue()`        | Reset to default value                                                             | Not supported (manage default in user code)                        |
| `focus()`             | Set focus on the component                                                         | Not supported                                                      |
| `blur()`              | Remove focus from the component                                                    | `OnBlur` event exists, but no programmatic blur method             |
| `setDisabled(bool)`   | Toggle disabled state                                                              | `.Disabled(bool)` fluent setter                                    |
| `setHidden(bool)`     | Toggle visibility                                                                  | `Visible` property                                                 |
| `clearValidation()`   | Clear validation messages                                                          | `.Invalid(null)` to clear                                          |
| `scrollIntoView()`    | Scroll component into visible area                                                 | Not supported                                                      |
