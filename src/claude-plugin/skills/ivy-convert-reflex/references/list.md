# List

Displays a vertical collection of items. Supports ordered (numbered) and unordered (bulleted) variants, customizable markers, and icon integration.

## Reflex

```python
import reflex as rx

# Unordered list
rx.list.unordered(
    rx.list.item("Example 1"),
    rx.list.item("Example 2"),
    rx.list.item("Example 3"),
)

# Ordered list
rx.list.ordered(
    rx.list.item("Example 1"),
    rx.list.item("Example 2"),
    rx.list.item("Example 3"),
)

# List from iterable, no bullet markers
rx.list.unordered(
    rx.list.item("Example 1"),
    rx.list.item("Example 2"),
    list_style_type="none",
)
```

## Ivy

```csharp
public class BasicListDemo : ViewBase
{
    public override object? Build()
    {
        var items = new[]
        {
            new ListItem("Apple"),
            new ListItem("Banana"),
            new ListItem("Cherry"),
        };
        return new List(items);
    }
}
```

## Parameters

| Parameter         | Documentation                                                  | Ivy                                                    |
|-------------------|----------------------------------------------------------------|--------------------------------------------------------|
| `items`           | Iterable collection of items to display                        | Passed via constructor (`new List(items)`)             |
| `list_style_type` | Controls bullet/number style (`"none"`, `"disc"`, `"decimal"`) | Not supported                                          |
| `ordered`         | Renders numbered list (`rx.list.ordered`)                      | Not supported                                          |
| `unordered`       | Renders bulleted list (`rx.list.unordered`)                    | Default behavior                                       |
| `title`           | Not supported                                                  | `ListItem.title` — primary text label                  |
| `subtitle`        | Not supported                                                  | `ListItem.subtitle` — secondary descriptive text       |
| `icon`            | Supported via nested icon components in `rx.list.item`         | `ListItem.icon` — built-in icon property               |
| `badge`           | Not supported                                                  | `ListItem.badge` — label badge on the item             |
| `onClick`         | Via `on_click` event handler on items                          | `ListItem.onClick` — `Action<Event<ListItem>>` handler |
| `Height`          | Via CSS styling                                                | `List.Height` — `Size` property                        |
| `Width`           | Via CSS styling                                                | `List.Width` — `Size` property                         |
| `Visible`         | Via `display` CSS or conditional rendering                     | `List.Visible` — `bool` property                       |
