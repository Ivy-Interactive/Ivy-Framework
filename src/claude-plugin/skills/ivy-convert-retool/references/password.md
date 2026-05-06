# Password

An input field for secure password entry that masks the entered text. In Retool this is a dedicated component; in Ivy it is the `TextInput` widget with the `Password` variant.

## Retool

```toolscript
password1.value          // read current value
password1.setValue("x")  // set value programmatically
password1.clearValue()   // clear value
password1.focus()        // focus the input
```

## Ivy

```csharp
state.ToTextInput().Placeholder("Enter password").Variant(TextInputVariants.Password);

// or using the extension method
state.ToPasswordInput();
```

## Parameters

| Parameter              | Retool Documentation                                                       | Ivy                                                                 |
|------------------------|---------------------------------------------------------------------------|---------------------------------------------------------------------|
| `value`                | Current or default value of the input                                     | `Value` property (read-only); set via `IAnyState` binding           |
| `placeholder`          | Placeholder text shown when empty (`"Enter a value"`)                     | `.Placeholder(string)`                                              |
| `disabled`             | Disables user interaction                                                 | `.Disabled()` / `.Disabled(bool)`                                   |
| `hidden`               | Hides the component from view                                             | `.Visible(bool)`                                                    |
| `label`                | Text label for the field                                                  | `.Label(string)` (via `.WithField()`)                               |
| `required`             | Makes the field mandatory                                                 | Not supported                                                       |
| `readOnly`             | Prevents user edits while still visible                                   | Not supported (use `Disabled` as alternative)                       |
| `loading`              | Shows a loading indicator                                                 | Not supported                                                       |
| `maxLength`            | Maximum character count allowed                                           | `.MaxLength(int)`                                                   |
| `minLength`            | Minimum character count required                                          | Not supported                                                       |
| `pattern`              | Regex pattern for validation                                              | Not supported (use `Invalid` with custom logic)                     |
| `patternType`          | Preset validation pattern (`email`, `url`, `regex`)                       | Not supported (use `Variant` for email/url types)                   |
| `customValidation`     | Custom validation expression                                              | `.Invalid(string)` — set a validation message, empty to clear       |
| `hideValidationMessage`| Hides validation error messages                                           | Not supported                                                       |
| `showCharacterCount`   | Displays a character counter                                              | Not supported                                                       |
| `showClear`            | Shows a clear button inside the input                                     | Not supported                                                       |
| `iconBefore`           | Prefix icon                                                               | `.Prefix(IWidget)` — accepts any widget including icons             |
| `iconAfter`            | Suffix icon                                                               | `.Suffix(IWidget)` — accepts any widget including icons             |
| `formDataKey`          | Key used when submitting form data                                        | Not supported (handled by state binding)                            |
| `autoFill`             | Browser autofill hint (`current-password`, `new-password`, `one-time-code`)| Not supported                                                       |
| `spellCheck`           | Enables browser spellcheck                                                | Not supported                                                       |
| `inputTooltip`         | Helper text shown on focus                                                | Not supported                                                       |
| `enforceMaxLength`     | Strictly enforces the max character limit                                 | Not supported (`MaxLength` enforces by default)                     |
| **Events**             |                                                                           |                                                                     |
| `Change`               | Fires when the value changes                                              | `OnChange`                                                          |
| `Focus`                | Fires when the input gains focus                                          | Not supported                                                       |
| `Blur`                 | Fires when the input loses focus                                          | `OnBlur`                                                            |
| `Submit`               | Fires when the value is submitted                                         | Not supported                                                       |
| **Methods**            |                                                                           |                                                                     |
| `setValue(value)`       | Sets the current value                                                   | Set via bound `IAnyState`                                           |
| `clearValue()`         | Clears the current value                                                  | Set state to `""` or `null`                                         |
| `resetValue()`         | Restores the default value                                                | Not supported                                                       |
| `focus()`              | Focuses the input                                                         | Not supported                                                       |
| `blur()`               | Removes focus                                                             | Not supported                                                       |
| `setDisabled(bool)`    | Toggles disabled state                                                    | `.Disabled(bool)`                                                   |
| `setHidden(bool)`      | Toggles visibility                                                        | `.Visible(bool)`                                                    |
| `clearValidation()`    | Clears validation messages                                                | `.Invalid("")`                                                      |
| `scrollIntoView(opts)` | Scrolls component into view                                               | Not supported                                                       |
