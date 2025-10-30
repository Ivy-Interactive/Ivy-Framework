---
searchHints:
  - overlay
  - modal
  - popup
  - floating
  - dialog
  - window
---

# FloatingPanel

<Ingress>
Create fixed-position UI elements that remain visible and accessible regardless of scroll position, perfect for navigation buttons, action panels, and floating controls.
</Ingress>

The `FloatingPanel` widget positions its content at a fixed location on the screen, making it ideal for elements that should remain accessible while users scroll through content. It's commonly used for navigation buttons, action panels, and floating controls that need to stay visible.

## Basic Usage

The simplest floating panel positions content in the bottom-right corner by default:

```csharp demo-tabs
public class BasicFloatingPanelView : ViewBase
{
    public override object? Build()
    {
        var showPanel = UseState(false);
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panel", onClick: _ => showPanel.Set(true))
                    | new Button("Hide Panel", onClick: _ => showPanel.Set(false))
            ).Width(Size.Full())
            | (showPanel.Value ? new FloatingPanel(
                new Button("Floating Action")
                    .Icon(Icons.Plus)
                    .Large()
                    .BorderRadius(BorderRadius.Full)
            ) : null);
    }
}
```

<Callout Type="tip">
Floating panels automatically use a high z-index (50) to ensure they appear above other content. Be mindful of layering when using multiple floating elements.
</Callout>

## Alignment Options

The `FloatingPanel` supports nine different alignment positions to place content exactly where you need it:

### Corner Positions

Position content in any of the four corners of the screen:

```csharp demo-tabs
public class CornerAlignmentView : ViewBase
{
    public override object? Build()
    {
        var showPanels = UseState(false);
        var floatingButton = new Button("Action")
            .Icon(Icons.Star)
            .Large()
            .BorderRadius(BorderRadius.Full);

        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panels", onClick: _ => showPanels.Set(true))
                    | new Button("Hide Panels", onClick: _ => showPanels.Set(false))
            ).Width(Size.Full())
            | (showPanels.Value ? new Fragment()
                | new FloatingPanel(floatingButton, Align.TopLeft)
                | new FloatingPanel(floatingButton, Align.TopRight)
                | new FloatingPanel(floatingButton, Align.BottomLeft)
                | new FloatingPanel(floatingButton, Align.BottomRight)
            : null);
    }
}
```

### Edge Center Positions

Center content along the edges of the screen:

```csharp demo-tabs
public class EdgeCenterAlignmentView : ViewBase
{
    public override object? Build()
    {
        var showPanels = UseState(false);
        var floatingButton = new Button("Center")
            .Icon(Icons.Move)
            .Large()
            .BorderRadius(BorderRadius.Full);

        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panels", onClick: _ => showPanels.Set(true))
                    | new Button("Hide Panels", onClick: _ => showPanels.Set(false))
            ).Width(Size.Full())
            | (showPanels.Value ? new Fragment()
                | new FloatingPanel(floatingButton, Align.TopCenter)
                | new FloatingPanel(floatingButton, Align.BottomCenter)
                | new FloatingPanel(floatingButton, Align.Left)
                | new FloatingPanel(floatingButton, Align.Right)
            : null);
    }
}
```

### Screen Center

Position content in the exact center of the screen:

```csharp demo-tabs
public class CenterAlignmentView : ViewBase
{
    public override object? Build()
    {
        var showPanel = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panel", onClick: _ => showPanel.Set(true))
                    | new Button("Hide Panel", onClick: _ => showPanel.Set(false))
            ).Width(Size.Full())
            | (showPanel.Value ? new FloatingPanel(
                new Card(
                    Layout.Vertical()
                        | Text.H3("Centered Panel")
                        | Text.Block("This panel is positioned")
                        | Text.Block("in the center of the screen")
                        | new Button("Close", onClick: _ => showPanel.Set(false)).Secondary()
                ).Width(Size.Auto())
            , Align.Center) : null);
    }
}
```

## Offset Positioning

