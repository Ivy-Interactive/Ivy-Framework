# Code Block

Displays formatted code snippets with syntax highlighting. Supports multiple programming languages, line numbers, and long-line wrapping.

## Reflex

```python
rx.code_block(
    """function fibonacci(n) {
  if (n <= 1) return n;
  return fibonacci(n-1) + fibonacci(n-2);
}""",
    language="javascript",
    show_line_numbers=True,
    theme="one_light",
)
```

## Ivy

```csharp
new CodeBlock(@"function fibonacci(n) {
  if (n <= 1) return n;
  return fibonacci(n-1) + fibonacci(n-2);
}")
    .Language(Languages.Javascript)
    .ShowLineNumbers()
    .ShowCopyButton()
```

## Parameters

| Parameter            | Documentation                                       | Ivy                              |
|----------------------|-----------------------------------------------------|----------------------------------|
| `code`               | The code string to display                          | Constructor parameter `content`  |
| `language`           | Language for syntax highlighting (default: python)  | `.Language()` (default: C#)      |
| `theme`              | Color theme (default: `one_light`)                  | Not supported                    |
| `show_line_numbers`  | Toggle line number display                          | `.ShowLineNumbers()`             |
| `starting_line_number` | Starting line number offset                       | Not supported                    |
| `wrap_long_lines`    | Wrap long lines instead of horizontal scroll        | Not supported                    |
| `code_tag_props`     | Additional props for the inner code tag             | Not supported                    |
| N/A                  | N/A                                                 | `.ShowCopyButton()` copy button  |
| N/A                  | N/A                                                 | `.ShowBorder()` toggle border    |
