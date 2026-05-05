# Hover Card

Displays rich preview content when a user hovers over a trigger element (typically a link). Useful for showing supplementary information without requiring a click.

## Reflex

```python
rx.hover_card.root(
    rx.hover_card.trigger(
        rx.link("Hover over me", color_scheme="blue", underline="always"),
    ),
    rx.hover_card.content(
        rx.text("This is the hovercard content."),
    ),
)
```

## Ivy

Ivy does not have a dedicated HoverCard widget. The closest equivalent is the **Tooltip**, which shows contextual information on hover. Unlike Reflex's HoverCard, the Tooltip is primarily designed for short text hints, though the constructor accepts any object as content.

```csharp
new Button("Hover over me").WithTooltip("This is the tooltip content.")

// Or using the constructor for richer content:
new Tooltip(
    trigger: new Button("Hover over me"),
    content: "This is the tooltip content."
)
```

## Parameters

### Root (`rx.hover_card.root`)

| Parameter          | Documentation                                     | Ivy           |
|--------------------|----------------------------------------------------|---------------|
| `default_open`     | Sets the initial open state (uncontrolled)          | Not supported |
| `open`             | Controls the open state (controlled)                | Not supported |
| `open_delay`       | Milliseconds to wait before opening                 | Not supported |
| `close_delay`      | Milliseconds to wait before closing                 | Not supported |
| `on_open_change`   | Event fired when the open state changes             | Not supported |

### Content (`rx.hover_card.content`)

| Parameter            | Documentation                                       | Ivy           |
|----------------------|------------------------------------------------------|---------------|
| `side`               | Positioning side relative to trigger (`"top"`, `"right"`, `"bottom"`, `"left"`) | Not supported |
| `side_offset`        | Distance in pixels from the trigger edge             | Not supported |
| `align`              | Alignment within the positioned area (`"start"`, `"center"`, `"end"`) | Not supported |
| `align_offset`       | Offset from the alignment point                      | Not supported |
| `avoid_collisions`   | Prevents the content from overflowing the viewport   | Not supported |
| `collision_padding`  | Padding around collision boundary detection           | Not supported |
| `sticky`             | Behavior when trigger moves (`"partial"`, `"always"`) | Not supported |
| `hide_when_detached` | Hides content if the trigger element is detached      | Not supported |
| `size`               | Size variant (`"1"`, `"2"`, `"3"`)                   | Not supported |

### Trigger (`rx.hover_card.trigger`)

No component-specific props in either framework.