Fine-tune the position of floating panels using offset values. The `Offset` method accepts a `Thickness` object to specify precise positioning:

### Basic Offset

Adjust the position from the default alignment:

```csharp demo-tabs
public class BasicOffsetView : ViewBase
{
    public override object? Build()
    {
        var showPanels = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panels", onClick: _ => showPanels.Set(true))
                    | new Button("Hide Panels", onClick: _ => showPanels.Set(false))
            ).Width(Size.Full())
            | (showPanels.Value ? new Fragment()
                | new FloatingPanel(
                    new Button("Default Position")
                        .Icon(Icons.Circle)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.TopRight)
                | new FloatingPanel(
                    new Button("Offset Down & Left")
                        .Icon(Icons.ArrowDownLeft)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.BottomLeft)
                    .Offset(new Thickness(0, 20, 0, 0))  // 20 units up from bottom edge
                | new FloatingPanel(
                    new Button("Custom Offset")
                        .Icon(Icons.Move)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.BottomRight)
                    .Offset(new Thickness(10, 0, 0, 10)) // Thickness(left, top, right, bottom): 10 from left edge, 10 from bottom edge
            : null);
    }
}
```

### Convenience Offset Methods

Use the convenience methods for quick positioning adjustments:

```csharp demo-tabs
public class ConvenienceOffsetView : ViewBase
{
    public override object? Build()
    {
        var showPanels = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panels", onClick: _ => showPanels.Set(true))
                    | new Button("Hide Panels", onClick: _ => showPanels.Set(false))
            ).Width(Size.Full())
            | (showPanels.Value ? new Fragment()
                | new FloatingPanel(
                    new Button("Top Offset")
                        .Icon(Icons.ArrowUp)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.TopRight)
                    .OffsetTop(30) 
                | new FloatingPanel(
                    new Button("Left Offset")
                        .Icon(Icons.ArrowLeft)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.TopRight)
                    .OffsetLeft(30) 
                | new FloatingPanel(
                    new Button("Right Offset")
                        .Icon(Icons.ArrowRight)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.TopLeft)
                    .OffsetRight(30) 
                | new FloatingPanel(
                    new Button("Bottom Offset")
                        .Icon(Icons.ArrowDown)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                , Align.BottomLeft)
                    .OffsetBottom(30) 
            : null);
    }
}
```

## Complex Content

Floating panels can contain complex layouts and multiple widgets:

### Navigation Panel

Create a floating navigation panel with multiple buttons:

```csharp demo-tabs
public class NavigationPanelView : ViewBase
{
    public override object? Build()
    {
        var showPanel = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panel", onClick: _ => showPanel.Set(true))
                    | new Button("Hide Panel", onClick: _ => showPanel.Set(false))
            ).Width(Size.Full())
            | (showPanel.Value ? new FloatingPanel(
                Layout.Vertical().Gap(2)
                    | new Button("Home")
                        .Icon(Icons.House)
                        .Secondary()
                        .Width(Size.Units(12))
                    | new Button("Settings")
                        .Icon(Icons.Settings)
                        .Secondary()
                        .Width(Size.Units(12))
                    | new Button("Profile")
                        .Icon(Icons.User)
                        .Secondary()
                        .Width(Size.Units(12))
                    | new Button("Help")
                        .Icon(Icons.Info)
                        .Secondary()
                        .Width(Size.Units(12))
            , Align.Right)
                .Offset(new Thickness(0, 0, 10, 0)) : null);
    }
}
```

### Action Panel

A floating action panel with multiple action buttons:

