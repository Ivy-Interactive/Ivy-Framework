# Rich Text Editor

An input field to enter text with rich formatting (bold, italic, headings, lists, links, etc.). Retool provides a full WYSIWYG rich text editor component that outputs HTML.

## Retool

```toolscript
richTextEditor1.setValue("<p>Hello <strong>World</strong></p>")

// Reading the value
richTextEditor1.value // returns HTML string

// Events
richTextEditor1.events = [
  { type: "change", handler: someQuery },
  { type: "blur",   handler: anotherQuery },
  { type: "focus",  handler: focusQuery }
]
```

## Ivy

Ivy does not have a built-in Rich Text Editor widget. The closest alternatives are:

- **`Html`** — renders raw HTML content (display only, no editing)
- **`TextInput` (Textarea)** — multi-line plain text input (no rich formatting)
- **`CodeInput` (HTML language)** — code editing with syntax highlighting (not WYSIWYG)

```csharp
// Display-only: render rich HTML content
new Html("<p>Hello, <strong>World</strong>!</p>")

// Plain text editing (no rich formatting)
var text = UseState("");
text.ToTextInput()
    .Variant(TextInputVariants.Textarea)
    .Placeholder("Enter text...")

// HTML code editing with syntax highlighting (not WYSIWYG)
var html = UseState("");
html.ToCodeInput()
    .Language(Languages.Html)
    .Placeholder("Enter HTML...")
```

## Parameters

| Parameter                | Documentation                                          | Ivy                                                        |
|--------------------------|--------------------------------------------------------|------------------------------------------------------------|
| `id`                     | Unique identifier for the component                    | Not needed (variable name serves as identifier)            |
| `value`                  | The current input value (HTML string, read-only)       | `Html.Content` (display only) / `TextInput.Value` (plain)  |
| `hidden`                 | Controls visibility                                    | `Html.Visible` / `TextInput.Visible`                       |
| `margin`                 | Outer spacing (`"0"` or `"4px 8px"`)                   | Not supported                                              |
| `formDataKey`            | Key used when inside a Form component                  | Not supported                                              |
| `isHiddenOnDesktop`      | Hide on desktop layout                                 | Not supported                                              |
| `isHiddenOnMobile`       | Hide on mobile layout                                  | Not supported                                              |
| `maintainSpaceWhenHidden`| Reserve canvas space when hidden                       | Not supported                                              |
| `showInEditor`           | Remain visible in editor when hidden                   | Not supported                                              |
| `style`                  | Custom styling options                                 | Not supported                                              |
| `selection`              | Cursor position and selection length (read-only)       | Not supported                                              |
| **Events**               |                                                        |                                                            |
| `onChange`               | Triggered when value changes                           | `TextInput.OnChange`                                       |
| `onBlur`                 | Triggered when field loses focus                       | `TextInput.OnBlur`                                         |
| `onFocus`                | Triggered when field gains focus                       | Not supported                                              |
| **Methods**              |                                                        |                                                            |
| `setValue()`             | Updates the component's current value                  | `state.Set()` (for TextInput)                              |
| `setSelection()`        | Modifies current text selection                        | Not supported                                              |
| `scrollIntoView()`      | Scrolls the component into the visible area            | Not supported                                              |
