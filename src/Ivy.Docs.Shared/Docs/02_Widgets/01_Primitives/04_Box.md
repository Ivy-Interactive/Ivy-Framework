---
searchHints:
  - box
  - container
  - div
  - wrapper
  - rectangle
  - styling
  - layout
---

# Box

<Ingress>
Create versatile container elements with customizable borders, colors, and padding for grouping content and structuring [layouts](../../01_Onboarding/02_Concepts/04_Layout.md).
</Ingress>

The `Box` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a versatile container element that provides customizable borders, colors, padding, margins, and content alignment. It's perfect for visually grouping related content, creating distinct sections in your [UI](../../01_Onboarding/02_Concepts/02_Views.md), and building [card](../03_Common/04_Card.md)-based layouts.

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

Control the width of borders using the `BorderThickness` property. You can specify a single value for uniform thickness or use the [Thickness](../../04_ApiReference/Ivy/Thickness.md) class for more precise control.

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

### Aspect Ratio

Set a width and an aspect ratio — the height adjusts automatically. Pass the ratio as a float: `16f / 9f` for widescreen, `1f` for square. Works on any widget since it's defined on `WidgetBase`.

```csharp demo-tabs
public class AspectRatioExamplesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(4)
            | new Box("16:9").Width(Size.Units(80)).AspectRatio(16f / 9f).Background(Colors.Primary)
            | new Box("4:3").Width(Size.Units(80)).AspectRatio(4f / 3f).Background(Colors.Secondary)
            | new Box("1:1").Width(Size.Units(40)).AspectRatio(1f).Background(Colors.Warning);
    }
}
```

### Advanced Features & Interactions

Use the [Thickness](../../04_ApiReference/Ivy/Thickness.md) class for more precise control over padding on different sides. This allows you to specify different spacing values for left, top, right, and bottom edges.

Boxes also support a wide range of [Colors](../../04_ApiReference/Ivy/Colors.md), and interactive states using `OnClick` and `Hover()`.

The following example demonstrates combining these features to create status indicators, interactive selections, and professional card layouts.

```csharp demo-tabs
public class BoxFeaturesView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var selected = UseState("Option A");

        return Layout.Vertical().Gap(8)
            | Layout.Grid().Columns(2).Gap(8)
                // Advanced Spacing & Colors
                | new Box("Asymmetric Padding & Primary Color")
                    .Padding(new Thickness(24, 12, 6, 18))
                    .Width(Size.Fraction(1/2f));
    }
}
```

### Interactive States

Boxes support hover effects and click events.

```csharp demo-tabs
public class InteractiveBoxView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var selected = UseState("Option A");

        return Layout.Vertical().Gap(8)
            | new Box("Pointer + Translate Hover")
                .Hover(HoverEffect.PointerAndTranslate)
                .OnClick(_ => client.Toast("Box clicked!"))
                .Padding(8)
            | Layout.Horizontal().Gap(4)
                | CreateOption("Option A", selected, client)
                | CreateOption("Option B", selected, client);
    }

    private Box CreateOption(string label, IState<string> selected, IClientProvider client)
    {
        var isSelected = selected.Value == label;
        return new Box(label)
            .Background(isSelected ? Colors.Primary : Colors.Muted)
            .BorderThickness(isSelected ? 2 : 1)
            .Hover(HoverEffect.Pointer)
            .OnClick(_ => {
                selected.Set(label);
                client.Toast($"Selected: {label}");
            })
            .Padding(8)
            .Width(Size.Fraction(1/2f));
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
            | new Box("Primary Color").Background(Colors.Primary).Padding(8);
    }
}
```

For more colors, see the [Colors](../../04_ApiReference/Ivy/Colors.md) reference.

## Faq

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
                .Background(Colors.Green)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(2)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Warning: High CPU Usage")
                .Background(Colors.Yellow)
                .BorderStyle(BorderStyle.Dashed)
                .BorderThickness(2)
                .Padding(8)
                .Width(Size.Fraction(1/3f))
            | new Box("Database Error")
                .Background(Colors.Red)
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
                .Background(Colors.White)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(1)
                .Padding(12)
                .Content(
                    Text.Label("User Profile"),
                    Text.P("John Doe"),
                    Text.P("Software Developer")
                );
    }

    private Box CreateOption(string label, IState<string> selected, IClientProvider client)
    {
        var isSelected = selected.Value == label;
        return new Box(label)
            .Background(isSelected ? Colors.Primary : Colors.Muted)
            .BorderThickness(isSelected ? 2 : 1)
            .Hover(HoverEffect.Pointer)
            .OnClick(_ => {
                selected.Set(label);
                client.Toast($"Selected: {label}");
            })
            .Padding(8)
            .Width(Size.Fraction(1/2f));
    }
}
```

</Body>
</Details>

<Details>
<Summary>
How do I create a circular shape or circle in Ivy?
</Summary>
<Body>

There is no dedicated Shape or Circle widget. Use a `Box` with `BorderRadius.Full` and equal width and height to create a circle:

```csharp
// A colored circle
new Box()
    .Background(Colors.Primary)
    .Width(Size.Px(36))
    .Height(Size.Px(36))
    .BorderRadius(BorderRadius.Full)

// A circle with content centered inside
new Box(Text.P("A"))
    .Color(Colors.Slate)
    .Width(Size.Px(48))
    .Height(Size.Px(48))
    .ContentAlign(Align.Center)
    .BorderRadius(BorderRadius.Full)
```

`BorderRadius.Full` makes the box fully rounded. When width and height are equal, this produces a perfect circle. Use `BorderRadius.Rounded` for rounded corners instead.

</Body>
</Details>

<Details>
<Summary>
How do I apply styling (width, height, color, padding) to Ivy components?
</Summary>
<Body>

Ivy uses a fluent API for styling — there is no `.Style()` method for arbitrary CSS. Use the built-in extension methods:

```csharp
new Box(content)
    .Width(Size.Px(200))
    .Height(Size.Px(100))
    .Background(Colors.Blue)
    .Padding(16)
    .Margin(8)
    .BorderRadius(BorderRadius.Rounded)
    .BorderStyle(BorderStyle.Solid)
```

For CSS transforms, rotations, or complex visual effects that can't be expressed with Ivy's styling API, use the `Html` widget with inline styles:

```csharp
new Html($"<div style='transform: rotate({degrees}deg); width: 100px; height: 2px; background: #000;'></div>")
    .DangerouslyAllowScripts()
```

Note: The `Html` widget renders in an iframe. CSS variables like `var(--primary)` do not resolve — use hardcoded color values.

</Body>
</Details>

<WidgetDocs Type="Ivy.Box" ExtensionTypes="Ivy.BoxExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Box.cs"/>
