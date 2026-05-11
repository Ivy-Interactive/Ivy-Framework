# Editable Number

A click-to-edit input field for entering a number. Functions similarly to a standard Number Input but renders as plain text until clicked, then reveals the input field for editing.

## Retool

```toolscript
// Basic editable number with default value
editableNumber1.setValue(42);

// Access the current value
const currentValue = editableNumber1.value;

// Clear or reset
editableNumber1.clearValue();
editableNumber1.resetValue();

// Configuration via Inspector:
// - format: "decimal" | "percent" | "currency"
// - currency: "USD" (ISO code, when format is "currency")
// - decimalPlaces: 2
// - min / max validation
// - placeholder: "Enter a value"
// - showStepper: true/false
// - showSeparators: true/false
```

## Ivy

```csharp
// Basic number input bound to state
var amount = new State<decimal>(42m);
amount.ToNumberInput();

// With min/max validation
amount.ToNumberInput()
    .Min(0)
    .Max(1000);

// Currency formatted input
amount.ToMoneyInput()
    .Currency("USD")
    .Precision(2);

// With placeholder and step
amount.ToNumberInput()
    .Placeholder("Enter a value")
    .Step(0.5m);

// Percentage format
var rate = new State<decimal>(0m);
rate.ToNumberInput()
    .Format(NumberFormatStyle.Percent);

// Slider variant
amount.ToSliderInput()
    .Min(0)
    .Max(100)
    .Step(1);

// Event handling
amount.ToNumberInput()
    .OnChange(val => Console.WriteLine($"Changed to {val}"))
    .OnBlur(() => Console.WriteLine("Lost focus"));
```

## Parameters

| Parameter        | Retool                                             | Ivy                                                     |
|------------------|----------------------------------------------------|---------------------------------------------------------|
| value            | `value` (number, read-only)                        | State-bound `TNumber` via constructor                   |
| defaultValue     | `setValue()` / `resetValue()` methods               | Initial state value                                     |
| min              | `min` (number)                                     | `.Min()`                                                |
| max              | `max` (number)                                     | `.Max()`                                                |
| placeholder      | `placeholder` (string)                             | `.Placeholder()`                                        |
| format           | `format` ("decimal" \| "percent" \| "currency")    | `NumberFormatStyle.Decimal / Percent / Currency`        |
| currency         | `currency` (ISO code, e.g. "USD")                  | `.Currency("USD")`                                      |
| decimalPlaces    | `decimalPlaces` (integer)                          | `.Precision()`                                          |
| padDecimal       | `padDecimal` (boolean)                             | Not supported                                           |
| step             | `showStepper` (boolean, shows +/- buttons)         | `.Step()` (numeric increment value)                     |
| showSeparators   | `showSeparators` (boolean, thousands separator)    | Not supported                                           |
| required         | `required` (boolean)                               | Not supported                                           |
| allowNull        | `allowNull` (boolean)                              | Nullable types (`decimal?`, `int?`)                     |
| readOnly         | `readOnly` (boolean)                               | `.Disabled()`                                           |
| loading          | `loading` (boolean)                                | Not supported                                           |
| label            | `labelPosition` ("top" \| "left")                  | Not supported                                           |
| tooltipText      | `tooltipText` (markdown string)                    | Not supported                                           |
| inputTooltip     | `inputTooltip` (helper text below input)           | Not supported                                           |
| prefixText       | `textBefore` (string)                              | Not supported                                           |
| suffixText       | `textAfter` (string)                               | Not supported                                           |
| prefixIcon       | `iconBefore` (icon key)                            | Not supported                                           |
| suffixIcon       | `iconAfter` (icon key)                             | Not supported                                           |
| editIcon         | `editIcon` (icon for editable indicator)           | Not supported                                           |
| showClear        | `showClear` (boolean, clear button)                | Not supported                                           |
| textAlign        | `textAlign` ("left" \| "center" \| "right")        | Not supported                                           |
| hidden           | `isHiddenOnDesktop` / `isHiddenOnMobile`           | Not supported                                           |
| maintainSpace    | `maintainSpaceWhenHidden` (boolean)                | Not supported                                           |
| margin           | `margin` ("4px 8px" \| "0")                        | Not supported                                           |
| style            | `style` (object)                                   | Not supported                                           |
| validation       | `invalid` state via `clearValidation()`            | `.Invalid()`                                            |
| onChange         | Change event handler                               | `.OnChange()`                                           |
| onBlur           | Blur event handler                                 | `.OnBlur()`                                             |
| onFocus          | Focus event handler                                | Not supported                                           |
| click-to-edit    | Built-in (renders as text until clicked)           | Not supported                                           |
| slider           | Not supported (separate component)                 | `.ToSliderInput()` variant                              |
