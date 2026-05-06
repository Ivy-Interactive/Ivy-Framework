# Text

A foundational text primitive for displaying text content. In Reflex it renders a `<p>` element with corrective line height and letter spacing. In Ivy the equivalent is `TextBlock`, accessed via the `Text` helper class.

## Reflex

```python
import reflex as rx

rx.text("The quick brown fox jumps over the lazy dog.")
rx.text("Bold text", weight="bold")
rx.text("Large centered", size="7", align="center")
rx.text("Tomato colored", color_scheme="tomato")
```

## Ivy

```csharp
public class TextDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.P("The quick brown fox jumps over the lazy dog.")
            | Text.Strong("Bold text")
            | Text.P("Large centered").Large()
            | Text.P("Colored text").Color(Colors.Red);
    }
}
```

## Parameters

| Parameter        | Documentation                                              | Ivy                                          |
|------------------|------------------------------------------------------------|----------------------------------------------|
| `size`           | Text size from `"1"` to `"9"`                              | `.Small()` / `.Medium()` / `.Large()`        |
| `weight`         | `"light"`, `"regular"`, `"bold"`                           | `.Bold()` / `.Italic()` or `Text.Strong()`   |
| `align`          | `"left"`, `"center"`, `"right"`                            | Not supported                                |
| `as_`            | Render as `"p"`, `"label"`, `"div"`, `"span"`              | Variant methods: `Text.P()`, `Text.Label()`  |
| `color_scheme`   | Named color scheme (`"tomato"`, `"red"`, etc.)             | `.Color(Colors.X)`                           |
| `high_contrast`  | Increases color contrast with background                   | Not supported                                |
| `trim`           | Trims leading whitespace: `"normal"`, `"start"`, `"end"`, `"both"` | Not supported                         |
| `as_child`       | Merges props onto child element                            | Not supported                                |
| N/A              | N/A                                                        | `.StrikeThrough()` — strikethrough styling   |
| N/A              | N/A                                                        | `.NoWrap()` — prevent line wrapping          |
| N/A              | N/A                                                        | `.Overflow()` — clip or ellipsis overflow    |
| N/A              | Emphasis via `rx.text.em`                                  | `.Italic()`                                  |
