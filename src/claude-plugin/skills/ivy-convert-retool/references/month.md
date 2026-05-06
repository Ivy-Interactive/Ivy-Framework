# Month

A dropdown menu and input field to select or enter a month. It is a preset version of Select, preconfigured with a list of month values (January through December).

## Retool

```toolscript
// Basic month selector with default value
month1.setValue("January");

// Read the selected month
const selected = month1.value;

// Disable the month selector
month1.setDisabled(true);

// Clear the selected value
month1.clearValue();

// Reset to default value
month1.resetValue();
```

## Ivy

Ivy does not have a dedicated Month widget. Use `SelectInput` with month options to achieve the same result.

```csharp
var month = UseState("January");

var monthOptions = new[] {
    "January", "February", "March", "April",
    "May", "June", "July", "August",
    "September", "October", "November", "December"
}.ToOptions();

return month.ToSelectInput(monthOptions)
    .Variant(SelectInputs.Select)
    .Label("Month")
    .Placeholder("Select a month...");
```

## Parameters

| Parameter                | Documentation                                                              | Ivy                                                            |
|--------------------------|----------------------------------------------------------------------------|----------------------------------------------------------------|
| `value`                  | The current selected value                                                 | `UseState<string>()` bound to `ToSelectInput()`                |
| `placeholder`            | Text displayed when no value is set. Default: `"Enter a value"`            | `.Placeholder("...")`                                          |
| `disabled`               | Whether interaction is disabled. Default: `false`                          | `.Disabled(true)`                                              |
| `required`               | Whether a value is required. Default: `false`                              | Not supported (use manual validation)                          |
| `readOnly`               | Whether user input is read-only. Default: `false`                          | Not supported                                                  |
| `loading`                | Whether to display a loading indicator. Default: `false`                   | Not supported                                                  |
| `allowCustomValue`       | Whether to allow values not in the list. Default: `false`                  | Not supported                                                  |
| `allowDeselect`          | Whether to allow deselection. Default: `false`                             | `.Nullable(true)`                                              |
| `showClear`              | Whether to show a clear button. Default: `false`                           | `.Nullable(true)`                                              |
| `labelPosition`          | Position of the label (`top` or `left`). Default: `"left"`                 | `.Label("...")` (top only)                                     |
| `tooltipText`            | Tooltip text displayed on hover                                            | Not supported                                                  |
| `emptyMessage`           | Message displayed if no value is set                                       | `.Placeholder("...")`                                          |
| `searchMode`             | Type of search (`fuzzy`, `caseInsensitive`, `caseSensitive`)               | Not supported                                                  |
| `disabledValues`         | Values not available for selection                                         | Not supported                                                  |
| `selectedLabel`          | Label of the selected value (read-only)                                    | Not supported (value and label are the same with `ToOptions()`) |
| `selectedIndex`          | Index of the selected value (read-only)                                    | Not supported                                                  |
| `selectedItem`           | The selected item object (read-only)                                       | Not supported                                                  |
| `isHiddenOnDesktop`      | Whether hidden on desktop. Default: `false`                                | Not supported                                                  |
| `isHiddenOnMobile`       | Whether hidden on mobile. Default: `true`                                  | Not supported                                                  |
| `maintainSpaceWhenHidden`| Whether to maintain space when hidden. Default: `false`                    | Not supported                                                  |
| `margin`                 | Margin around the component. Default: `"4px 8px"`                          | Not supported                                                  |
| `overlayMaxHeight`       | Max height of dropdown list in pixels                                      | `.Height(Size)`                                                |
| `overlayMinWidth`        | Min width of dropdown list in pixels                                       | `.Width(Size)`                                                 |
| `style`                  | Custom style options                                                       | Not supported                                                  |
| `showSelectionIndicator` | Whether to show icon next to selected item. Default: `true`                | Not supported                                                  |
| `events`                 | Event handlers (Change, Focus, Blur, Input Value Change)                   | `OnChange`, `OnBlur`                                           |
| `id`                     | Unique identifier. Default: `"month1"`                                     | Variable name                                                  |
