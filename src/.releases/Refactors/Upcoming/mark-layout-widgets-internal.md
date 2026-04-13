# Breaking Change: Layout widgets marked as internal

## What changed

`TabsLayout`, `StackLayout`, and `GridLayout` classes (and their extension classes) are now `internal`.  
External consumers can no longer instantiate them directly with `new TabsLayout(...)`, `new StackLayout(...)`, or `new GridLayout(...)`.

## Migration

Replace direct widget instantiation with the `Layout.*` fluent API:

| Before (broken) | After |
|------------------|-------|
| `new StackLayout([...], Orientation.Vertical)` | `Layout.Vertical() \| child1 \| child2` |
| `new StackLayout([...], Orientation.Horizontal)` | `Layout.Horizontal() \| child1 \| child2` |
| `new StackLayout([...], wrap: true)` | `Layout.Wrap() \| child1 \| child2` |
| `new GridLayout(new GridDefinition { Columns = 2 }, ...)` | `Layout.Grid().Columns(2) \| child1 \| child2` |
| `new TabsLayout(null, null, null, null, 0, tabs)` | `Layout.Tabs(tabs)` |

### Advanced TabsLayout features

`TabView` (returned by `Layout.Tabs()`) now supports advanced features via fluent API:

```csharp
Layout.Tabs(tabs)
    .Variant(TabsVariant.Tabs)
    .OnSelect(index => { ... })
    .OnClose(index => { ... })
    .OnCloseOthers(index => { ... })
    .OnRefresh(index => { ... })
    .AddButton("+", () => { ... })
    .SelectedIndex(selectedIndex)
```

## Why

Reducing the public API surface to encourage consistent usage patterns and prevent direct widget construction that bypasses the fluent layout system.
