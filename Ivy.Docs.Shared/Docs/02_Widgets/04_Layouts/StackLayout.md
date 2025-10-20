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
StackLayout arranges child elements in either a vertical or horizontal stack with configurable spacing, alignment, and styling options. It's the foundation for creating linear layouts where elements are arranged sequentially in a single direction.
</Ingress>

The `StackLayout` widget is the core building block for most layout compositions, offering flexible configuration for orientation, gaps between elements, padding, margins, background colors, and content alignment. It can be used to create simple stacks or as the foundation for more complex layout systems.

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
        var box1 = new Box().Width(2).Height(2);
        var box2 = new Box().Width(2).Height(2);
        var box3 = new Box().Width(2).Height(2);
        
        return new StackLayout([
            Text.H2("StackLayout Features"),
            Text.Label("Orientation.Horizontal, gap(2), padding(1)"),
            new StackLayout([box1, box2, box3], Orientation.Horizontal, gap: 2, padding: new Thickness(1)),
            Text.Label("Orientation.Vertical, gap(1), Align.Center, padding(2)"),
            new StackLayout([box1, box2, box3], Orientation.Vertical, gap: 1, align: Align.Center, padding: new Thickness(2))
        ], gap: 4);
    }
}
```

## Advanced Features

Complete example showing padding, margins, background colors, and parent padding control:

```csharp demo-tabs
public class AdvancedStackLayoutExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Width(2).Height(2);
        
        return new StackLayout([
            Text.H2("Advanced StackLayout Features"),
            Text.Label("With Margin (external spacing)"),
            new StackLayout([box, box], Orientation.Horizontal, margin: new Thickness(4)),
            Text.Label("Remove Parent Padding, Background color"),
            new StackLayout([box, box], Orientation.Horizontal, removeParentPadding: true, background: Colors.Gray)
        ], gap: 2, padding: new Thickness(8));
    }
}
```

<Callout type="info">
StackLayout is the foundation for most other layout widgets. Understanding its properties will help you master more complex layout systems.
</Callout>

<WidgetDocs Type="Ivy.StackLayout" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/StackLayout.cs"/>

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
        
        return new StackLayout([
            // Navigation buttons
            new StackLayout([
                new Button("Home", _ => client.Toast("Home")),
                new Button("About", _ => client.Toast("About")),
                new Button("Contact", _ => client.Toast("Contact")),
                new Button("Settings", _ => client.Toast("Settings"))
            ], Orientation.Horizontal, gap: 8, align: Align.Center),

             // App title and user info
            new StackLayout([
                Text.H3("MyApp"),
                Text.Small("Welcome back!")
            ], Orientation.Vertical, align: Align.Left),
            
            // User actions
            new StackLayout([
                new Button("Profile", _ => client.Toast("Profile")),
                new Button("Logout", _ => client.Toast("Logout"))
            ], Orientation.Horizontal, gap: 4, align: Align.Right)
            
        ], Orientation.Vertical, gap: 16, padding: new Thickness(12), align: Align.Center);
    }
}
```

</Body>
</Details>

