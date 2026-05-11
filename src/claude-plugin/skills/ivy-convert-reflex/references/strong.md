# Strong

Marks text to signify strong importance (bold emphasis).

## Reflex

```python
rx.text(
    "The most important thing to remember is, ",
    rx.text.strong("stay positive"),
    "."
)
```

## Ivy

```csharp
Text.Strong("stay positive")
```

## Parameters

| Parameter | Documentation                              | Ivy           |
|-----------|--------------------------------------------|---------------|
| children  | The text content to render as strong/bold  | First argument to `Text.Strong()` |

No component-specific props in either framework. Both `rx.text.strong` and `Text.Strong` are simple wrappers that apply bold/strong emphasis to their text content.
