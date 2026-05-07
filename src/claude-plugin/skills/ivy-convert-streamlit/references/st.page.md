# st.Page

Configures a page in a multipage app. Each `st.Page` defines a page source (a Python file or callable), along with display metadata like title, icon, and URL path. Pages are passed to `st.navigation` to declare the app's page structure.

## Streamlit

```python
import streamlit as st

def page2():
    st.title("Second page")

pg = st.navigation([
    st.Page("page1.py", title="First page", icon="🔥"),
    st.Page(page2, title="Second page", icon=":material/favorite:"),
])
pg.run()
```

## Ivy

In Ivy, pages are defined as classes decorated with the `[App]` attribute. Each app class inherits from `ViewBase` and implements a `Build()` method. Navigation and page registration are handled automatically by the framework based on conventions and attribute configuration.

```csharp

[App(title: "First page", icon: Icons.Whatshot)]
public class FirstPageApp : ViewBase
{
    public override object? Build()
    {
        return new Text("First page");
    }
}

[App(title: "Second page", icon: Icons.Favorite)]
public class SecondPageApp : ViewBase
{
    public override object? Build()
    {
        return new Text("Second page");
    }
}
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| page | `str`, `Path`, or `callable` - the page source (a file path or function) | Class inheriting from `ViewBase` with `[App]` attribute |
| title | `str` - display name in browser tabs and nav menus; inferred from filename if `None` | `title:` named parameter on `[App]`; inferred from class name if omitted |
| icon | `str` - emoji or Material Symbol (e.g. `":material/favorite:"`) | `icon:` named parameter on `[App]` using the `Icons` enum (e.g. `Icons.Favorite`) |
| url_path | `str` - URL pathname relative to app root; inferred from filename if `None` | `id:` or `path:` on `[App]`; auto-generated from namespace/class name via convention-based routing |
| default | `bool` - whether the page is the initial page; sets url_path to empty | `DefaultApp<T>()` in Chrome settings (e.g. `app.UseAppShell(AppShellSettings.Default().DefaultApp<MyApp>())`) |
