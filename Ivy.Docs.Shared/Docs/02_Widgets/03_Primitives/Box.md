---
searchHints:
  - container
  - div
  - wrapper
  - rectangle
  - styling
  - layout
---

# Box

<Ingress>
Create versatile container elements with customizable borders, colors, and padding for grouping content and structuring layouts.
</Ingress>

The `Box` widget is a versatile container element that provides customizable borders, colors, padding, margins, and content alignment. It's perfect for visually grouping related content, creating distinct sections in your UI, and building card-based layouts.

## Basic Usage

By default, `Box` is unstyled and transparent, acting as a simple container. You can apply styling properties to create cards, headers, or alerts.

```csharp demo-tabs
public class BasicBoxExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(8)
            | Layout.Horizontal().Gap(4)
                | new Box("Clean Box").Padding(8).Width(Size.Fit())
                
                | new Box("Styled Box")
                    .Color(Colors.Primary)
                    .Padding(8)
                    .BorderRadius(BorderRadius.Rounded)
                    .Width(Size.Fit());
    }
}
```

<Callout Type="tip">
Use `Layout.Vertical()` or `Layout.Horizontal()` for structural layout, and `Box` when you need visual grouping or specific borders/backgrounds.
</Callout>
