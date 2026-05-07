# st.spinner

Displays a temporary loading spinner with a message while a block of code executes. Commonly used as a context manager to provide visual feedback during long-running operations.

## Streamlit

```python
import streamlit as st
import time

with st.spinner("Wait for it...", show_time=True):
    time.sleep(5)
st.success("Done!")
```

## Ivy

Ivy has no direct spinner widget. The closest equivalent is `Progress` for showing task status, or an animated `LoaderCircle` icon for the visual spinner effect.

```csharp
public class SpinnerDemo : ViewBase
{
    public override object? Build()
    {
        var isLoading = UseState(false);

        return Layout.Vertical(
            isLoading.Value
                ? Layout.Horizontal(
                    Icons.LoaderCircle
                        .ToIcon()
                        .WithAnimation(AnimationType.Rotate)
                        .Trigger(AnimationTrigger.Auto)
                        .Duration(2),
                    new Text("Wait for it...")
                )
                : new Text("Done!"),
            new Button("Run", async _ =>
            {
                isLoading.Set(true);
                await Task.Delay(5000);
                isLoading.Set(false);
            })
        );
    }
}
```

## Parameters

| Parameter   | Documentation                                                                            | Ivy                                                   |
|-------------|------------------------------------------------------------------------------------------|-------------------------------------------------------|
| `text`      | A string to display next to the spinner. Supports GitHub-flavored Markdown. Default `"In progress..."`. | Use a `Text` widget placed next to the animated icon. |
| `show_time` | A bool. When `True`, displays the elapsed time with 0.1s precision. Default `False`.     | Not supported                                         |
