# Email

A preset version of Text Input preconfigured with common options for email address input, including built-in email pattern validation, a mail icon prefix, and autocomplete hints.

## Retool

```toolscript
email1.setValue("user@example.com")
email1.value // "user@example.com"
```

## Ivy

```csharp
var email = state.ToEmailInput(placeholder: "Enter a value");

// or explicitly
var email = state.ToTextInput().Placeholder("Enter a value").Variant(TextInputVariants.Email);
```

## Parameters

| Parameter         | Retool                                                                 | Ivy                                                                                  |
|-------------------|------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| value             | `value` (string) - current or default value                            | `Value` (string) via `IAnyState` binding                                             |
| placeholder       | `placeholder` (string, default `"Enter a value"`)                      | `.Placeholder(string)`                                                               |
| disabled          | `disabled` (bool, default `false`)                                     | `.Disabled(bool)`                                                                    |
| label             | `label` (string, default `"Email"`)                                    | `.Label(string)` via `.WithField()`                                                  |
| labelPosition     | `labelPosition` (`"left"` or `"top"`)                                  | Not supported                                                                        |
| required          | `required` (bool, default `false`)                                     | Not supported (use `.Invalid(string)` for validation)                                |
| readOnly          | `readOnly` (bool, default `false`)                                     | Not supported                                                                        |
| loading           | `loading` (bool, default `false`)                                      | Not supported                                                                        |
| pattern           | `pattern` (string) - regex validation pattern                          | Not supported (use `.Invalid(string)` for custom validation)                         |
| patternType       | `patternType` (`"email"`, `"regex"`, `"url"`)                          | `TextInputVariants.Email` variant provides built-in email validation                        |
| minLength         | `minLength` (number)                                                   | Not supported                                                                        |
| maxLength         | `maxLength` (number)                                                   | `.MaxLength(int)`                                                                    |
| enforceMaxLength  | `enforceMaxLength` (bool, default `false`)                             | Not supported (MaxLength enforces by default)                                        |
| iconBefore        | `iconBefore` (string, default mail icon)                               | `.Prefix(IWidget)` - pass any widget as prefix                                       |
| iconAfter         | `iconAfter` (string)                                                   | `.Suffix(IWidget)` - pass any widget as suffix                                       |
| textBefore        | `textBefore` (string)                                                  | `.Prefix(string)`                                                                    |
| textAfter         | `textAfter` (string)                                                   | `.Suffix(string)`                                                                    |
| showClear         | `showClear` (bool, default `false`)                                    | Not supported                                                                        |
| inputTooltip      | `inputTooltip` (string)                                                | Not supported                                                                        |
| autoComplete      | `autoComplete` (bool, default `false`)                                 | Not supported                                                                        |
| autoFill          | `autoFill` (string, default `"off"`)                                   | Not supported                                                                        |
| autoCapitalize    | `autoCapitalize` (`"none"`, `"sentences"`, `"words"`, `"characters"`)  | Not supported                                                                        |
| spellCheck        | `spellCheck` (bool, default `false`)                                   | Not supported                                                                        |
| hidden            | `setHidden(bool)` method                                               | `.Visible(bool)`                                                                     |
| **Events**        |                                                                        |                                                                                      |
| onChange          | Change event                                                           | `OnChange` callback                                                                  |
| onBlur            | Blur event                                                             | `OnBlur` callback                                                                    |
| onFocus           | Focus event                                                            | Not supported                                                                        |
| onSubmit          | Submit event                                                           | Not supported (use `ShortcutKey` for keyboard submission)                            |
| **Methods**       |                                                                        |                                                                                      |
| setValue          | `setValue(value)`                                                      | State binding (set state value directly)                                             |
| clearValue        | `clearValue()`                                                         | Set state to `""` or `null`                                                          |
| resetValue        | `resetValue()`                                                         | Not supported                                                                        |
| focus             | `focus()`                                                              | Not supported                                                                        |
| blur              | `blur()`                                                               | Not supported                                                                        |
| clearValidation   | `clearValidation()`                                                    | `.Invalid(null)` to clear                                                            |
| scrollIntoView    | `scrollIntoView(options)`                                              | Not supported                                                                        |
| setDisabled       | `setDisabled(bool)`                                                    | `.Disabled(bool)`                                                                    |
