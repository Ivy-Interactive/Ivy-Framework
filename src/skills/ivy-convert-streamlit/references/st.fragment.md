# st.fragment

Decorator that turns a function into a **fragment** — a section of the app that can rerun independently from the full page. When a user interacts with a widget inside a fragment, only that fragment re-executes instead of the entire app. Supports automatic periodic reruns via `run_every`.

## Streamlit

```python
import streamlit as st

if "app_runs" not in st.session_state:
    st.session_state.app_runs = 0
    st.session_state.fragment_runs = 0

@st.fragment
def my_fragment():
    st.session_state.fragment_runs += 1
    st.button("Rerun fragment")
    st.write(f"Fragment ran {st.session_state.fragment_runs} times.")

st.session_state.app_runs += 1
my_fragment()
st.button("Rerun full app")
st.write(f"App ran {st.session_state.app_runs} times.")
```

## Ivy

Ivy does not have a direct equivalent of `st.fragment`. Ivy uses a reactive model where state changes automatically trigger re-renders only for affected parts of the UI, so independent reruns are the default behavior rather than an opt-in decorator.

Ivy does have a `Fragment` widget, but it serves a different purpose: it groups child elements without producing extra HTML markup (similar to React's `<Fragment>`).

```csharp
// Ivy's Fragment is a DOM grouping primitive, not an execution-flow concept.
new Fragment(
    Text.P("Welcome"),
    Text.P("Grouped text without extra DOM elements.")
);

// For periodic updates (similar to run_every), use Observable.Interval:
Observable.Interval(TimeSpan.FromSeconds(5))
```

## Parameters

| Parameter   | Documentation                                                                                                                                                      | Ivy           |
|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| `func`      | The function to turn into a fragment. Can be used as `@st.fragment` or `@st.fragment(run_every="5s")`.                                                             | Not supported |
| `run_every`  | Automatic rerun interval. Accepts `int`/`float` (seconds), `timedelta`, or Pandas duration strings like `"1d"`, `"1.5 days"`, `"1h23s"`. `None` disables it.     | `Observable.Interval(TimeSpan)` provides periodic updates but requires explicit subscription |
