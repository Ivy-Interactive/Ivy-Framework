# TextArea

A multi-line text input field that allows users to enter extended text content across multiple lines. In Ivy, this is handled by the `TextInput` widget with the `TextInputVariants.Textarea` variant.

## Reflex

```python
import reflex as rx

class State(rx.State):
    text: str = ""

def index():
    return rx.text_area(
        value=State.text,
        placeholder="Enter your feedback...",
        rows="5",
        on_change=State.set_text,
    )
```

## Ivy

```csharp
var text = UseState("");

text.ToTextInput()
    .Placeholder("Enter your feedback...")
    .Variant(TextInputVariants.Textarea)
    .SetRows(5);
```

## Parameters

| Parameter      | Reflex Documentation                                      | Ivy                                                     |
|----------------|-----------------------------------------------------------|---------------------------------------------------------|
| value          | Current text value                                        | `Value` / state binding via constructor                  |
| placeholder    | Placeholder text shown when empty                         | `Placeholder`                                           |
| disabled       | Disables the input                                        | `Disabled`                                              |
| max_length     | Maximum number of characters allowed                      | `MaxLength`                                             |
| rows           | Number of visible text rows                               | `Rows`                                                  |
| on_change      | Triggered on each keystroke / value change                | `OnChange`                                              |
| on_blur        | Triggered when focus leaves the element                   | `OnBlur`                                                |
| variant        | Visual style (`"classic"`, `"surface"`, etc.)             | `Variant` (controls input type, not visual style)       |
| size           | Component sizing (`"1"`, `"2"`, `"3"`)                    | `Height` / `Width`                                      |
| default_value  | Initial text content                                      | Set via constructor `value` parameter                   |
| read_only      | Makes the input read-only                                 | Not supported                                           |
| required       | Marks the field as required                               | Not supported                                           |
| min_length     | Minimum number of characters required                     | Not supported                                           |
| auto_focus     | Automatically focus the input on page load                | Not supported                                           |
| auto_complete  | Enables browser autocomplete                              | Not supported                                           |
| resize         | Resize behavior (`"none"`, `"vertical"`, `"horizontal"`)  | Not supported                                           |
| color_scheme   | Color theme for the component                             | Not supported (use global theming)                      |
| radius         | Border radius styling                                     | Not supported                                           |
| name           | Field identifier for form submission                      | Not supported (state-driven)                            |
| form           | Associates with a specific form by ID                     | Not supported                                           |
| dirname        | Text directionality attribute                             | Not supported                                           |
| wrap           | Text wrapping behavior                                    | Not supported                                           |
| on_focus       | Triggered when element receives focus                     | Not supported                                           |
| on_key_down    | Triggered when a key is pressed                           | Not supported                                           |
| on_key_up      | Triggered when a key is released                          | Not supported                                           |
| N/A            | —                                                         | `Prefix` / `Suffix` (prepend/append text or icon)       |
| N/A            | —                                                         | `ShortcutKey` (keyboard shortcut binding)               |
| N/A            | —                                                         | `Invalid` (validation error message)                    |
