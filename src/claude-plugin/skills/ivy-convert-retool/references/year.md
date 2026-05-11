# Year

A dropdown menu and input field to select or enter a year. It is a preset version of Select, preconfigured with a list of year values.

## Retool

```toolscript
// Set the year value programmatically
year.setValue(2025);

// Clear the current selection
year.clearValue();

// Reset to default value
year.resetValue();

// Disable the year picker
year.setDisabled(true);

// Listen for changes via event handler
// Event: Change — triggers when selection updates
```

## Ivy

Ivy does not have a dedicated Year widget. The equivalent is achieved using a `SelectInput` populated with year options.

```csharp
var years = Enumerable.Range(2000, 50)
    .Select(y => y.ToString())
    .ToArray();

var selectedYear = UseState("2025");

return selectedYear.ToSelectInput(years.ToOptions())
    .Placeholder("Enter a value")
    .Variant(SelectInputs.Select)
    .WithField()
    .Label("Year");
```

## Parameters

| Parameter              | Documentation                                                        | Ivy                                                             |
|------------------------|----------------------------------------------------------------------|-----------------------------------------------------------------|
| value                  | The current selected year value                                      | `SelectInput.Value`                                             |
| disabled               | Whether interaction is disabled                                      | `.Disabled(true)`                                               |
| placeholder            | Text displayed when no selection exists                              | `.Placeholder("...")`                                           |
| allowCustomValue       | Whether to allow values outside the predefined list                  | Not supported                                                   |
| allowDeselect          | Whether to allow deselection of an item                              | `.Nullable(true)`                                               |
| required               | Whether a value is required                                          | Not supported (use validation logic)                            |
| readOnly               | Whether user input is read-only                                      | Not supported                                                   |
| selectedLabel          | The label of the selected value                                      | Not supported (access via state)                                |
| selectedItem           | The complete selected item                                           | `SelectInput.Value`                                             |
| selectedIndex          | The selected value by index                                          | Not supported                                                   |
| inputValue             | The default or most recently entered value                           | Not supported                                                   |
| loading                | Whether to display a loading indicator                               | Not supported (use `AsyncSelectInput` for async loading)        |
| showClear              | Whether to display a clear button                                    | Not supported                                                   |
| labelPosition          | Position of the label (top or left)                                  | `.WithField().Label("...")` (layout controlled via `WithField`) |
| tooltipText            | Tooltip text displayed on hover next to the label                    | Not supported                                                   |
| emptyMessage           | Message displayed if no value is set                                 | Not supported                                                   |
| searchMode             | Type of search (fuzzy, caseInsensitive, caseSensitive)               | Not supported                                                   |
| overlayMaxHeight       | Maximum height of the options list in pixels                         | Not supported                                                   |
| overlayMinWidth        | Minimum width of the options list in pixels                          | Not supported                                                   |
| showSelectionIndicator | Whether to display an icon next to a selected item                   | Not supported                                                   |
| clearInputValueOnChange| Whether to clear the input value when the selection changes          | Not supported                                                   |
| disabledValues         | A list of values not available for selection                         | Not supported                                                   |
| margin                 | Amount of margin rendered outside the component                      | Not supported                                                   |
| style                  | Custom style options                                                 | Not supported                                                   |
| isHiddenOnDesktop      | Whether hidden in the desktop layout                                 | `.Visible(false)`                                               |
| isHiddenOnMobile       | Whether hidden in the mobile layout                                  | Not supported                                                   |
| maintainSpaceWhenHidden| Whether to take up space when hidden                                 | Not supported                                                   |
| **Events**             |                                                                      |                                                                 |
| Change                 | Fires when the selection updates                                     | `.OnChange(...)`                                                |
| Blur                   | Fires when the input field loses focus                               | `.OnBlur(...)`                                                  |
| Focus                  | Fires when the input field gains focus                               | Not supported                                                   |
| Input Value Change     | Fires when typed content changes                                     | Not supported                                                   |
| **Methods**            |                                                                      |                                                                 |
| setValue()             | Set the year value programmatically                                  | Set via state: `selectedYear.Set("2025")`                       |
| clearValue()           | Clear the current selection                                          | Set via state: `selectedYear.Set(null)`                         |
| resetValue()           | Reset to the default value                                           | Not supported (manage via state logic)                          |
| focus() / blur()       | Manage input focus                                                   | Not supported                                                   |
| setDisabled()          | Toggle whether interaction is disabled                               | `.Disabled(bool)`                                               |
| setHidden()            | Toggle visibility                                                    | `.Visible(bool)`                                                |
| clearValidation()      | Clear validation message                                             | `.Invalid(null)`                                                |
| scrollIntoView()       | Scroll component into view                                           | Not supported                                                   |
