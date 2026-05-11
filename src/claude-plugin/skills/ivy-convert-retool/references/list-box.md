# Listbox

An input field to select one or more options from a visible list. Displays all options inline (not as a dropdown) with support for search filtering, multi-select, and deselection.

## Retool

```toolscript
// Basic listbox with options and change handler
listbox1.setValue(["Option A", "Option C"]);

// Properties configured in the inspector:
// - Labels: ["Option A", "Option B", "Option C", "Option D"]
// - Values: ["a", "b", "c", "d"]
// - Allow deselect: true
// - Show selection indicator: true
// - Search mode: "fuzzy"

// Reading selected values
const selected = listbox1.value;          // ["a", "c"]
const label = listbox1.selectedLabel;     // "Option A"
const item = listbox1.selectedItem;       // { id: "1", name: "Option A" }
const index = listbox1.selectedIndex;     // [0, 2]

// Methods
listbox1.clearValue();
listbox1.resetValue();
listbox1.setDisabled(true);
listbox1.setHidden(false);
listbox1.focus();
listbox1.clearValidation();
listbox1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

The closest equivalent is `SelectInput<T>` with `Variant(SelectInputs.List)`, which renders options as an inline checkbox list rather than a dropdown.

```csharp
public class ListboxExample : ViewBase
{
    public override object? Build()
    {
        var options = new[] { "Option A", "Option B", "Option C", "Option D" }.ToOptions();

        // Multi-select listbox (collection type enables multi-select automatically)
        var selected = UseState<string[]>([]);

        return selected.ToSelectInput(options)
            .Variant(SelectInputs.List)
            .Placeholder("No items selected")
            .Disabled(false)
            .WithField()
            .Label("Choose options:");
    }
}
```

## Parameters

| Parameter              | Retool Documentation                                                        | Ivy                                                                             |
|------------------------|-----------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `value`                | A list of selected values.                                                  | `Value` property on `SelectInput<T>`, or bound via `UseState`                   |
| `labels`               | A list of labels for each item. Falls back to value if no label provided.   | Defined via `IAnyOption[]` passed to `.ToOptions()` with label/value pairs       |
| `disabled`             | Whether input, interaction, selection, or triggering is disabled.           | `.Disabled(bool)`                                                               |
| `disabledValues`       | A list of values not available for selection.                               | Not supported                                                                   |
| `allowDeselect`        | Whether to allow deselection of an item.                                    | `.Nullable(bool)`                                                               |
| `required`             | Whether a value is required to be selected.                                 | `.Invalid(string)` for validation messages                                      |
| `emptyMessage`         | The message to display if no value is set.                                  | `.Placeholder(string)`                                                          |
| `searchMode`           | The type of search to perform (fuzzy, case sensitive, case insensitive).    | Not supported                                                                   |
| `searchTerm`           | An optional string to filter the displayed options.                         | Not supported                                                                   |
| `selectedIndex`        | The selected values by index.                                               | Not supported (use `Value` directly)                                            |
| `selectedItem`         | The selected item object.                                                   | Not supported (use `Value` directly)                                            |
| `selectedLabel`        | The label of the selected value.                                            | Not supported (use `Value` directly)                                            |
| `showSelectionIndicator` | Whether to display an icon next to a selected item.                       | Not supported                                                                   |
| `labelPosition`        | The position of the label relative to the input (top or left).              | `.WithField().Label(string)` (layout controlled by field)                       |
| `tooltipText`          | The tooltip text to display next to the label on hover.                     | Not supported                                                                   |
| `margin`               | The amount of margin to render outside.                                     | Not supported (use layout widgets for spacing)                                  |
| `style`                | Custom style options.                                                       | Not supported (use theming)                                                     |
| `isHiddenOnDesktop`    | Whether to hide in the desktop layout.                                      | `Visible` property (no per-breakpoint control)                                  |
| `isHiddenOnMobile`     | Whether to hide in the mobile layout.                                       | `Visible` property (no per-breakpoint control)                                  |
| `maintainSpaceWhenHidden` | Whether to take up space on the canvas if hidden.                        | Not supported                                                                   |
| `showInEditor`         | Whether the component remains visible in the editor if hidden.              | Not supported                                                                   |
| `events` (Change)      | The value is changed.                                                       | `OnChange` event handler                                                        |
| `clearValue()`         | Clear the current values.                                                   | `state.Set(Array.Empty<T>())`                                                   |
| `resetValue()`         | Reset the current value to the default value.                               | `state.Set(initialValue)` (manual reset)                                        |
| `setValue()`           | Set the current value.                                                      | `state.Set(value)`                                                              |
| `setDisabled()`        | Toggle whether the input is disabled.                                       | `.Disabled(bool)`                                                               |
| `setHidden()`          | Toggle whether the component is visible.                                    | Not supported (no imperative hide/show)                                         |
| `focus()`              | Set focus on the component.                                                 | Not supported                                                                   |
| `clearValidation()`    | Clear the validation message from the input field.                          | `.Invalid(null)`                                                                |
| `scrollIntoView()`     | Scroll the canvas so that the component appears in view.                    | Not supported                                                                   |
