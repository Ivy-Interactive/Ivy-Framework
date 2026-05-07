# Scroll Area

Custom styled, cross-browser scrollable area using native functionality. Provides a container that displays scrollbars when content overflows, with control over which axes are scrollable and how scrollbars appear.

## Reflex

```python
rx.scroll_area(
    rx.flex(
        rx.text("Long content that overflows..."),
        direction="column",
        gap="4",
    ),
    scrollbars="vertical",
    type="auto",
    scroll_hide_delay=300,
    style={"height": 150},
)
```

## Ivy

Not supported. Ivy does not have a dedicated ScrollArea widget or equivalent. The `Box` primitive provides containers with borders, padding, and alignment but does not expose overflow or scroll behavior properties.

```csharp
// No direct equivalent in Ivy.
// A Box can constrain content size but has no scroll properties:
new Box(
    new Text("Long content...")
).Height(Size.Px(150));
```

## Parameters

| Parameter           | Documentation                                                                                              | Ivy           |
|---------------------|------------------------------------------------------------------------------------------------------------|---------------|
| `scrollbars`        | Controls which axes allow scrolling. Values: `"vertical"`, `"horizontal"`, `"both"`                        | Not supported |
| `type`              | Scrollbar visibility mode. `"auto"`: on overflow, `"always"`: always visible, `"scroll"`: while scrolling, `"hover"`: while scrolling or hovering | Not supported |
| `scroll_hide_delay` | Delay in milliseconds before scrollbars hide (applies when `type` is `"scroll"` or `"hover"`)              | Not supported |
