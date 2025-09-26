# ResizeablePanelGroup

<Ingress>
Create flexible, resizable layouts with draggable handles that allow users to dynamically adjust panel sizes in your applications.
</Ingress>

The `ResizeablePanelGroup` widget creates layouts with multiple panels separated by draggable handles, allowing users to resize sections interactively. Panels can be arranged horizontally or vertically and support nesting for complex layouts.

## Basic Usage

The simplest resizable panel group consists of two or more panels arranged horizontally:

```csharp demo-below
public class BasicResizeablePanelView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(30, 
                new Card("Left Panel")),
            new ResizeablePanel(70, 
                new Card("Right Panel"))
        );
    }
}
```

## Orientation

### Horizontal Layout (Default)

Panels are arranged side by side with vertical drag handles:

```csharp demo-tabs
public class HorizontalResizeableView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(25, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Sidebar")
                        | Text.Block("Navigation")
                        | Text.Block("• Home")
                        | Text.Block("• Settings")
                )),
            new ResizeablePanel(50, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Main Content")
                        | Text.Block("This is the primary content area")
                        | Text.Block("where the main application content")
                        | Text.Block("would be displayed.")
                )),
            new ResizeablePanel(25, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Info Panel")
                        | Text.Block("Additional info")
                        | Text.Block("• Stats")
                        | Text.Block("• Notifications")
                ))
        ).Horizontal();
    }
}
```

### Vertical Layout

Panels are stacked vertically with horizontal drag handles:

```csharp demo-tabs
public class VerticalResizeableView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(30, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Header Section")
                        | Text.Block("Navigation and branding")
                )),
            new ResizeablePanel(40, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Main Content")
                        | Text.Block("This is the main content area where")
                        | Text.Block("your primary content would be displayed.")
                        | Text.Block("It takes up the majority of the space.")
                )),
            new ResizeablePanel(30, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Footer Section")
                        | Text.Block("Copyright and links")
                ))
        ).Vertical().Height(Size.Units(150));
    }
}
```

## Panel Sizing

### Default Sizes

Each panel can have a default size specified as a percentage of the total space:

```csharp demo-tabs
public class DefaultSizesView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(20, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("20% Panel")
                        | Text.Block("Small panel")
                )),
            new ResizeablePanel(60, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("60% Panel")
                        | Text.Block("Large main panel")
                        | Text.Block("with more content space")
                )),
            new ResizeablePanel(20, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("20% Panel")
                        | Text.Block("Small panel")
                ))
        );
    }
}
```

### Auto-Sizing

Panels without specified sizes will automatically distribute the remaining space:

```csharp demo-tabs
public class AutoSizingView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(25, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Fixed 25%")
                        | Text.Block("This panel has")
                        | Text.Block("a fixed size")
                )),
            new ResizeablePanel(null, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Auto Size")
                        | Text.Block("This panel automatically")
                        | Text.Block("sizes to available space")
                )),
            new ResizeablePanel(null, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Auto Size")
                        | Text.Block("This panel also")
                        | Text.Block("sizes automatically")
                ))
        );
    }
}
```

## Handle Visibility

### Show/Hide Resize Handles

Create a workspace with multiple resizable sections for different content areas:

```csharp demo-tabs
public class HandleVisibilityView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Box(Text.Block("With Handles (Default)")).Padding(2)
            | new ResizeablePanelGroup(
                new ResizeablePanel(50, 
                    new Card(
                        Layout.Vertical()
                            | Text.Label("Panel A")
                            | Text.Block("Resizable panel")
                    )),
                new ResizeablePanel(50, 
                    new Card(
                        Layout.Vertical()
                            | Text.Label("Panel B")
                            | Text.Block("Resizable panel")
                    ))
            ).ShowHandle(true).Height(Size.Units(50))
            | new Box(Text.Block("Without Handles")).Padding(2)
            | new ResizeablePanelGroup(
                new ResizeablePanel(50, 
                    new Card(
                        Layout.Vertical()
                            | Text.Label("Panel A")
                            | Text.Block("Fixed panel")
                    )),
                new ResizeablePanel(50, 
                    new Card(
                        Layout.Vertical()
                            | Text.Label("Panel B")
                            | Text.Block("Fixed panel")
                    ))
            ).ShowHandle(false).Height(Size.Units(50));
    }
}
```

