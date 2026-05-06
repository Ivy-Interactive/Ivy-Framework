# Blockquote

A block-level extended quotation component used to visually distinguish quoted text from surrounding content.

## Reflex

```python
rx.blockquote(
    "Perfect typography is certainly the most elusive of all arts.",
    size="3",
    weight="regular",
    color_scheme="tomato",
    high_contrast=True,
    cite="https://example.com/source",
)
```

## Ivy

```csharp
Text.Blockquote("Perfect typography is certainly the most elusive of all arts.")
    .Color(Colors.Danger)
    .Large()
    .Bold()
```

## Parameters

| Parameter      | Documentation                                                        | Ivy                                                       |
|----------------|----------------------------------------------------------------------|-----------------------------------------------------------|
| text (content) | The quoted text to display                                           | First argument to `Text.Blockquote()`                     |
| size           | Controls blockquote size (`"1"` through `"9"`)                       | `.Small()` / `.Medium()` / `.Large()`                     |
| weight         | Font weight (`"light"`, `"regular"`, `"medium"`, `"bold"`)           | `.Bold()` modifier (no light/medium granularity)          |
| color_scheme   | Sets a specific color (`"tomato"`, `"red"`, `"blue"`, etc.)          | `.Color(Colors.X)` using the `Colors` enum                |
| high_contrast  | Increases color contrast with the background                         | Not supported                                             |
| cite           | Specifies citation URL for the quotation                             | Not supported                                             |
