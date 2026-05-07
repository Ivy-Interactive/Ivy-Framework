# URL

A preset text input preconfigured for URL entry, with built-in validation and browser autofill support for web addresses.

## Retool

```toolscript
url1.setValue("https://example.com")
url1.value // "https://example.com"
url1.clearValue()
url1.focus()
```

## Ivy

```csharp
// Using the Url variant of TextInput
var urlState = UseState("https://example.com");
urlState.ToTextInput().Variant(TextInputVariants.Url)

// Or via extension method on state
var url = new State<string>("https://example.com");
url.ToUrlInput()
```

## Parameters

| Parameter          | Documentation                                           | Ivy                                              |
|--------------------|---------------------------------------------------------|--------------------------------------------------|
| value              | Current or default value of the input                   | Constructor `value` parameter                    |
| placeholder        | Placeholder text displayed when empty                   | `Placeholder`                                    |
| disabled           | Disables input interaction                              | `Disabled`                                       |
| label              | Text label for the input                                | Not supported                                    |
| labelPosition      | Position of the label (`top` or `left`)                 | Not supported                                    |
| required           | Whether the field is required                           | Not supported (use `Invalid` for validation)     |
| readOnly           | Makes input read-only                                   | Not supported                                    |
| loading            | Shows a loading indicator                               | Not supported                                    |
| maxLength          | Maximum number of characters allowed                    | `MaxLength`                                      |
| minLength          | Minimum number of characters required                   | Not supported                                    |
| pattern            | Regex validation pattern                                | Not supported (use `Invalid` for validation)     |
| patternType        | Validation type (`email`, `regex`, `url`)               | `Variant` (e.g. `TextInputVariants.Url`)               |
| showClear          | Displays a clear button                                 | Not supported                                    |
| iconBefore         | Prefix icon                                             | `Prefix`                                         |
| iconAfter          | Suffix icon                                             | `Suffix`                                         |
| autoCapitalize     | Auto-capitalization behavior                            | Not supported                                    |
| autoComplete       | Browser autocomplete capability                         | Not supported                                    |
| autoFill           | Data type for browser autofill                          | Not supported                                    |
| spellCheck         | Enables browser spellcheck                              | Not supported                                    |
| enforceMaxLength   | Enforces character limit                                | Not supported                                    |
| inputTooltip       | Helper text shown on focus                              | Not supported                                    |
| isHiddenOnDesktop  | Visibility on desktop                                   | `Visible`                                        |
| isHiddenOnMobile   | Visibility on mobile                                    | `Visible`                                        |
| **Events**         |                                                         |                                                  |
| Change             | Fires when value is modified                            | `OnChange`                                       |
| Focus              | Fires when input is selected                            | Not supported                                    |
| Blur               | Fires when input is deselected                          | `OnBlur`                                         |
| Submit             | Fires when value is submitted                           | Not supported (use `ShortcutKey`)                |
| **Methods**        |                                                         |                                                  |
| setValue()         | Sets the current value                                  | Set via state binding                            |
| clearValue()       | Clears the current value                                | Set state to `""`                                |
| resetValue()       | Resets to default value                                 | Not supported                                    |
| focus()            | Sets focus on the component                             | Not supported                                    |
| blur()             | Removes focus from the component                        | Not supported                                    |
| clearValidation()  | Clears validation messages                              | Set `Invalid` to `null`                          |
| setDisabled()      | Toggles disabled state                                  | Set `Disabled` property                          |
| setHidden()        | Toggles visibility                                      | Set `Visible` property                           |
| scrollIntoView()   | Scrolls component into view                             | Not supported                                    |
