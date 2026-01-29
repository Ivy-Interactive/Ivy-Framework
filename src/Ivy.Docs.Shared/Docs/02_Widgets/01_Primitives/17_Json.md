---
searchHints:
  - data
  - format
  - json
  - syntax
  - structure
  - object
---

# Json

The `Json` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays JSON data in a formatted, syntax-highlighted view. It's useful for debugging, data visualization, and displaying API responses.

## Basic Usage

The simplest way to display JSON data is by passing a serialized string directly to the Json widget.

```csharp demo-tabs
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

<WidgetDocs Type="Ivy.Json" ExtensionTypes="Ivy.JsonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Json.cs"/>
