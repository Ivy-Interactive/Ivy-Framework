---
searchHints:
  - layout
  - vertical
  - horizontal
  - stack
  - arrangement
  - flexbox
---

# StackLayout

<Ingress>
StackLayout arranges child elements in either a vertical or horizontal stack with configurable spacing, alignment, and styling options. It's the foundation for creating linear [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) where elements are arranged sequentially in a single direction.
</Ingress>

The `StackLayout` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is the core building block for most layout compositions, offering flexible configuration for orientation, gaps between elements, padding, margins, [background colors](../../04_ApiReference/Ivy/Colors.md), and content [alignment](../../04_ApiReference/Ivy/Align.md). It can be used to create simple stacks or as the foundation for more complex layout systems.

## Basic Usage

Create simple stack using the helper methods:

```csharp demo-tabs
public class BasicStackExample : ViewBase
{
    public override object? Build()
    {   
        return new StackLayout([
            Text.H2("Stack"), 
            Text.Label("Creation of a simple Stack Layout")]);
    }
}
```

## Alignment

The `StackLayout` widget arranges child elements in a linear sequence with configurable orientation, spacing, alignment, and padding. This example demonstrates the core features:

```csharp demo-tabs
public class StackLayoutExample : ViewBase
{
    public override object? Build()
    {
        var box1 = new Box().Background(Colors.Primary).Width(Size.Units(2)).Height(Size.Units(2));
        var box2 = new Box().Background(Colors.Primary).Width(Size.Units(2)).Height(Size.Units(2));
        var box3 = new Box().Background(Colors.Primary).Width(Size.Units(2)).Height(Size.Units(2));

        return Layout.Vertical().Gap(4)
            | Text.H2("StackLayout Features")
            | Text.Label("Horizontal, Gap(2), Padding(1)")
            | (Layout.Horizontal().Gap(2).Padding(1) | box1 | box2 | box3)
            | Text.Label("Vertical, Gap(1), Align(Center), Padding(2)")
            | (Layout.Vertical().Gap(1).Align(Align.Center).Padding(2) | box1 | box2 | box3);
    }
}
```

## Advanced Features

Complete example showing padding, margins, background colors, and parent padding control. Use [Thickness](../../04_ApiReference/Ivy/Thickness.md) for padding and margin, and [Colors](../../04_ApiReference/Ivy/Colors.md) for background. Alignment options are in [Align](../../04_ApiReference/Ivy/Align.md):

```csharp demo-tabs
public class AdvancedStackLayoutExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Background(Colors.Primary).Width(Size.Units(2)).Height(Size.Units(2));

        return Layout.Vertical().Gap(2).Padding(8)
            | Text.H2("Advanced StackLayout Features")
            | Text.Label("With Margin (external spacing)")
            | (Layout.Horizontal().Margin(4) | box | box)
            | Text.Label("Remove Parent Padding, Background color")
            | (Layout.Horizontal().RemoveParentPadding().Background(Colors.Gray) | box | box);
    }
}
```

<Callout type="info">
StackLayout is the foundation for most other layout widgets. Understanding its properties will help you master more complex layout systems.
</Callout>

<WidgetDocs Type="Ivy.StackLayout" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Layouts/StackLayout.cs"/>

## Examples

<Details>
<Summary>
Navigation Bar
</Summary>
<Body>
Create a horizontal navigation bar with proper alignment:

```csharp demo-tabs
public class NavigationExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.Vertical().Gap(16).Padding(12).Align(Align.Center)
            // Navigation buttons
            | (Layout.Horizontal().Gap(8).Align(Align.Center)
                | new Button("Home", _ => client.Toast("Home"))
                | new Button("About", _ => client.Toast("About"))
                | new Button("Contact", _ => client.Toast("Contact"))
                | new Button("Settings", _ => client.Toast("Settings")))
            // App title and user info
            | (Layout.Vertical().Align(Align.Left)
                | Text.H3("MyApp")
                | Text.P("Welcome back!").Small())
            // User actions
            | (Layout.Horizontal().Gap(4).Align(Align.Right)
                | new Button("Profile", _ => client.Toast("Profile"))
                | new Button("Logout", _ => client.Toast("Logout")));
    }
}
```

</Body>
</Details>
