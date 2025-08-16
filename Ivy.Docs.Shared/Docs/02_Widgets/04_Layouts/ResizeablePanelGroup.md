# ResizeablePanelGroup

`ResizeablePanelGroup` arranges panels with draggable handles for resizable
layouts. Panels can be placed horizontally or vertically.

```csharp
new ResizeablePanelGroup(
    new ResizeablePanel(25, "Left"),
    new ResizeablePanel(75,
        new ResizeablePanelGroup(
            new ResizeablePanel(50, "Top"),
            new ResizeablePanel(50, "Bottom")
        ).Vertical())
).Horizontal().Height(Size.Screen());
```
