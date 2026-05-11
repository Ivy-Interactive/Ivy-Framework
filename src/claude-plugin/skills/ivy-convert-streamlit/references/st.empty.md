# st.empty

Inserts a single-element container that can be used to hold a placeholder and replace or clear its content dynamically. Each new element written to the empty container replaces the previous one. Calling `.empty()` on the returned object clears the content entirely.

## Streamlit

```python
import streamlit as st
import time

placeholder = st.empty()

for seconds in range(10):
    placeholder.write(f"⏳ {seconds} seconds have passed")
    time.sleep(1)

placeholder.empty()
```

## Ivy

Ivy has no direct equivalent of `st.empty()`. Since Ivy uses a reactive rendering model (similar to React), dynamic content replacement is achieved through `UseState` inside a `View`. When state changes, the view re-renders with the new content automatically.

```csharp
class CountdownView : ViewBase
{
    public override object? Build()
    {
        var seconds = UseState(0);

        UseEffect(() =>
        {
            var timer = new Timer(_ =>
            {
                if (seconds.Value < 10)
                    seconds.Set(seconds.Value + 1);
            }, null, 0, 1000);
            return () => timer.Dispose();
        }, []);

        return seconds.Value < 10
            ? Text.P($"⏳ {seconds.Value} seconds have passed")
            : null;
    }
}
```

## Parameters

`st.empty()` accepts no parameters (aside from the excluded `key`). In Ivy, the equivalent pattern is built from `UseState` and conditional rendering rather than a dedicated widget, so there are no directly comparable parameters.
