# Box

<Ingress>
Create versatile container elements with customizable borders, colors, and padding for grouping content and structuring layouts.
</Ingress>

`Box`es are versatile container elements with customizable borders, colors, and padding. They're useful for visually grouping related content or creating distinct sections in your UI.

```csharp demo-tabs 
public class StatusDashboardView : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3).Gap(4)
            | new Box("System Online")
                .Padding(3)
            | new Box("Warning: High CPU Usage")
                .Color(Colors.Yellow)
                .BorderStyle(BorderStyle.Dashed)
                .BorderThickness(2)
                .Padding(3)
            | new Box("Database Error")
                .Color(Colors.Red)
                .BorderThickness(2)
                .Padding(3);
    }
}
```

<WidgetDocs Type="Ivy.Box" ExtensionTypes="Ivy.BoxExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Box.cs"/>
