# Text

A content area to display Markdown text or HTML content. Supports GitHub Flavored Markdown with images, links, and text formatting.

## Retool

```toolscript
text1.setValue("## Hello World\nThis is **bold** and *italic* text.")
text1.disableMarkdown = false
text1.horizontalAlign = "left"
text1.heightType = "auto"
```

## Ivy

Ivy provides several approaches for text rendering: `TextBlock` (via `Text` helper class) for styled text, and `Markdown` widget for full Markdown rendering.

```csharp
// Styled text via Text helper
Text.H1("Hello World");
Text.P("This is a paragraph.").Bold().Color(Colors.Primary);
Text.Muted("Secondary information");

// Full Markdown rendering
new Markdown("""
    ## Hello World
    This is **bold** and *italic* text.
    - List item one
    - List item two
    [Link](https://example.com)
    """);

// HTML content
Text.Html("<h2>Hello</h2><p>HTML content</p>");
```

## Parameters

| Parameter          | Documentation                                     | Ivy                                                       |
|--------------------|----------------------------------------------------|------------------------------------------------------------|
| `value`            | Current content value (string)                    | `Text.*()` content or `Markdown` content                   |
| `disableMarkdown`  | Render as plain text instead of Markdown           | Use `Text.Literal()` for plain, `new Markdown()` for MD   |
| `horizontalAlign`  | Horizontal alignment (left/center/right)           | Not supported (use layout)                                 |
| `verticalAlign`    | Vertical alignment (top/center/bottom)             | Not supported (use layout)                                 |
| `heightType`       | Fixed or auto height                               | `Height` property / `Size`                                 |
| `imageWidth`       | Image scaling behavior (fit/fill)                  | Not supported                                              |
| `hidden`           | Visibility toggle                                  | `Visible` property                                         |
| `margin`           | External spacing                                   | Not supported (use layout spacing)                         |
| `isHiddenOnMobile` | Hide on mobile layout                              | Not supported                                              |
| `isHiddenOnDesktop`| Hide on desktop layout                             | Not supported                                              |
| `maintainSpaceWhenHidden` | Reserve space when hidden                   | Not supported                                              |
| `style`            | Custom styling options                             | `.Bold()`, `.Italic()`, `.Color()`, `.Small()`, `.Large()` |
| `clearValue()`     | Removes current content                            | Not supported                                              |
| `resetValue()`     | Restores default value                             | Not supported                                              |
| `setValue(value)`   | Sets component content                            | Not supported (content set at build time)                  |
| `setHidden(hidden)`| Toggles visibility                                 | `Visible` property                                         |
| N/A                | N/A                                                | `Text.H1()`-`H4()` heading variants                       |
| N/A                | N/A                                                | `Text.Code()` with syntax highlighting                     |
| N/A                | N/A                                                | `Text.Latex()` for LaTeX formulas                          |
| N/A                | N/A                                                | `Markdown` OnLinkClick event for interactive links         |
