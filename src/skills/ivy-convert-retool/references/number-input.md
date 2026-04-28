# Number Input

An input field to enter a number, with support for formatting (decimal, percent, currency), min/max constraints, and validation.

## Retool

```toolscript
// Set the value programmatically
numberInput.setValue(42);

// Clear the current value
numberInput.clearValue();

// Reset to default value
numberInput.resetValue();

// Focus the input
numberInput.focus();

// Toggle visibility
numberInput.setHidden(true);

// Scroll into view
numberInput.scrollIntoView({ behavior: 'auto', block: 'nearest' });

// Clear validation messages
numberInput.clearValidation();
```

## Ivy

```csharp
// Basic number input bound to state
var count = new State<int>(0);
count.ToNumberInput(placeholder: "Enter a number");

// With min/max and step
var value = UseState(0.0);
value.ToNumberInput()
    .Min(0)
    .Max(100)
    .Step(0.5)
    .Precision(2);

// Currency-formatted input
count.ToMoneyInput(placeholder: "Enter amount", currency: "USD");

// Percent-formatted input
count.ToNumberInput(placeholder: "Enter percent")
    .FormatStyle(NumberFormatStyle.Percent);

// Slider variant
count.ToSliderInput(placeholder: "Slide to select");

// Disabled input
count.ToNumberInput(placeholder: "Read only").Disabled();

// With validation
count.ToNumberInput().Invalid("Value is out of range");
```

## Parameters

| Parameter         | Retool                                            | Ivy                                                              |
|-------------------|---------------------------------------------------|------------------------------------------------------------------|
| Value             | `value` (read-only current value)                 | `Value` (generic `TNumber`)                                      |
| Placeholder       | `placeholder` (default: "Enter a value")          | `Placeholder` (string)                                           |
| Disabled          | `disabled` (boolean)                              | `Disabled` (bool)                                                |
| Min               | `min` (number)                                    | `Min` (double?)                                                  |
| Max               | `max` (number)                                    | `Max` (double?)                                                  |
| Step              | `showStepper` (toggle increment/decrement UI)     | `Step` (double?, controls increment value)                       |
| Decimal Places    | `decimalPlaces` (number, auto-rounds)             | `Precision` (int?)                                               |
| Format            | `format` (decimal, percent, currency)             | `FormatStyle` (NumberFormatStyle: Decimal, Percent, Currency)     |
| Currency          | `currency` (ISO currency code)                    | `Currency` (string, e.g. "USD", "EUR")                           |
| Allow Null        | `allowNull` (permits null instead of 0)           | `Nullable` (bool)                                                |
| Required          | `required` (mandates a value)                     | Not supported                                                    |
| Read Only         | `readOnly` (prevents edits)                       | Not supported (use `Disabled` as alternative)                    |
| Show Clear        | `showClear` (displays clear button)               | Not supported                                                    |
| Show Separators   | `showSeparators` (thousands separator)            | Not supported (handled by format style)                          |
| Icon Before       | `iconBefore` (leading icon)                       | Not supported                                                    |
| Icon After        | `iconAfter` (trailing icon)                       | Not supported                                                    |
| Prefix Text       | `prefixText` (e.g. "$")                           | Not supported                                                    |
| Suffix Text       | `suffixText` (e.g. "USD")                         | Not supported                                                    |
| Validation        | `clearValidation()` method                        | `Invalid` (string error message)                                 |
| Variant           | N/A                                               | `Variant` (NumberInputs: Number, Slider)                         |
| Width             | Configured via layout                             | `Width` (Size)                                                   |
| Visible           | `setHidden()` method                              | `Visible` (bool, read-only)                                      |
| On Change         | Change event handler                              | `OnChange` (Func<Event<IInput<TNumber>, TNumber>, ValueTask>)    |
| On Blur           | Blur event handler                                | `OnBlur` (Func<Event<IAnyInput>, ValueTask>)                     |
| On Focus          | Focus event handler                               | Not supported                                                    |
| On Submit         | Submit event handler                              | Not supported                                                    |
| Input Value       | `inputValue` (most recently entered, read-only)   | Not supported                                                    |
| setValue()        | `numberInput.setValue(value)`                      | Set via state binding (`state.Value = x`)                        |
| clearValue()      | `numberInput.clearValue()`                         | Set via state binding (`state.Value = default`)                  |
| resetValue()      | `numberInput.resetValue()`                         | Not supported                                                    |
| focus()           | `numberInput.focus()`                              | Not supported                                                    |
| scrollIntoView()  | `numberInput.scrollIntoView()`                     | Not supported                                                    |
