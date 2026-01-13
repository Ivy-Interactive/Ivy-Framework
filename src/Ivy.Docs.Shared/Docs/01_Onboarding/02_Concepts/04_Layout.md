---
searchHints:
  - layout
  - horizontal
  - vertical
  - center
  - wrap
  - grid
  - tabs
  - fluent
  - composition
---

# Layout

<Ingress>
The Layout static class provides a fluent API for creating common layout compositions with minimal code, serving as the primary entry point for building [UI structures](./02_Views.md) in Ivy.
</Ingress>

The Layout class offers convenient factory methods that return pre-configured layout views. Instead of manually instantiating layout widgets, you can use Layout.Vertical(), Layout.Horizontal(), and other methods to quickly compose your UI with a clean, fluent syntax.

## Basic Usage

Create simple layouts using the static helper methods.

Vertical layout arranges elements from top to bottom:

```csharp demo-below
Layout.Vertical()
    | new Badge("Top")
    | new Badge("Middle")
    | new Badge("Bottom")
```

Horizontal layout arranges elements from left to right:

```csharp demo-below
Layout.Horizontal()
    | new Badge("Left")
    | new Badge("Center")
    | new Badge("Right")
```

## Pipe Operator Syntax

The Layout class supports the pipe operator `|` for adding children, enabling a clean and readable composition syntax:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Label("User Profile")
    | (Layout.Horizontal().Gap(2)
        | new Badge("Active").Primary()
        | new Badge("Premium").Secondary())
    | Text.P("Choose your plan").Small()
```

## Configuration Methods

All layout methods return a LayoutView that can be further configured:

### Gap

Control spacing between elements:

```csharp demo-tabs
Layout.Vertical()
    | Text.Label("No Gap:")
    | (Layout.Horizontal().Gap(0)
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("With Gap:")
    | (Layout.Horizontal().Gap(8)
        | new Badge("A") | new Badge("B") | new Badge("C"))
```

### Padding and Margin

Add internal and external spacing:

```csharp demo-tabs
Layout.Vertical().Padding(4).Background(Colors.Muted)
    | Text.Label("This layout has padding and background")
    | new Badge("Example")
```

### Width and Height

Control layout dimensions:

```csharp demo-tabs
Layout.Horizontal().Gap(4)
    | (Layout.Vertical().Width(50).Height(20).Background(Colors.Muted).Center()
        | Text.Label("50 units wide"))
    | (Layout.Vertical().Width(30).Height(20).Background(Colors.Muted).Center()
        | Text.Label("30 units"))
```

### Alignment

Align content within the layout:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | (Layout.Horizontal().Left()
        | new Badge("Left aligned"))
    | (Layout.Horizontal().Center()
        | new Badge("Center aligned"))
    | (Layout.Horizontal().Right()
        | new Badge("Right aligned"))
```

## Combining with Other Layouts

The Layout methods integrate seamlessly with specialized layout widgets:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Label("Dashboard")
    | (Layout.Grid().Columns(2).Gap(4)
        | new Card("Sales").Title("$12,450")
        | new Card("Users").Title("1,234")
        | new Card("Orders").Title("89")
        | new Card("Revenue").Title("$45,000"))
```

## Extension Methods

The LayoutExtensions class provides additional helper methods:

| Extension | Description |
|-----------|-------------|
| .WithMargin(int) | Wraps any object in a layout with margin |
| .WithMargin(int, int) | Wraps with horizontal and vertical margin |
| .WithMargin(int, int, int, int) | Wraps with left, top, right, bottom margin |
| .WithLayout() | Wraps any object in a vertical layout |

## Available Methods

The Layout class provides the following factory methods:

| Method | Description |
|--------|-------------|
| Layout.Vertical() | Creates a vertical stack layout |
| Layout.Horizontal() | Creates a horizontal stack layout |
| Layout.Center() | Creates a centered layout with removed parent padding |
| Layout.TopCenter() | Creates a top-center aligned layout |
| Layout.Wrap() | Creates a wrap layout for flowing content |
| Layout.Grid() | Creates a grid layout for two-dimensional arrangements |
| Layout.Tabs() | Creates a tabbed layout |

## Available Layouts

| Layout | Description |
|--------|-------------|
| [StackLayout](../../02_Widgets/02_Layouts/01_StackLayout.md) | Arranges elements vertically or horizontally in a linear sequence |
| [GridLayout](../../02_Widgets/02_Layouts/03_GridLayout.md) | Two-dimensional grid system with precise control over positioning and spanning |
| [WrapLayout](../../02_Widgets/02_Layouts/02_WrapLayout.md) | Automatically wraps items to new lines when space is limited |
| [TabsLayout](../../02_Widgets/02_Layouts/07_TabsLayout.md) | Organizes content into tabbed sections for easy navigation |
| [SidebarLayout](../../02_Widgets/02_Layouts/06_SidebarLayout.md) | Main content area with a collapsible sidebar for navigation |
| [HeaderLayout](../../02_Widgets/02_Layouts/04_HeaderLayout.md) | Page layout with a fixed header section |
| [FooterLayout](../../02_Widgets/02_Layouts/05_FooterLayout.md) | Page layout with a fixed footer section |
| [FloatingPanel](../../02_Widgets/02_Layouts/09_FloatingPanel.md) | Overlay panels that float above the main content |
| [ResizeablePanelGroup](../../02_Widgets/02_Layouts/08_ResizeablePanelGroup.md) | Split panels that users can resize by dragging |

