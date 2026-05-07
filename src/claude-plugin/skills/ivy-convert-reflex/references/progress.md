# Progress

Displays a visual indicator of task completion status, showing how much of a process has been completed.

## Reflex

```python
import reflex as rx

class State(rx.State):
    progress_value: int = 50

def index() -> rx.Component:
    return rx.progress(value=State.progress_value, max=100)
```

## Ivy

```csharp
public class ProgressDemo : ViewBase
{
    public override object? Build()
    {
        var progress = UseState(50);

        return new Progress(progress.Value).Goal($"{progress.Value}% Complete");
    }
}
```

## Parameters

| Parameter      | Documentation                                      | Ivy                                          |
|----------------|-----------------------------------------------------|----------------------------------------------|
| value          | Sets the progress percentage/amount                 | `Value` (int?) via constructor or property    |
| max            | Maximum value for the progress bar                  | Not supported                                |
| color_scheme   | Color theme (e.g. "tomato", "red")                  | `ColorVariant` (ColorVariants)               |
| size           | Visual size of the component ("1", "2", ...)        | `Scale` (Scale?, read-only)                  |
| variant        | Style variant ("classic", "surface", ...)           | Not supported                                |
| high_contrast  | Enhanced contrast mode (bool)                       | Not supported                                |
| radius         | Corner radius ("none", "small", ...)                | Not supported                                |
| duration       | Animation duration (str)                            | Not supported                                |
| fill_color     | Custom fill color (str)                             | Not supported                                |
| width          | Component width (default 100%)                      | `Width` (Size, read-only)                    |
| —              | —                                                   | `Goal` (string) — label text for the bar     |
| —              | —                                                   | `Height` (Size, read-only)                   |
| —              | —                                                   | `Visible` (bool, read-only)                  |
