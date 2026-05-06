# Multiselect Listbox

An input field to select multiple options from a list. Users can choose multiple values from available options with optional search functionality.

## Retool

```toolscript
multiselectListbox1.value        // Currently selected values
multiselectListbox1.labels       // Display text for each item
multiselectListbox1.values       // Available options
multiselectListbox1.selectedItems // Selected item objects (read-only)
multiselectListbox1.setValue(["a", "b"])
multiselectListbox1.clearValue()
```

## Ivy

```csharp
// Multiple selection is automatically enabled when using a collection type as state
var selected = new State<string[]>(["a", "b"]);
var options = new Option[] {
    new("a", "Option A"),
    new("b", "Option B"),
    new("c", "Option C"),
};

selected.ToSelectInput(options).Variant(SelectInputs.List);
```

## Parameters

| Parameter                | Documentation                                            | Ivy                                                            |
|--------------------------|----------------------------------------------------------|----------------------------------------------------------------|
| `value`                  | Currently selected values (array)                        | `Value` property / state binding                               |
| `values`                 | Available options for selection                          | `Options` (IEnumerable&lt;IAnyOption&gt;)                      |
| `labels`                 | Display text for each item                               | Set via `Option` label constructor arg                         |
| `selectedItems`          | Read-only selected item objects                          | Not supported (derive from state)                              |
| `selectedIndexes`        | Read-only numeric indices of chosen items                | Not supported                                                  |
| `selectedLabels`         | Read-only display text of selected choices               | Not supported                                                  |
| `disabled`               | Toggles whether interaction is permitted                 | `Disabled` (bool)                                              |
| `required`               | Enforces mandatory selection                             | `Invalid` (string validation message)                          |
| `maxCount`               | Upper limit on selectable items                          | Not supported                                                  |
| `minCount`               | Lower limit on selectable items                          | Not supported                                                  |
| `disabledValues`         | Values unavailable for selection                         | Not supported                                                  |
| `searchMode`             | Search type: fuzzy, caseInsensitive, caseSensitive       | Not supported                                                  |
| `searchTerm`             | Filters displayed options                                | Not supported                                                  |
| `emptyMessage`           | Message shown when no value exists                       | `Placeholder` (string)                                         |
| `showActions`            | Displays footer with select/clear all options            | Not supported                                                  |
| `showSelectionIndicator` | Shows icon next to selected items                        | Built-in with `List` variant (checkboxes)                      |
| `labelPosition`          | Label placement: top or left                             | Not supported (use layout)                                     |
| `tooltipText`            | Hover helper text in Markdown                            | Not supported                                                  |
| `id`                     | Component identifier                                     | Not applicable (C# reference)                                  |
| **Methods**              |                                                          |                                                                |
| `clearValue()`           | Removes all selections                                   | Set state to empty array                                       |
| `resetValue()`           | Restores default value                                   | Set state to initial value                                     |
| `setValue(value)`        | Sets selections programmatically                         | Set state value                                                |
| `clearValidation()`      | Removes validation error messages                        | Set `Invalid` to null                                          |
| `focus()`                | Sets component focus                                     | Not supported                                                  |
| `setDisabled(disabled)`  | Toggles disabled state                                   | Set `Disabled` property                                        |
| `setHidden(hidden)`      | Toggles visibility                                       | `Visible` (bool)                                               |
| `scrollIntoView(options)`| Brings component into viewport                           | Not supported                                                  |
| **Events**               |                                                          |                                                                |
| Change                   | Triggered when selections are modified                   | `OnChange` (Func&lt;Event&lt;IInput, TValue&gt;, ValueTask&gt;) |
| Clear                    | Triggered when values are cleared                        | Not supported (use OnChange)                                   |
| —                        | —                                                        | `OnBlur` (Func&lt;Event&lt;IAnyInput&gt;, ValueTask&gt;)       |
