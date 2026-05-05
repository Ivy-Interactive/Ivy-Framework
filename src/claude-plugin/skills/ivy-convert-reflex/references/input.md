# Input

A text field that users can type into. Captures user text input with support for placeholder text, validation, different input types, and event handling.

## Reflex

```python
rx.input(
    placeholder="Enter text",
    value=state.input_value,
    on_change=state.set_input_value,
)
```

## Ivy

```csharp
public override object? Build()
{
    var text = UseState("");
    return text.ToTextInput().Placeholder("Enter text");
}
```

## Parameters

| Parameter       | Documentation                                       | Ivy                                                        |
|-----------------|-----------------------------------------------------|------------------------------------------------------------|
| `placeholder`   | Placeholder text shown when empty                   | `.Placeholder("text")`                                     |
| `value`         | Current value of the input (str, int, float)        | `Value` property or state passed via constructor            |
| `default_value` | Initial uncontrolled value                          | Initial value set on the state via `UseState("")`          |
| `disabled`      | Disables the input field                            | `.Disabled` / constructor parameter                        |
| `max_length`    | Maximum number of characters allowed                | `.MaxLength(int)`                                          |
| `min_length`    | Minimum number of characters required               | Not supported                                              |
| `read_only`     | Makes the input read-only                           | Separate `ReadOnlyInput` widget                            |
| `required`      | Marks the field as required                         | Not supported (use validation via `.Invalid()`)            |
| `name`          | Name attribute for form submission                  | Not supported                                              |
| `type`          | HTML input type (text, password, email, etc.)       | `.Variant(TextInputVariants.Password)` / `.ToEmailInput()` etc.  |
| `size`          | Component size ("1", "2", "3")                      | `.Height()` / `.Width()` / `.Scale()`                      |
| `variant`       | Visual style ("classic", "surface", "soft", "ghost")| Not supported                                              |
| `color_scheme`  | Color theme for the input                           | Not supported (uses global theme)                          |
| `radius`        | Border radius ("none", "small", "medium", etc.)     | Not supported (uses global theme)                          |
| `auto_complete` | Enables browser autocomplete                        | Not supported                                              |
| `on_change`     | Fired when the value changes                        | `OnChange` event via constructor or property               |
| `on_focus`      | Fired when the input gains focus                    | Not supported                                              |
| `on_blur`       | Fired when the input loses focus                    | `OnBlur` event                                             |
| `on_key_down`   | Fired when a key is pressed                         | `.ShortcutKey("Ctrl+S")` for specific shortcuts            |
| `on_key_up`     | Fired when a key is released                        | Not supported                                              |
| N/A             | Prefix icon or text                                 | `.Prefix(Affix)` - prepend text or icon                    |
| N/A             | Suffix icon or text                                 | `.Suffix(Affix)` - append text or icon                     |
| N/A             | Validation error message                            | `.Invalid("error message")`                                |
| N/A             | Nullable input support                              | `.Nullable` - allow null values                            |
