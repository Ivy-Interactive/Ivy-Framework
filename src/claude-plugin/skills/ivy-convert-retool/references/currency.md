# Currency

A preset version of Number Input that is preconfigured with common options for currency input, including currency code, decimal places, separators, and prefix/suffix text.

## Retool

```toolscript
currency1.setValue(99.99)
currency1.clearValue()
currency1.focus()
currency1.resetValue()
```

## Ivy

```csharp
var amount = UseState<decimal>(99.99M);

return amount.ToMoneyInput()
    .Currency("USD")
    .Precision(2)
    .Min(0)
    .Max(10000)
    .WithField()
    .Label("Amount");
```

## Parameters

| Parameter        | Documentation                                                               | Ivy                                                                              |
|------------------|-----------------------------------------------------------------------------|----------------------------------------------------------------------------------|
| `value`          | Current input value                                                         | `Value` / constructor state binding                                              |
| `currency`       | ISO 4217 currency code (e.g. `"USD"`)                                       | `.Currency("USD")`                                                               |
| `decimalPlaces`  | Number of decimal places to display, with auto-rounding                     | `.Precision(int)`                                                                |
| `format`         | Display format: `decimal`, `percent`, `currency`                            | `.FormatStyle(NumberFormatStyle.Currency)` / `.Decimal` / `.Percent`              |
| `allowNull`      | Permit null values instead of defaulting to 0                               | `.Nullable(true)` or use nullable type `decimal?`                                |
| `disabled`       | Disables user interaction                                                   | `.Disabled()`                                                                    |
| `min`            | Minimum allowed value                                                       | `.Min(double)`                                                                   |
| `max`            | Maximum allowed value                                                       | `.Max(double)`                                                                   |
| `placeholder`    | Text shown when field is empty (default `"Enter a value"`)                  | `.Placeholder(string)` or constructor parameter                                  |
| `padDecimal`     | Pad trailing zeros to match `decimalPlaces`                                 | Not supported (`.Precision()` controls display but padding behavior is automatic) |
| `showStepper`    | Show increment/decrement buttons                                            | `.Step(double)` (stepper appears when step is configured)                        |
| `showSeparators` | Show thousands separator (e.g. `1,000`)                                     | Not supported (handled implicitly by `FormatStyle`)                              |
| `showClear`      | Show a clear button                                                         | Not supported                                                                    |
| `textBefore`     | Prefix text displayed before the value (e.g. `"$"`)                         | Not supported (currency symbol is derived from `Currency` code)                  |
| `textAfter`      | Suffix text displayed after the value (e.g. `"USD"`)                        | Not supported (currency symbol is derived from `Currency` code)                  |
| `readOnly`       | Prevents user modification while keeping appearance                         | Not supported (`.Disabled()` is the closest equivalent)                          |
| `required`       | Mandates that a value is provided                                           | Not supported (use `.Invalid("Required")` for custom validation)                 |
| `clearValue()`   | Method to clear the field                                                   | `state.Set(default)`                                                             |
| `setValue(v)`    | Method to set the value programmatically                                    | `state.Set(v)`                                                                   |
| `focus()`        | Method to set keyboard focus                                                | Not supported                                                                    |
| `resetValue()`   | Method to revert to default value                                           | Not supported (re-set state to initial value manually)                           |
| Event: Change    | Fires when the value changes                                                | `OnChange` handler via constructor or event binding                              |
| Event: Focus     | Fires when the field gains focus                                            | Not supported                                                                    |
| Event: Blur      | Fires when the field loses focus                                            | `OnBlur` handler via `.HandleBlur()`                                             |
| Event: Submit    | Fires when the value is submitted                                           | Not supported                                                                    |
