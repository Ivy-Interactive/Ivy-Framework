# JSON Explorer

A content area to display JSON data with collapsible nodes and syntax highlighting.

## Retool

```toolscript
// Set JSON data on the component
jsonExplorer1.setValue({
  name: "John Doe",
  age: 30,
  isActive: true,
  tags: ["developer", "designer", "architect"]
});

// Expand all nodes
jsonExplorer1.expandNodes = true;

// Scroll into view
jsonExplorer1.scrollIntoView({ behavior: 'smooth', block: 'center' });
```

## Ivy

```csharp
public class BasicJsonExample : ViewBase
{
    public override object? Build()
    {
        var simpleData = new
        {
            name = "John Doe",
            age = 30,
            isActive = true,
            tags = new[] { "developer", "designer", "architect" }
        };

        return Layout.Vertical().Gap(4)
            | new Json(System.Text.Json.JsonSerializer.Serialize(simpleData));
    }
}
```

## Parameters

| Parameter              | Documentation                                                          | Ivy                    |
|------------------------|------------------------------------------------------------------------|------------------------|
| `value`                | The JSON object to display                                             | `Content` (via constructor) |
| `expandNodes`          | Whether to expand all nodes                                            | Not supported          |
| `hidden`               | Whether the component is hidden from view                              | `Visible`              |
| `id`                   | The unique identifier (name)                                           | Not supported          |
| `isHiddenOnDesktop`    | Whether to hide in the desktop layout                                  | Not supported          |
| `isHiddenOnMobile`     | Whether to hide in the mobile layout                                   | Not supported          |
| `maintainSpaceWhenHidden` | Whether to take up space on the canvas when hidden                  | Not supported          |
| `margin`               | The amount of margin to render outside (`4px 8px` or `0`)              | Not supported          |
| `showInEditor`         | Whether the component remains visible in the editor when hidden        | Not supported          |
| `style`                | Custom style options                                                   | Not supported          |
| `html`                 | The HTML content                                                       | Not supported          |
| `scrollIntoView()`     | Scrolls the canvas so the component appears in the visible area        | Not supported          |
| N/A                    | N/A                                                                    | `Height`               |
| N/A                    | N/A                                                                    | `Width`                |
| N/A                    | N/A                                                                    | `Scale`                |
