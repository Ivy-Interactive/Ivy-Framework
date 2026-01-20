---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - loading
  - percentage
  - bar
  - indicator
  - status
  - completion
---

# Progress

<Ingress>
Show task completion status with customizable progress bars that support dynamic updates and multiple [color variants](../../01_Onboarding/02_Concepts/17_Theming.md).
</Ingress>

The `Progress` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is used to visually represent the completion status of a task or process. It displays a visual progress bar that can be customized with different color variants and can be bound to [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic updates.

## Example

```csharp demo-tabs
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

<WidgetDocs Type="Ivy.Progress" ExtensionTypes="Ivy.ProgressExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Progress.cs"/>
