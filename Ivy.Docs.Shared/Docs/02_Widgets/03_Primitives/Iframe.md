# Iframe

<Ingress>
Embed external web pages securely within your application using contained browsing contexts with proper security boundaries.
</Ingress>

The `Iframe` widget embeds external web pages into your app. It creates a contained browsing context that can display content from other websites while maintaining security boundaries.

```csharp demo-tabs 
public class ToolsDashboardView : ViewBase
{
    public override object? Build()
    {
        var selectedTool = UseState("dashboard");
        
        object GetSelectedTool() => selectedTool.Value switch
        {
            "dashboard" => new Iframe("https://grafana.com/grafana/dashboards/1860-node-exporter-full/")
                .Width(Size.Full())
                .Height(Size.Units(600)),
            
            "weather" => new Iframe("https://weather.com")
                .Width(Size.Full())
                .Height(Size.Units(600)),
            
            "calendar" => new Iframe("https://calendar.google.com/calendar/embed")
                .Width(Size.Full())
                .Height(Size.Units(600)),
                
            _ => Text.H3("Please select a tool")
        };
        
        return Layout.Vertical().Gap(4)
            | Text.H1("External Tools Dashboard")
            | Layout.Horizontal().Gap(2)
                | new Button("Metrics Dashboard", onClick: _ => selectedTool.Set("dashboard"))
                | new Button("Weather", onClick: _ => selectedTool.Set("weather"))
                | new Button("Calendar", onClick: _ => selectedTool.Set("calendar"))
            | GetSelectedTool();
    }
}
```

<WidgetDocs Type="Ivy.Iframe" ExtensionTypes="Ivy.IframeExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Iframe.cs"/> 