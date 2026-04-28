# Em (Emphasis)

Marks text to stress emphasis. Renders as italic (`<em>`) HTML element to indicate stressed text within a sentence.

## Reflex

```python
rx.text("We ", rx.text.em("had"), " to do something about it.")
```

## Ivy

```csharp
Text.P("We had to do something about it.").Italic()
```

## Parameters

| Parameter              | Documentation                        | Ivy           |
|------------------------|--------------------------------------|---------------|
| Component-specific props | No component-specific props         | `.Italic()` modifier on `TextBuilder` |
| Event triggers         | Default event triggers supported     | Not supported |
