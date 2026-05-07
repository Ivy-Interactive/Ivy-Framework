# Progress Bar

A horizontal progress bar that visually represents the completion status of a task or process.

## Retool

```toolscript
progressBar1.setValue(75);
progressBar1.setHidden(false);
```

## Ivy

```csharp
public class ProgressDemo : ViewBase
{
    public override object? Build()
    {
        var progress = UseState(25);

        return Layout.Vertical(
            new Progress(progress.Value).Goal($"{progress.Value}% Complete"),
            Layout.Horizontal(
                new Button("0%", _ => progress.Set(0)),
                new Button("25%", _ => progress.Set(25)),
                new Button("50%", _ => progress.Set(50)),
                new Button("75%", _ => progress.Set(75)),
                new Button("100%", _ => progress.Set(100))
            )
        );
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Value | `value` (number) - The progress value | `Value` (int?) - Progress percentage via constructor or setter |
| Label/Goal | `hideOutput` (boolean) - Hides the caption | `Goal` (string) - Display label for progress status |
| Color/Style | `style` (object) - Custom styling options | `ColorVariant` (ColorVariants) - Theming via color variants |
| Visibility | `isHiddenOnDesktop` / `isHiddenOnMobile` (boolean) | `Visible` (bool) |
| Tooltip | `tooltipText` (string, Markdown) | Not supported |
| Indeterminate | `indeterminate` (boolean) - Loading state when progress unknown | Not supported |
| Label Position | `labelPosition` (string) - `top` or `left` | Not supported |
| Margin | `margin` (string) - External spacing | Not supported |
| Height | Not supported | `Height` (Size) |
| Width | Not supported | `Width` (Size) |
| Scale | Not supported | `Scale` (Scale?) |
| State Binding | Not supported (uses methods like `setValue`) | `IState<int>` constructor for reactive updates |
| Maintain Space When Hidden | `maintainSpaceWhenHidden` (boolean) | Not supported |
| Clear/Reset | `clearValue()` / `resetValue()` methods | Not supported (use state `.Set()`) |
| Scroll Into View | `scrollIntoView(options)` method | Not supported |
