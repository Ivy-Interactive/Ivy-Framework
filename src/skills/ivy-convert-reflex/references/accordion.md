# Accordion

Vertically stacked interactive headings that reveal associated content sections. Composed of a root container and collapsible items.

## Reflex

```python
rx.accordion.root(
    rx.accordion.item(
        header="First Item",
        content="First Item Content"
    ),
    rx.accordion.item(
        header="Second Item",
        content="Second Item Content"
    ),
    rx.accordion.item(
        header="Third Item",
        content="Third Item Content"
    ),
    collapsible=True,
)
```

## Ivy

Ivy equivalent: [`Expandable`](https://docs.ivy.app/widgets/common/expandable)

```csharp
Layout.Vertical(
    new Expandable("First Item", "First Item Content"),
    new Expandable("Second Item", "Second Item Content"),
    new Expandable("Third Item", "Third Item Content")
)
```

## Parameters

| Parameter        | Documentation                                              | Ivy                                          |
|------------------|------------------------------------------------------------|-----------------------------------------------|
| `header`         | Trigger heading text or component                          | First constructor argument                    |
| `content`        | Collapsible content section                                | Second constructor argument                   |
| `type`           | `"single"` or `"multiple"` — controls how many items open  | Not supported (each Expandable is independent) |
| `default_value`  | Which item(s) are open by default                          | `Open(bool)` per individual Expandable        |
| `collapsible`    | Allows all items to close when `True`                      | Not supported (always collapsible)            |
| `disabled`       | Prevents interaction                                       | `Disabled()`                                  |
| `orientation`    | `"vertical"` or `"horizontal"` keyboard navigation         | Not supported                                 |
| `variant`        | Visual style (`"classic"`, `"soft"`, `"outline"`, etc.)    | Not supported                                 |
| `color_scheme`   | Background color assignment                                | Not supported                                 |
| `radius`         | Border radius sizing                                       | Not supported                                 |
| `show_dividers`  | Display dividers between items                             | Not supported                                 |
| `on_value_change`| Event fired when opened items change                       | Not supported                                 |
