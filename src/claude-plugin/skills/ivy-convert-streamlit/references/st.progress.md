# st.progress

Display a progress bar that visually represents task completion status, supporting dynamic updates.

## Streamlit

```python
import streamlit as st
import time

progress_text = "Operation in progress. Please wait."
my_bar = st.progress(0, text=progress_text)

for percent_complete in range(100):
    time.sleep(0.01)
    my_bar.progress(percent_complete + 1, text=progress_text)
time.sleep(1)
my_bar.empty()
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

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| value | `int` or `float` — 0-100 (int) or 0.0-1.0 (float). Sets the progress completion. | `Value` (`int?`) — 0-100. Can also bind to `IState<int>` via constructor. |
| text | `str` or `None` — A message displayed above the progress bar. Supports markdown. Default: `None`. | `Goal` (`string`) — A label displayed for the progress status. |
| color | Not supported | `ColorVariant` (`ColorVariants`) — Visual styling variant for the progress bar. |