```csharp demo-tabs
public class ActionPanelView : ViewBase
{
    public override object? Build()
    {
        var showPanel = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panel", onClick: _ => showPanel.Set(true))
                    | new Button("Hide Panel", onClick: _ => showPanel.Set(false))
            ).Width(Size.Full())
            | (showPanel.Value ? new FloatingPanel(
                Layout.Horizontal().Gap(2)
                    | new Button("New")
                        .Icon(Icons.Plus)
                        .Primary()
                        .BorderRadius(BorderRadius.Full)
                    | new Button("Edit")
                        .Icon(Icons.Pen)
                        .Secondary()
                        .BorderRadius(BorderRadius.Full)
                    | new Button("Delete")
                        .Icon(Icons.Trash)
                        .Destructive()
                        .BorderRadius(BorderRadius.Full)
            , Align.BottomCenter)
                .Offset(new Thickness(0, 0, 0, 20)) : null);
    }
}
```

<Callout Type="tip">
Ensure floating panels don't interfere with content readability and provide clear visual hierarchy. Use appropriate contrast and sizing for interactive elements.
</Callout>

<WidgetDocs Type="Ivy.FloatingPanel" ExtensionTypes="Ivy.FloatingLayerExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/FloatingPanel.cs"/>

## Examples

<Details>
<Summary>
Back to Top Button
</Summary>
<Body>
A common use case for floating panels - a "back to top" button:

```csharp demo-tabs
public class BackToTopView : ViewBase
{
    public override object? Build()
    {
        var showButton = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Button", onClick: _ => showButton.Set(true))
                    | new Button("Hide Button", onClick: _ => showButton.Set(false))
            ).Width(Size.Full())
            | (showButton.Value ? new FloatingPanel(
                new Button("Top")
                    .Icon(Icons.ArrowUp)
                    .Large()
                    .BorderRadius(BorderRadius.Full)
                    .Secondary()
            , Align.BottomRight)
                .Offset(new Thickness(0, 0, 20, 20)) : null);
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Floating Search Bar
</Summary>
<Body>
A floating search bar that stays accessible:

```csharp demo-tabs
public class FloatingSearchView : ViewBase
{
    public override object? Build()
    {
        var showSearchBar = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Search Bar", onClick: _ => showSearchBar.Set(true))
                    | new Button("Hide Search Bar", onClick: _ => showSearchBar.Set(false))
            ).Width(Size.Full())
            | (showSearchBar.Value ? new FloatingPanel(
                new Card(
                    Layout.Horizontal().Gap(2)
                        | new TextInput(placeholder: "Search...")
                        | new Button("Search")
                            .Icon(Icons.Search)
                            .Primary()
                )
            , Align.TopCenter)
                .Offset(new Thickness(0, 10, 0, 0)) : null);
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Multi-Panel Layout
</Summary>
<Body>
Demonstrate multiple floating panels working together:

```csharp demo-tabs
public class MultiPanelView : ViewBase
{
    public override object? Build()
    {
        var showPanels = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | new Card(
                Layout.Horizontal().Gap(2).Align(Align.Center)
                    | new Button("Show Panels", onClick: _ => showPanels.Set(true))
                    | new Button("Hide Panels", onClick: _ => showPanels.Set(false))
            ).Width(Size.Full())
            | (showPanels.Value ? new Fragment()
                | new FloatingPanel(
                    new Button("Menu")
                        .Icon(Icons.Menu)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                        .Secondary()
                , Align.TopLeft)
                    .Offset(new Thickness(10, 10, 0, 0))
                | new FloatingPanel(
                    new Button("Notifications")
                        .Icon(Icons.Bell)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                        .Secondary()
                , Align.TopRight)
                    .Offset(new Thickness(0, 10, 10, 0))
                | new FloatingPanel(
                    new Button("Chat")
                        .Icon(Icons.MessageCircle)
                        .Large()
                        .BorderRadius(BorderRadius.Full)
                        .Primary()
                , Align.BottomRight)
                    .Offset(new Thickness(0, 0, 20, 20))
                | new FloatingPanel(
                    new Card(
                        Layout.Vertical()
                            | Text.Block("Quick Actions")
                            | new Button("Save").Small().Primary()
                            | new Button("Share").Small().Secondary()
                    ).Width(Size.Units(40))
                , Align.Left)
                    .Offset(new Thickness(10, 0, 0, 0))
            : null);
    }
}
```

</Body>
</Details>
