# TextArea

A multi-line text input field that allows users to enter and edit larger amounts of text. In Retool this is a dedicated `TextArea` component; in Ivy it is a `TextInput` with the `Textarea` variant.

## Retool

```toolscript
textArea1.setValue("Hello, world!");
textArea1.value; // "Hello, world!"
textArea1.clearValue();
textArea1.focus();
textArea1.validate();
```

## Ivy

```csharp
var text = new State<string>("Hello, world!");
text.ToTextInput()
    .Placeholder("Enter a value")
    .Variant(TextInputVariants.Textarea)
    .Rows(5)
    .MaxLength(500)
    .Label("Description")
    .WithField();
```

## Parameters

| Parameter          | Retool Documentation                                      | Ivy                                                             |
|--------------------|-----------------------------------------------------------|-----------------------------------------------------------------|
| value              | Current field value (`string`)                            | `Value` property / `IAnyState` binding                          |
| placeholder        | Default display text when empty                           | `.Placeholder(string)`                                          |
| disabled           | Disables input interaction (`bool`, default `false`)      | `.Disabled()` / constructor param                               |
| readOnly           | Prevents user modification (`bool`, default `false`)      | Not supported                                                   |
| required           | Makes field mandatory (`bool`, default `false`)           | Not supported (use `.Invalid()` for validation)                 |
| loading            | Shows loading indicator (`bool`, default `false`)         | Not supported                                                   |
| maxLength          | Maximum character count (`number`)                        | `.MaxLength(int)`                                               |
| minLength          | Minimum character requirement (`number`)                  | Not supported                                                   |
| minLines           | Minimum visible lines (`number`)                          | `.Rows(int)` (sets fixed row count)                             |
| maxLines           | Maximum visible lines (`number`)                          | Not supported (use `.Height()` instead)                         |
| autoResize         | Field expands to fit content (`bool`, default `true`)     | Not supported                                                   |
| pattern            | Regex validation pattern (`string`)                       | Not supported (use `.Invalid()` with custom logic)              |
| showCharacterCount | Displays character counter (`bool`)                       | Not supported                                                   |
| spellCheck         | Enables spell checking (`bool`, default `false`)          | Not supported                                                   |
| autoComplete       | Enables browser autocomplete (`bool`, default `false`)    | Not supported                                                   |
| autoCapitalize     | Capitalization behavior (`string`, default `none`)        | Not supported                                                   |
| autoFill           | Data type for browser autofill (`string`, default `off`)  | Not supported                                                   |
| enforceMaxLength   | Enforces maximum character limit (`bool`, default `false`)| Not supported (MaxLength enforces by default)                   |
| labelPosition      | Label placement: top or left (`string`, default `left`)   | `.WithField()` / `.Label(string)`                               |
| inputTooltip       | Helper text displayed on focus (`string`)                 | Not supported                                                   |
| id                 | Unique component identifier (`string`)                    | Not applicable (C# object reference)                            |
| visible            | Component visibility                                      | `.Visible(bool)`                                                |
| prefix / suffix    | Not supported                                             | `.Prefix(Affix)` / `.Suffix(Affix)`                            |
| shortcutKey        | Not supported                                             | `.ShortcutKey(string)`                                          |
| height / width     | Not directly configurable                                 | `.Height(Size)` / `.Width(Size)`                                |
| onChange           | Change event handler                                      | `OnChange` callback / `IAnyState` auto-binding                  |
| onBlur             | Blur event handler                                        | `OnBlur` callback                                               |
| onFocus            | Focus event handler                                       | Not supported                                                   |
| setValue()         | Sets field content                                        | State binding (e.g. `state.Set("value")`)                       |
| clearValue()       | Empties the field                                         | `state.Set("")`                                                 |
| resetValue()       | Restores default value                                    | Not supported                                                   |
| focus()            | Sets keyboard focus                                       | Not supported                                                   |
| validate()         | Triggers validation                                       | `.Invalid(string)` with custom logic                            |
| clearValidation()  | Removes validation messages                               | `.Invalid(null)`                                                |
| scrollIntoView()   | Scrolls component into view                               | Not supported                                                   |
| setDisabled()      | Toggles disabled state                                    | `.Disabled(bool)` or state binding                              |
| setHidden()        | Toggles visibility                                        | `.Visible(bool)` or state binding                               |