## Nested Layouts

Create complex layouts by nesting ResizeablePanelGroups:

```csharp demo-tabs
public class NestedLayoutView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(25, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("Sidebar")
                        | Text.Block("Navigation menu")
                        | Text.Block("• Dashboard")
                        | Text.Block("• Reports")
                        | Text.Block("• Settings")
                )),
            new ResizeablePanel(75,
                new ResizeablePanelGroup(
                    new ResizeablePanel(60, 
                        new Card(
                            Layout.Vertical()
                                | Text.Label("Main Content")
                                | Text.Block("Primary workspace area")
                                | Text.Block("This is where the main")
                                | Text.Block("application content is displayed.")
                        )),
                    new ResizeablePanel(40,
                        new ResizeablePanelGroup(
                            new ResizeablePanel(50, 
                                new Card(
                                    Layout.Vertical()
                                        | Text.Label("Top Right")
                                        | Text.Block("Quick stats")
                                        | Text.Block("or tools")
                                )),
                            new ResizeablePanel(50, 
                                new Card(
                                    Layout.Vertical()
                                        | Text.Label("Bottom Right")
                                        | Text.Block("Additional info")
                                        | Text.Block("or controls")
                                ))
                        ).Vertical())
                ).Horizontal())
        ).Horizontal();
    }
}
```

<WidgetDocs Type="Ivy.ResizeablePanelGroup" ExtensionTypes="Ivy.ResizeablePanelsExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/ResizeablePanelGroup.cs"/>

## Examples

<Details>
<Summary>
Multi-Directional Resizing
</Summary>
<Body>
Demonstrate both horizontal and vertical resizing in a complex nested layout:

```csharp demo-tabs
public class MultiDirectionalResizingView : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            // Left panel - File browser
            new ResizeablePanel(25, 
                new Card(
                    Layout.Vertical()
                        | Text.Label("File Browser")
                        | Text.Block("App.cs")
                        | Text.Block("Layout.cs")
                        | Text.Block("Components.cs")
                        | Text.Block("Utils.cs")
                        | Text.Block("Assets/")
                        | Text.Block("Tests/")
                ).Title("Files")
            ),
            // Center area - Split editor and console
            new ResizeablePanel(50,
                new ResizeablePanelGroup(
                    // Top - Code editor
                    new ResizeablePanel(70, 
                        new Card(
                            Layout.Vertical()
                                | Text.Label("Code Editor")
                                | Text.Code("public class ResizeableExample\n{\n    public void DemoResize()\n    {\n        // Drag handles to resize!\n        Console.WriteLine(\"Resizing works!\");\n    }\n}", Languages.Csharp)
                        ).Title("main.cs")
                    ),
                    // Bottom - Console/Output
                    new ResizeablePanel(70,
                        new Card(
                            Layout.Vertical()
                                | Text.Label("Console Output")
                                | Text.Block("> dotnet run")
                                | Text.Block("Building...")
                                | Text.Block("Build succeeded")
                                | Text.Block("Application started")
                                | Text.Block("Resizing works!")
                        ).Title("Output")
                    )
                ).Vertical().Height(Size.Units(190))),
            // Right panel - Properties and tools
            new ResizeablePanel(25,
                new Card(
                    Layout.Vertical()
                        | Text.Label("Properties")
                        | Text.Block("Selected: ResizeablePanel")
                        | Text.Block("Default Size: 25%")
                        | Text.Block("Direction: Horizontal")
                        | Text.Block("Show Handle: true")
                        | Text.Block("")
                        | Text.Label("Actions")
                        | Text.Block("• Resize horizontally ↔")
                        | Text.Block("• Resize vertically ↕")
                        | Text.Block("• Nested resizing ⤡")
                ).Title("Inspector")
            )
        ).Height(Size.Units(200));
    }
}
```

</Body>
</Details>
