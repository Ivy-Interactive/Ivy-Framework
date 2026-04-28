# Code

Displays formatted code snippets. Reflex renders inline code with styling options, while Ivy provides a block-level code display with syntax highlighting, line numbers, and a copy button.

## Reflex

```python
rx.code("console.log()")
```

## Ivy

```csharp
new CodeBlock(@"console.log()")
    .Language(Languages.Javascript)
    .ShowLineNumbers()
    .ShowCopyButton()
```

## Parameters

| Parameter        | Documentation                                                            | Ivy                              |
|------------------|--------------------------------------------------------------------------|----------------------------------|
| content          | The code string to display                                               | `new CodeBlock(string content)`       |
| variant          | Visual style: "classic", "solid", etc.                                   | Not supported                    |
| size             | Text size ("1"-"9"), adjusts line height and letter spacing              | Not supported                    |
| weight           | Font weight: "light", "regular", "medium", "bold"                        | Not supported                    |
| color_scheme     | Color theme: "tomato", "red", "ruby", etc.                              | Not supported                    |
| high_contrast    | Increases color contrast with background                                 | Not supported                    |
| language         | Not supported (inline code, no syntax highlighting)                      | `.Language(Languages.Javascript)` |
| show_line_numbers| Not supported                                                            | `.ShowLineNumbers()`             |
| show_copy_button | Not supported                                                            | `.ShowCopyButton()`              |
| show_border      | Not supported                                                            | `.ShowBorder()`                  |
| width            | Standard CSS width                                                       | `.Width(Size.Full())`            |
| height           | Standard CSS height                                                      | `.Height(Size.Auto())`           |
