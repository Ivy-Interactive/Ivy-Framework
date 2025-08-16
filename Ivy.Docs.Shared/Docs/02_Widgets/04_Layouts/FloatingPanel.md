# FloatingPanel

`FloatingPanel` places its child in a fixed position on the screen. It is useful
for buttons that should remain visible, such as a "Back" or "Create" button.

```csharp
new FloatingPanel(Align.TopRight)
    | (Layout.Horizontal()
        | new Button("Back").Icon(Icons.ArrowLeft)
        | new Button("Create Something Amazing"));
```
