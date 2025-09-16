# StackLayout

<Ingress>
StackLayout arranges child elements in either a vertical or horizontal stack with configurable spacing, alignment, and styling options. It's the foundation for creating linear layouts where elements are arranged sequentially in a single direction.
</Ingress>

The `StackLayout` widget is the core building block for most layout compositions, offering flexible configuration for orientation, gaps between elements, padding, margins, background colors, and content alignment. It can be used to create simple stacks or as the foundation for more complex layout systems.

## Basic Usage

Create simple vertical and horizontal stacks using the helper methods:

```csharp demo-tabs
public class BasicStackExample : ViewBase
{
    public override object? Build()
    {
        var squareBox = new Box().Width(5).Height(5);
        
        return Layout.Vertical()
            | Text.H2("Vertical Stack")
            | Layout.Vertical() | squareBox | squareBox | squareBox
            | Text.H2("Horizontal Stack") 
            | (Layout.Horizontal() | squareBox | squareBox | squareBox);
    }
}
```

## Orientation

Control the direction in which elements are arranged:

```csharp demo-tabs
public class OrientationExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Width(4).Height(4).Color(Colors.Green);
        
        return Layout.Vertical()
            | Text.H2("Vertical Orientation (Default)")
            | new StackLayout([box, box, box], Orientation.Vertical)
            | Text.H2("Horizontal Orientation")
            | new StackLayout([box, box, box], Orientation.Horizontal);
    }
}
```

## Gap Control

Set the spacing between child elements:

```csharp demo-tabs
public class GapExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Width(4).Height(4).Color(Colors.Orange);
        
        return Layout.Vertical()
            | Text.H2("No Gap")
            | Layout.Horizontal().Gap(0) | box | box | box
            | Text.H2("Small Gap (2px)")
            | Layout.Horizontal().Gap(2) | box | box | box
            | Text.H2("Default Gap (4px)")
            | Layout.Horizontal().Gap(4) | box | box | box
            | Text.H2("Large Gap (8px)")
            | Layout.Horizontal().Gap(8) | box | box | box;
    }
}
```

## Alignment

Control how children are positioned within the stack:

```csharp demo-tabs
public class AlignmentExample : ViewBase
{
    public override object? Build()
    {
        var smallBox = new Box().Width(3).Height(3).Color(Colors.Red);
        var largeBox = new Box().Width(6).Height(6).Color(Colors.Blue);
        
        return Layout.Vertical()
            | Text.H2("Horizontal Stack Alignment")
            | new Box(Layout.Horizontal().Align(Align.Left) | smallBox | largeBox)
                .Width(20).Height(12).Color(Colors.Gray).Padding(0)
            | new Box(Layout.Horizontal().Align(Align.Center) | smallBox | largeBox)
                .Width(20).Height(12).Color(Colors.Gray).Padding(0)
            | new Box(Layout.Horizontal().Align(Align.Right) | smallBox | largeBox)
                .Width(20).Height(12).Color(Colors.Gray).Padding(0)
            | Text.H2("Vertical Stack Alignment")
            | new Box(Layout.Vertical().Align(Align.TopLeft) | smallBox | largeBox)
                .Width(20).Height(12).Color(Colors.Gray).Padding(0)
            | new Box(Layout.Vertical().Align(Align.Center) | smallBox | largeBox)
                .Width(20).Height(12).Color(Colors.Gray).Padding(0)
            | new Box(Layout.Vertical().Align(Align.BottomRight) | smallBox | largeBox)
                .Width(20).Height(12).Color(Colors.Gray).Padding(0);
    }
}
```

## Padding and Margins

Add internal and external spacing:

```csharp demo-tabs
public class SpacingExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Width(4).Height(4).Color(Colors.Purple);
        
        return Layout.Vertical()
            | Text.H2("With Padding")
            | new StackLayout([box, box], Orientation.Horizontal, padding: new Thickness(8), background: Colors.Gray)
            | Text.H2("With Margin")
            | new StackLayout([box, box], Orientation.Horizontal, margin: new Thickness(8), background: Colors.Gray)
            | Text.H2("Combined")
            | new StackLayout([box, box], Orientation.Horizontal, 
                padding: new Thickness(4), margin: new Thickness(8), background: Colors.Gray);
    }
}
```

## Background Colors

Add visual distinction to your stacks:

```csharp demo-tabs
public class BackgroundExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Width(4).Height(4).Color(Colors.White);
        
        return Layout.Vertical()
            | Text.H2("Colored Backgrounds")
            | Layout.Horizontal()
                | new StackLayout([box, box], Orientation.Horizontal, background: Colors.Blue)
                | new StackLayout([box, box], Orientation.Horizontal, background: Colors.Green)
                | new StackLayout([box, box], Orientation.Horizontal, background: Colors.Red);
    }
}
```

## Remove Parent Padding

Extend stacks to fill the full available space:

```csharp demo-tabs
public class ParentPaddingExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Width(4).Height(4).Color(Colors.Yellow);
        
        return Layout.Vertical().Padding(16)
            | Text.H2("Normal Stack (Respects Parent Padding)")
            | new StackLayout([box, box], Orientation.Horizontal, background: Colors.Gray)
            | Text.H2("Removed Parent Padding (Full Width)")
            | new StackLayout([box, box], Orientation.Horizontal, removeParentPadding: true, background: Colors.Gray);
    }
}
```

## Complex Layouts

Combine multiple stacks for sophisticated layouts:

```csharp demo-tabs
public class ComplexLayoutExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var name = UseState("John Doe");
        var email = UseState("john@example.com");
        
        return Layout.Vertical()
            | new Card(
                Layout.Vertical()
                    | Layout.Horizontal().Align(Align.Center)
                        | new Box("JD").Width(64).Height(64).Color(Colors.Blue)
                        | Layout.Vertical().Padding(16)
                            | Text.H3(name)
                            | Text.Small(email).Color(Colors.Gray)
                    | Layout.Horizontal().Align(Align.Right).Gap(8)
                        | new Button("Edit", _ => client.Toast("Edit clicked"))
                        | new Button("Delete", _ => client.Toast("Delete clicked"))
                            .Variant(ButtonVariant.Destructive)
            ).Title("User Profile");
    }
}
```

<Callout type="info">
StackLayout is the foundation for most other layout widgets. Understanding its properties will help you master more complex layout systems.
</Callout>

<WidgetDocs Type="Ivy.StackLayout" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/StackLayout.cs"/>

## Examples

### Navigation Bar

Create a horizontal navigation bar with proper alignment:

```csharp demo-tabs
public class NavigationExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        
        return new StackLayout([
            Text.H3("MyApp").Color(Colors.White),
            Layout.Horizontal().Align(Align.Right).Gap(16)
                | new Button("Home", _ => client.Toast("Home"))
                | new Button("About", _ => client.Toast("About"))
                | new Button("Contact", _ => client.Toast("Contact"))
        ], Orientation.Horizontal, padding: new Thickness(16), background: Colors.Blue);
    }
}
```
