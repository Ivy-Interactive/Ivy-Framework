# Embed

The `Embed` widget allows you to incorporate external content such as videos, maps, or other web resources into your app. It creates a responsive container for embedded content.

```csharp demo-tabs 
public class ResourcesView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H3("Youtube")
            | new Embed("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
    }
}
```

<WidgetDocs Type="Ivy.Embed" ExtensionTypes="Ivy.EmbedExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Embed.cs"/>
