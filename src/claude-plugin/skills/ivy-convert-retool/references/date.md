# Date

An input field enabling users to select or enter a date. Supports customizable formatting, validation, and interaction options.

## Retool

```toolscript
date1.value        // Current selected date
date1.formattedValue // Selected value in specified format

date1.setValue("2024-03-15")
date1.clearValue()
date1.resetValue()
date1.focus()
date1.validate()
date1.clearValidation()
date1.setDisabled(true)
date1.setHidden(false)
```

## Ivy

```csharp
var dateState = UseState(DateTime.Today);

return Layout.Vertical()
    | dateState.ToDateInput()
        .Format("dd/MM/yyyy")
        .Placeholder("Enter a date")
        .WithField()
        .Label("Select a date");
```

## Parameters

| Parameter              | Documentation                                        | Ivy                                                        |
|------------------------|------------------------------------------------------|------------------------------------------------------------|
| value                  | Current selected date value                          | Value / state binding                                      |
| dateFormat             | Displayed date format (e.g. "MM/DD/YYYY")            | `.Format("dd/MM/yyyy")`                                    |
| datePlaceholder        | Placeholder text when field is empty                 | `.Placeholder("...")`                                      |
| disabled               | Toggles whether input is enabled                     | `.Disabled(true)`                                          |
| readOnly               | Read-only input mode                                 | Not supported                                              |
| required               | Mandatory value requirement                          | Not supported                                              |
| minDate                | Earliest selectable date                             | Not supported                                              |
| maxDate                | Latest selectable date                               | Not supported                                              |
| firstDayOfWeek         | Week start day (0=Sunday through 6=Saturday)         | Not supported                                              |
| showClear              | Clear button visibility                              | `.Nullable(true)`                                          |
| formattedValue         | Selected value in specified format (read-only)       | Not supported                                              |
| labelPosition          | Label placement (top/left)                           | `.WithField().Label("...")` (separate layout concept)      |
| tooltipText            | Helper text on hover                                 | Not supported                                              |
| iconBefore             | Prefix icon display                                  | Not supported                                              |
| iconAfter              | Suffix icon display                                  | Not supported                                              |
| textBefore             | Prefix text display                                  | Not supported                                              |
| textAfter              | Suffix text display                                  | Not supported                                              |
| isHiddenOnDesktop      | Desktop visibility toggle                            | `.Visible(false)` (no desktop/mobile distinction)          |
| isHiddenOnMobile       | Mobile visibility toggle                             | `.Visible(false)` (no desktop/mobile distinction)          |
| maintainSpaceWhenHidden| Space reservation when hidden                        | Not supported                                              |
| margin                 | External spacing                                     | Not supported                                              |
| style                  | Custom styling options                               | Not supported                                              |
| showInEditor           | Editor visibility when hidden                        | Not supported                                              |

## Methods

| Method                     | Documentation                              | Ivy                              |
|----------------------------|--------------------------------------------|----------------------------------|
| setValue(value)             | Sets current date value                    | State binding (`state.Set(...)`) |
| clearValue()               | Clears the current value                   | State binding                    |
| resetValue()               | Restores default value                     | State binding                    |
| focus()                    | Sets component focus                       | Not supported                    |
| setDisabled(disabled)      | Enables/disables input                     | `.Disabled(bool)`                |
| setHidden(hidden)          | Shows/hides component                      | `.Visible(bool)`                 |
| validate()                 | Validates input value                      | `.Invalid("message")`            |
| clearValidation()          | Removes validation messages                | `.Invalid(null)`                 |
| scrollIntoView(options)    | Scrolls component into visible area        | Not supported                    |

## Events

| Event  | Documentation                        | Ivy                |
|--------|--------------------------------------|--------------------|
| Change | Triggered when value changes         | `.OnChange(...)` |
| Focus  | Triggered when field gains focus     | Not supported      |
| Blur   | Triggered when field loses focus     | `.OnBlur(...)`   |
| Submit | Triggered on value submission        | Not supported      |
