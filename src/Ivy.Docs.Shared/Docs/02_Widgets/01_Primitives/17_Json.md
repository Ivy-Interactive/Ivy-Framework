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

## Object Overload

You can pass any object directly to the Json widget and it will be auto-serialized:

```csharp demo-tabs
public class ObjectJsonExample : ViewBase
{
    public override object? Build()
    {
        var dog = new { Breed = "Poodle", Color = "White", Age = 3 };
        return new Json(dog);
    }
}
```

## Expansion Control

Control how deeply the JSON tree is initially expanded:

```csharp demo-tabs
public class ExpandedJsonExample : ViewBase
{
    public override object? Build()
    {
        var data = System.Text.Json.JsonSerializer.Serialize(new
        {
            name = "John Doe",
            address = new { street = "123 Main St", city = "Anytown" },
            tags = new[] { "developer", "designer" }
        });

        return Layout.Vertical().Gap(4)
            | Text.P("Collapsed (default):")
            | new Json(data)
            | Text.P("Expanded 1 level:")
            | new Json(data) { Expanded = 1 }
            | Text.P("Fully expanded:")
            | new Json(data) { Expanded = -1 };
    }
}
```

<WidgetDocs Type="Ivy.Json" ExtensionTypes="Ivy.JsonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Json.cs"/>
