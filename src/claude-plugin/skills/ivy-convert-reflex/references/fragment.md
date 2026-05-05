# Fragment

Groups multiple components without rendering a wrapper node in the DOM. Useful for returning multiple elements from a function, conditional rendering, and avoiding unnecessary DOM nesting.

## Reflex

```python
rx.fragment(
    rx.text("Component1"),
    rx.text("Component2"),
)
```

## Ivy

```csharp
new Fragment(
    Text.P("Component1"),
    Text.P("Component2")
)
```

## Parameters

| Parameter | Documentation                                          | Ivy           |
|-----------|--------------------------------------------------------|---------------|
| children  | Child components to group without a wrapper DOM element | Supported (constructor argument) |
| Height    | Not supported                                          | `Size` (read-only, inherited) |
| Width     | Not supported                                          | `Size` (read-only, inherited) |
| Scale     | Not supported                                          | `Scale?` (read-only, inherited) |
| Visible   | Not supported                                          | `bool` (read-only, inherited) |

Both frameworks model Fragment as a minimal grouping primitive with no component-specific props. Ivy exposes inherited base-widget properties (`Height`, `Width`, `Scale`, `Visible`) that Reflex does not surface on Fragment.
