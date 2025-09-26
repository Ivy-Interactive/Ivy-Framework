# Svg

<Ingress>
Create beautiful, scalable vector graphics directly in your app with the `Svg` widget. Perfect for icons, illustrations, charts, and other graphics that need to scale without losing quality.
</Ingress>

The `Svg` widget renders scalable vector graphics directly in your app. SVGs are resolution-independent and perfect for icons, illustrations, charts, and other graphics that need to scale without losing quality.

## Basic Usage

The simplest way to create an SVG is to pass SVG markup as a string:

```csharp demo-below
public class SimpleSvgView : ViewBase
{
    public override object? Build()
    {
        var simpleCircle = """
            <svg width="100" height="100">
                <circle cx="50" cy="50" r="40" fill="green" />
            </svg>
            """;
            
        return new Svg(simpleCircle);
    }
}
```

<Callout Type="Tip">
The `Svg` widget has the following properties:
- Content (string): The SVG markup content to render
- Width (Size): The width of the SVG container (defaults to Auto)
- Height (Size): The height of the SVG container (defaults to Auto)
</Callout>

### Animated Svg

This example demonstrates how to create an animated SVG progress bar using the `<animate>` element. The animation continuously cycles the width of the red rectangle between 80 and 160 pixels over 3 seconds, creating a smooth loading effect.

```csharp demo-tabs
public class SimpleSvgView : ViewBase
{
    public override object? Build()
    {
        var animatedBar = """
                    <svg width="200" height="20" viewBox="0 0 200 20">
                        <rect width="200" height="20" fill="#e5e7eb" rx="10"/>
                        <rect width="80" height="20" fill="#ef4444" rx="10">
                            <animate attributeName="width" values="80;160;80" dur="3s" repeatCount="indefinite"/>
                        </rect>
                    </svg>
                    """;
        return new Svg(animatedBar);
    }
}
```

<WidgetDocs Type="Ivy.Svg" ExtensionTypes="Ivy.SvgExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Svg.cs"/>
