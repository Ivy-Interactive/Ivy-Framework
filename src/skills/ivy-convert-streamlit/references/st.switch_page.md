# st.switch_page

Programmatically switch the current page in a multipage app. When called, the current page stops execution and the target page runs instead.

## Streamlit

```python
import streamlit as st

if st.button("Home"):
    st.switch_page("your_app.py")
if st.button("Page 1"):
    st.switch_page("pages/page_1.py")
if st.button("Page 2"):
    st.switch_page("pages/page_2.py")
```

## Ivy

```csharp
var navigator = UseNavigation();

if (Button("Home"))
    navigator.Navigate(typeof(HomeApp));
if (Button("Page 1"))
    navigator.Navigate(typeof(Page1App));
if (Button("Page 2"))
    navigator.Navigate(typeof(Page2App));
```

## Parameters

| Parameter    | Documentation                                                                                                                                  | Ivy                                                                        |
|--------------|------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------|
| page         | The path of the page to switch to, relative to the main script. Can be a `str`, `Path`, or `st.Page` object.                                  | `Type type` or `string uri` — target app type or URI string                |
| query_params | Query parameters to apply during navigation. Accepts a dict or list of tuples. Values can be strings or iterables for repeated keys.           | `object? appArgs` — strongly-typed argument object passed via `UseArgs<T>` |
