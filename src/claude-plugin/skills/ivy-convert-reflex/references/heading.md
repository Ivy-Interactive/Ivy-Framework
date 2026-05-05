# Heading

Displays a heading element (`<h1>` through `<h6>`) with configurable size, weight, alignment, and color. Used for semantic section titles and visual hierarchy.

## Reflex

```python
rx.heading("Welcome to Reflex", size="5", weight="bold", align="center")
rx.heading("Subheading", as_="h2", size="3", color_scheme="blue")
```

## Ivy

```csharp
Text.H1("Welcome to Ivy")
Text.H2("Subheading").Bold()
Text.H3("Smaller heading")
Text.H4("Smallest heading")
```

## Parameters

| Parameter      | Documentation                                                                 | Ivy                                                                 |
|----------------|-------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `as_`          | Change heading level (h1-h6) for semantic markup                              | Use `Text.H1` through `Text.H4` directly                           |
| `size`         | Heading size from `"1"` to `"9"` with auto line-height/letter-spacing         | Use `.Large()`, `.Small()`, `.Medium()` or specific H1-H4 variants |
| `weight`       | Font weight: `"light"`, `"regular"`, `"medium"`, `"bold"`                     | `.Bold()` modifier                                                  |
| `align`        | Text alignment: `"left"`, `"center"`, `"right"`                              | Not supported                                                       |
| `trim`         | Trim leading whitespace: `"normal"`, `"start"`, `"end"`, `"both"`            | Not supported                                                       |
| `color_scheme` | Apply a named color scheme (e.g. `"tomato"`, `"red"`, `"blue"`)              | `.Color(Colors.Primary)` etc.                                       |
| `high_contrast` | Increase color contrast with background                                      | Not supported                                                       |
| `as_child`     | Render as child element instead of standalone heading                         | Not supported                                                       |
