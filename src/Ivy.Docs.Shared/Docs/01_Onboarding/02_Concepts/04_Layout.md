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
  - row
  - column
---

# Layout

<Ingress>
The Layout static class provides a fluent API for creating common layout compositions with minimal code, serving as the primary entry point for building [UI structures](./02_Views.md) in Ivy.
</Ingress>

The Layout class offers convenient factory methods that return pre-configured layout [views](./02_Views.md). Instead of manually instantiating layout [widgets](./03_Widgets.md), you can use Layout.Vertical(), Layout.Horizontal(), and other methods to quickly compose your UI with a clean, fluent syntax.

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

Control spacing between elements with `.Gap()`:

```csharp demo-tabs
Layout.Vertical()
    | Text.Label("No Gap:")
    | (Layout.Horizontal().Gap(0)
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("With Gap:")
    | (Layout.Horizontal().Gap(8)
        | new Badge("A") | new Badge("B") | new Badge("C"))
```

### Independent Row & Column Gap

Use `.Gap(rowGap, columnGap)` to control vertical and horizontal spacing independently:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Label("RowGap=8, ColumnGap=2:")
    | (Layout.Wrap().Gap(8, 2).Width(Size.Units(60))
        | new Badge("A") | new Badge("B") | new Badge("C")
        | new Badge("D") | new Badge("E") | new Badge("F")
        | new Badge("G") | new Badge("H") | new Badge("I")
        | new Badge("J") | new Badge("K") | new Badge("L")
        | new Badge("M") | new Badge("N"))
    | Text.Label("RowGap=2, ColumnGap=8:")
    | (Layout.Wrap().Gap(2, 8).Width(Size.Units(60))
        | new Badge("A") | new Badge("B") | new Badge("C")
        | new Badge("D") | new Badge("E") | new Badge("F")
        | new Badge("G") | new Badge("H"))
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
    | (Layout.Vertical().Width(Size.Units(50)).Height(Size.Units(20)).Background(Colors.Muted).Center()
        | Text.Label("50 units wide"))
    | (Layout.Vertical().Width(Size.Units(30)).Height(Size.Units(20)).Background(Colors.Muted).Center()
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

### Space Distribution

Distribute space between elements using `SpaceBetween`, `SpaceAround`, or `SpaceEvenly`:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Label("SpaceBetween — items pushed to edges:")
    | (Layout.Horizontal().AlignContent(Align.SpaceBetween).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("SpaceAround — equal space around each item:")
    | (Layout.Horizontal().AlignContent(Align.SpaceAround).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("SpaceEvenly — equal space between all items:")
    | (Layout.Horizontal().AlignContent(Align.SpaceEvenly).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
```

### Wrap

Use `Layout.Wrap()` to create a layout where items flow and wrap to the next line when they run out of space. Try resizing the window to see the wrapping behavior:

```csharp demo-tabs
Layout.Wrap().Gap(2)
    | new Badge("Tag 1").Primary()
    | new Badge("Tag 2").Secondary()
    | new Badge("Tag 3")
    | new Badge("Tag 4").Primary()
    | new Badge("Tag 5").Secondary()
    | new Badge("Tag 6")
    | new Badge("Tag 7").Primary()
    | new Badge("Tag 8").Secondary()
    | new Badge("Tag 9")
    | new Badge("Tag 10").Primary()
    | new Badge("Tag 11").Secondary()
```

### AlignSelf

Override alignment for individual children using `.AlignSelf()`. In a horizontal layout, this controls vertical positioning of each child independently:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | new Badge("Top").Primary().AlignSelf(Align.TopLeft)
    | new Badge("Center").Primary().AlignSelf(Align.Center)
    | new Badge("Bottom").Primary().AlignSelf(Align.BottomRight)
```


### Scroll

Add scrollable behavior to layouts with constrained height using `.Scroll()`:

```csharp demo-tabs
Layout.Vertical().Height(Size.Units(30)).Scroll(Scroll.Vertical).Gap(2)
    | new Badge("Item 1") | new Badge("Item 2") | new Badge("Item 3")
    | new Badge("Item 4") | new Badge("Item 5") | new Badge("Item 6")
    | new Badge("Item 7") | new Badge("Item 8") | new Badge("Item 9")
    | new Badge("Item 10") | new Badge("Item 11") | new Badge("Item 12")
```

## Combining with Other Layouts

The Layout methods integrate seamlessly with specialized layout [widgets](../../02_Widgets/02_Layouts/_Index.md) and [Card](../../02_Widgets/03_Common/04_Card.md):

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
| Layout.Wrap() | Creates a wrapping stack layout for flowing content |
| Layout.Grid() | Creates a grid layout for two-dimensional arrangements |
| Layout.Tabs() | Creates a tabbed layout |

## Available Layouts

| Layout | Description |
|--------|-------------|
| [StackLayout](../../02_Widgets/02_Layouts/01_StackLayout.md) | Arranges elements vertically or horizontally in a linear sequence (supports wrapping) |
| [GridLayout](../../02_Widgets/02_Layouts/03_GridLayout.md) | Two-dimensional grid system with precise control over positioning and spanning |
| [TabsLayout](../../02_Widgets/02_Layouts/07_TabsLayout.md) | Organizes content into tabbed sections for easy navigation |
| [SidebarLayout](../../02_Widgets/02_Layouts/06_SidebarLayout.md) | Main content area with a collapsible sidebar for navigation |
| [HeaderLayout](../../02_Widgets/02_Layouts/04_HeaderLayout.md) | Page layout with a fixed header section |
| [FooterLayout](../../02_Widgets/02_Layouts/05_FooterLayout.md) | Page layout with a fixed footer section |
| [FloatingPanel](../../02_Widgets/02_Layouts/09_FloatingPanel.md) | Overlay panels that float above the main content |
| [ResizablePanelGroup](../../02_Widgets/02_Layouts/08_ResizablePanelGroup.md) | Split panels that users can resize by dragging |

## Faq

<Details>
<Summary>
Does Ivy have Row and Column widgets
</Summary>
<Body>

No. Ivy uses `Layout.Horizontal()` for horizontal layouts (similar to Row) and `Layout.Vertical()` for vertical layouts (similar to Column). Usage of traditional `new StackLayout([...])` constructor is not recommended and should be avoided in favor of Fluent API.

```csharp
// Horizontal layout (like "Row")
Layout.Horizontal([widget1, widget2, widget3]);

// Vertical layout (like "Column")
Layout.Vertical([widget1, widget2, widget3]);
```

</Body>
</Details>

