# Percent

An input field to enter a percentage number value. Retool's Percent is a preset version of Number Input, preconfigured with common options for percentage input (format set to `percent`).

## Retool

```toolscript
// Basic percent input with default value
percent1.setValue(0.75);

// Read the current value
const currentPercent = percent1.value;

// Configure via Inspector:
// - Format: Percent
// - Decimal places: 2
// - Min: 0, Max: 1
// - Show stepper buttons
// - Show thousands separator

// Methods
percent1.clearValue();
percent1.resetValue();
percent1.focus();
percent1.setHidden(false);
percent1.clearValidation();
```

## Ivy

```csharp
var percent = UseState(0.75);

percent.ToNumberInput()
    .FormatStyle(NumberFormatStyle.Percent)
    .Min(0.0)
    .Max(1.0)
    .Precision(2)
    .Placeholder("Enter a value")
    .WithField()
    .Label("Percentage");
```

## Parameters

| Parameter          | Retool Documentation                                                         | Ivy                                                          |
|--------------------|-----------------------------------------------------------------------------|--------------------------------------------------------------|
| `value`            | The current value (number, read-only)                                       | `UseState<double>` bound to constructor                      |
| `format`           | Formatting style: `decimal`, `percent`, `currency`                          | `.FormatStyle(NumberFormatStyle.Percent)`                     |
| `decimalPlaces`    | Number of decimal places to display                                         | `.Precision(n)`                                              |
| `min`              | Minimum allowed value                                                       | `.Min(value)`                                                |
| `max`              | Maximum allowed value                                                       | `.Max(value)`                                                |
| `placeholder`      | Text displayed when input is empty                                          | `.Placeholder("text")`                                       |
| `disabled`         | Whether input is disabled                                                   | `.Disabled()`                                                |
| `required`         | Whether a value is required                                                 | Not supported                                                |
| `readOnly`         | Whether input is read-only                                                  | Not supported                                                |
| `allowNull`        | Allow `null` instead of `0` when empty                                      | Supported via nullable types (e.g. `double?`)                |
| `currency`         | ISO currency code when format is `currency`                                 | `.Currency("USD")`                                           |
| `showStepper`      | Show increment/decrement buttons                                            | `.Step(value)` with `.Variant(NumberInputs.Number)`          |
| `showSeparators`   | Show localized thousands separator                                          | Not supported                                                |
| `padDecimal`       | Include trailing zeros to match `decimalPlaces`                             | Not supported                                                |
| `labelPosition`    | Position of label (`top`, `left`)                                           | `.WithField().Label("text")` (layout controlled by Field)    |
| `textBefore`       | Prefix text                                                                 | Not supported                                                |
| `textAfter`        | Suffix text                                                                 | Not supported                                                |
| `iconBefore`       | Prefix icon                                                                 | Not supported                                                |
| `iconAfter`        | Suffix icon                                                                 | Not supported                                                |
| `tooltipText`      | Tooltip text on label hover                                                 | `.WithField().Description("text")`                           |
| `inputTooltip`     | Helper text below input on focus                                            | Not supported                                                |
| `loading`          | Show loading indicator                                                      | Not supported                                                |
| `showClear`        | Show a clear button                                                         | Not supported                                                |
| `textAlign`        | Text alignment (`left`, `center`, `right`)                                  | Not supported                                                |
| `margin`           | Outer margin (`4px 8px` or `0`)                                             | Not supported                                                |
| `isHiddenOnDesktop`| Hide in desktop layout                                                      | Not supported                                                |
| `isHiddenOnMobile` | Hide in mobile layout                                                       | Not supported                                                |
| `maintainSpaceWhenHidden` | Keep space when hidden                                               | Not supported                                                |
| `showInEditor`     | Show in editor when hidden                                                  | Not supported                                                |
| `events`           | Event handlers (Blur, Change, Focus, Submit)                                | `OnChange`, `OnBlur`                                         |
| `setValue()`       | Set the current value                                                       | `state.Set(value)`                                           |
| `clearValue()`     | Clear the current value                                                     | Not supported                                                |
| `resetValue()`     | Reset to default value                                                      | Not supported                                                |
| `focus()`          | Set focus on the component                                                  | Not supported                                                |
| `setHidden()`      | Toggle visibility                                                           | Not supported                                                |
| `clearValidation()`| Clear validation message                                                    | Not supported                                                |
| `scrollIntoView()` | Scroll component into view                                                  | Not supported                                                |
| `style`            | Custom style options                                                        | Not supported                                                |
| `invalid`          | Validation error display                                                    | `.Invalid("message")`                                        |
