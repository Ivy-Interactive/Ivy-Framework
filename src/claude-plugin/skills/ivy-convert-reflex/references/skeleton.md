# Skeleton

A loading placeholder component that mimics content structure while data is being fetched. It improves perceived performance by showing users the layout of the page during loading.

## Reflex

```python
rx.vstack(
    rx.skeleton(rx.button("button-small"), height="10px"),
    rx.skeleton(rx.button("button-big"), height="20px"),
    rx.skeleton(
        rx.text("Text is loaded."),
        loading=True,
    ),
    rx.skeleton(
        rx.text("Text is already loaded."),
        loading=False,
    ),
)
```

## Ivy

```csharp
// Ivy uses conditional rendering to swap between Skeleton placeholders and real content.
var isLoading = UseState(false);

return Layout.Vertical().Gap(3)
    | (isLoading.Value
        ? Layout.Vertical().Gap(3)
            | new Skeleton().Height(Size.Units(40)).Width(Size.Full())
            | new Skeleton().Height(Size.Units(24)).Width(Size.Units(40))
            | new Skeleton().Height(Size.Units(16)).Width(Size.Full())
        : Layout.Vertical().Gap(3)
            | new Image(product.Value?.ImageUrl).Height(Size.Units(40))
            | Text.H3(product.Value?.Name));
```

## Parameters

| Parameter    | Documentation                                                                 | Ivy                                                                 |
|--------------|-------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `width`      | Sets the skeleton width                                                       | `Width` (e.g. `.Width(Size.Full())`)                                |
| `min_width`  | Sets minimum width constraint                                                 | Not supported                                                       |
| `max_width`  | Sets maximum width constraint                                                 | Not supported                                                       |
| `height`     | Sets the skeleton height                                                      | `Height` (e.g. `.Height(Size.Units(40))`)                           |
| `min_height` | Sets minimum height constraint                                                | Not supported                                                       |
| `max_height` | Sets maximum height constraint                                                | Not supported                                                       |
| `loading`    | Controls whether the skeleton or its children are displayed (wraps children)  | Not supported — use conditional rendering to swap skeleton and content |
