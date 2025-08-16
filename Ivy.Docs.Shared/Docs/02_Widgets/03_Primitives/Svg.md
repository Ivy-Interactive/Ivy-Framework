# Svg

The `Svg` widget renders scalable vector graphics directly in your app. SVGs are resolution-independent and perfect for icons, illustrations, charts, and other graphics that need to scale without losing quality.

```csharp demo-tabs 
public class SvgDashboardView : ViewBase
{
    public override object? Build()
    {
        string GetCircleProgress(int percentage)
        {
            var radius = 40;
            var circumference = 2 * Math.PI * radius;
            var dashOffset = circumference * (1 - percentage / 100.0);
            
            return $"""
                <svg width="100" height="100" viewBox="0 0 100 100">
                  <circle 
                    cx="50" 
                    cy="50" 
                    r="{radius}" 
                    fill="none" 
                    stroke="#e5e7eb" 
                    stroke-width="8" 
                  />
                  <circle 
                    cx="50" 
                    cy="50" 
                    r="{radius}" 
                    fill="none" 
                    stroke="#3b82f6" 
                    stroke-width="8" 
                    stroke-dasharray="{circumference}" 
                    stroke-dashoffset="{dashOffset}" 
                    transform="rotate(-90 50 50)" 
                    stroke-linecap="round" 
                  />
                  <text 
                    x="50" 
                    y="60" 
                    text-anchor="middle" 
                    font-size="20" 
                    font-weight="bold" 
                    fill="#3b82f6"
                  >
                    {percentage}
                  </text>
                </svg>
                """;
        }
         
        return new Svg(GetCircleProgress(25));
    }
}
```

<WidgetDocs Type="Ivy.Svg" ExtensionTypes="Ivy.SvgExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Svg.cs"/>
