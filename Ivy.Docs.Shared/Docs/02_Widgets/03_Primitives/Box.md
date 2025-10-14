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

The simplest way to create a Box is by passing content directly to the constructor.

```csharp demo-tabs
public class BasicBoxExample : ViewBase
{
    public override object? Build()
    {
        return new Box("Simple content");
    }
}
```

<Callout Type="tip">
Box widgets come with sensible defaults: Primary color, 2-unit borders with rounded corners, 2-unit padding, centered content, and no margin.
</Callout>

### Border Styling

Boxes support various border styles, thicknesses, and radius options.

```csharp demo-tabs
public class BorderStyleExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Box("Solid Border").BorderStyle(BorderStyle.Solid).Padding(8)
            | new Box("Dashed Border").BorderStyle(BorderStyle.Dashed).Padding(8)
            | new Box("Dotted Border").BorderStyle(BorderStyle.Dotted).Padding(8)
            | new Box("No Border").BorderStyle(BorderStyle.None).Padding(8);
    }
}
```

### Border Thickness

Control the width of borders using the `BorderThickness` property. You can specify a single value for uniform thickness or use the `Thickness` class for more precise control.

```csharp demo-tabs
public class BorderThicknessExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(4)
            | new Box("Thin Border")
                .BorderThickness(1)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Medium Border")
                .BorderThickness(2)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Thick Border")
                .BorderThickness(4)
                .Padding(8)
                .Width(Size.Fraction(1/3f));
    }
}
```

### Border Radius

Choose from different border radius options to create rounded corners. This affects the visual style and can range from sharp edges to fully rounded corners.

```csharp demo-tabs
public class BorderRadiusExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(4)
            | new Box("No Radius")
                .BorderRadius(BorderRadius.None)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Rounded")
                .BorderRadius(BorderRadius.Rounded)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Full Radius")
                .BorderRadius(BorderRadius.Full)
                .Padding(8)
                .Width(Size.Fraction(1/3f));
    }
}
```

### Basic Spacing

Control internal and external spacing using padding and margins.

```csharp demo-tabs
public class SpacingExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Box("No Padding").Padding(0)
            | new Box("Small Padding").Padding(4)
            | new Box("Large Padding").Padding(10)
            | new Box("With Margin").Margin(8).Padding(8);
    }
}
```

### Advanced Spacing

Use the `Thickness` class for more precise control over padding on different sides. This allows you to specify different spacing values for left, top, right, and bottom edges.

```csharp demo-tabs
public class AdvancedSpacingView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(8)
            | Layout.Horizontal().Gap(4)
                | new Box("No Padding")
                    .Width(Size.Fraction(1/2f))
                | new Box("Uniform Padding (8)")
                    .Padding(new Thickness(8))
                    .Width(Size.Fraction(1/2f))
            | Layout.Horizontal().Gap(4)
                | new Box("Horizontal/Vertical (16,8)")
                    .Padding(new Thickness(16, 8))
                    .Width(Size.Fraction(1/2f))
                | new Box("Asymmetric (24,12,6,18)")
                    .Padding(new Thickness(24, 12, 6, 18))
                    .Width(Size.Fraction(1/2f));
    }
}
```

### Content Alignment

Control how content is positioned within the box using the `ContentAlign` property.

```csharp demo-tabs
public class ContentAlignmentView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(8)
            | new Box("Top Left Alignment")
                .ContentAlign(Align.TopLeft)
                .Height(Size.Units(30))
                .Width(Size.Full())
                .Padding(8)
                .Content(
                    new Box("Small").Color(Colors.White),
                    new Box("Medium").Width(Size.Units(20)).Height(Size.Units(8)).Color(Colors.White)
                )
            | new Box("Center Alignment")
                .ContentAlign(Align.Center)
                .Height(Size.Units(30))
                .Width(Size.Full())
                .Padding(8)
                .Content(
                    new Box("Small").Color(Colors.White),
                    new Box("Medium").Width(Size.Units(20)).Height(Size.Units(8)).Color(Colors.White)
                )
            | new Box("Bottom Right Alignment")
                .ContentAlign(Align.BottomRight)
                .Height(Size.Units(30))
                .Width(Size.Full())
                .Padding(8)
                .Content(
                    new Box("Small").Color(Colors.White),
                    new Box("Medium").Width(Size.Units(20)).Height(Size.Units(8)).Color(Colors.White)
                );
    }
}
```

### Sizing

Control the dimensions of your boxes.

```csharp demo-tabs
public class SizingExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Box("Auto Width")
                .Width(Size.Auto())
                .Padding(8)
            | new Box("Fixed Width")
                .Width(Size.Units(45))
                .Padding(8)
            | new Box("Full Width")
                .Width(Size.Full())
                .Padding(8)
            | new Box("Fixed Size")
                .Width(Size.Units(60))
                .Height(Size.Units(10))
                .Padding(8);
    }
}
```

### Colors

Boxes support a wide range of predefined colors that automatically adapt to light/dark themes.

```csharp demo-tabs
public class ColorExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Box("Primary Color").Color(Colors.Primary).Padding(8);
    }
}
```

For more colors, look at the [Ivy Colors page](../../03_ApiReference/IvyShared/Colors.md)

<WidgetDocs Type="Ivy.Box" ExtensionTypes="Ivy.BoxExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Box.cs"/>

## Examples

<Details>
<Summary>
Status Dashboard
</Summary>
<Body>
Create a dashboard with status indicators using different colors and styles. This example demonstrates how to use boxes for displaying system status information with appropriate visual cues.

```csharp demo-tabs
public class StatusDashboardView : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(4)
            | new Box("System Online")
                .Color(Colors.Green)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(2)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Warning: High CPU Usage")
                .Color(Colors.Yellow)
                .BorderStyle(BorderStyle.Dashed)
                .BorderThickness(2)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Database Error")
                .Color(Colors.Red)
                .BorderThickness(2)
                .Padding(8)
                .Width(Size.Fraction(1/3f));
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Card Layout
</Summary>
<Body>
Build card-based layouts with consistent styling for displaying structured information. This example shows how to create professional-looking cards with proper spacing, borders, and content organization.

```csharp demo-tabs
public class CardLayoutView : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(2).Gap(8)
            | new Box()
                .Color(Colors.White)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(1)
                .Padding(12)
                .Content(
                    Text.Label("User Profile"),
                    Text.P("John Doe"),
                    Text.P("Software Developer")
                )
            | new Box()
                .Color(Colors.White)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(1)
                .Padding(12)
                .Content(
                    Text.Label("Statistics"),
                    Text.P("Projects: 15"),
                    Text.P("Experience: 5 years")
                );
    }
}
```

</Body>
</Details>
