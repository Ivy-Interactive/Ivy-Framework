---
searchHints:
  - stacked
  - segmented
  - multi
  - progress
  - bar
  - composition
  - breakdown
---

# StackedProgress

<Ingress>
Display multiple colored segments in a single progress bar to show composition breakdowns like task status distribution, disk usage by category, or pipeline stages.
</Ingress>

The `StackedProgress` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) renders a horizontal bar with multiple colored segments, each proportional to its value. Hover over a segment to see its label and value in a tooltip.

## Example

```csharp demo-tabs
public class StackedProgressDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            new StackedProgress(
                new ProgressSegment(40, Colors.Green, "Completed"),
                new ProgressSegment(25, Colors.Blue, "In Progress"),
                new ProgressSegment(20, Colors.Orange, "Pending"),
                new ProgressSegment(15, Colors.Red, "Failed")
            ).ShowLabels()
        );
    }
}
```

## Color Variants

Use any [color](../../01_Onboarding/02_Concepts/12_Theming.md) from the `Colors` enum for each segment.

```csharp demo-tabs
public class StackedProgressColorsDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            new StackedProgress(
                new ProgressSegment(25, Colors.Emerald),
                new ProgressSegment(35, Colors.Sky),
                new ProgressSegment(20, Colors.Violet),
                new ProgressSegment(20, Colors.Amber)
            ),
            new StackedProgress(
                new ProgressSegment(50, Colors.Rose),
                new ProgressSegment(30, Colors.Indigo),
                new ProgressSegment(20, Colors.Teal)
            )
        );
    }
}
```

## Labels

Enable labels below the bar with `.ShowLabels()`. Each segment with a `Label` will be shown in the legend.

```csharp demo-tabs
public class StackedProgressLabelsDemo : ViewBase
{
    public override object? Build()
    {
        return new StackedProgress(
            new ProgressSegment(60, Colors.Green, "Passed"),
            new ProgressSegment(10, Colors.Orange, "Skipped"),
            new ProgressSegment(30, Colors.Red, "Failed")
        ).ShowLabels();
    }
}
```

<WidgetDocs Type="Ivy.StackedProgress" ExtensionTypes="Ivy.StackedProgressExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/StackedProgress.cs"/>
