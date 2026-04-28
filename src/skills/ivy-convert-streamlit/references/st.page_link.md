# st.page_link

Display a link to another page in a multipage app or to an external page. When clicking a link to another app page, execution stops and runs the specified page. External links open in a new tab.

## Streamlit

```python
import streamlit as st

st.page_link("pages/page_1.py", label="Page 1", icon="1️⃣")
st.page_link("http://www.google.com", label="Google", icon="🌎")
st.page_link("pages/page_2.py", label="Page 2", icon="2️⃣", disabled=True)
```

## Ivy

Ivy has no single `page_link` widget. Internal navigation uses the `UseNavigation()` hook, while external links use a `Button` with a `Link` variant and `.Url()`.

```csharp
// Internal navigation
var navigator = UseNavigation();
new Button("Page 1", onClick: _ => navigator.Navigate(typeof(Page1App)))
    .Icon(Icons.LooksOne);

// External link
new Button("Google")
    .Variant(ButtonVariant.Link)
    .Icon(Icons.Public)
    .Url("https://www.google.com")
    .OpenInNewTab();

// Disabled link
new Button("Page 2", onClick: _ => navigator.Navigate(typeof(Page2App)))
    .Icon(Icons.LooksTwo)
    .Disabled(true);
```

## Parameters

| Parameter     | Documentation                                                                                        | Ivy                                                                                       |
|---------------|------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| page          | Path to an app page or external URL (str, Path, or StreamlitPage)                                    | `.Url("...")` for external links, `navigator.Navigate(typeof(App))` for internal pages    |
| label         | Display text for the link. Supports GitHub-flavored Markdown                                         | `title` constructor parameter: `new Button("label")`                                      |
| icon          | Emoji, Material Symbols icon (`:material/icon_name:`), or `"spinner"`                                | `.Icon(Icons.Name)`                                                                       |
| icon_position | Position of the icon relative to the label (`"left"` or `"right"`, default `"left"`)                 | Not supported                                                                             |
| help          | Tooltip text shown on hover. Supports GitHub-flavored Markdown                                       | `.Tooltip("text")`                                                                        |
| disabled      | If `True`, the link is grayed out and not clickable (default `False`)                                | `.Disabled(true)`                                                                         |
| query_params  | Query parameters appended to the target page URL (dict or list of tuples)                            | `navigator.Navigate(typeof(App), new Args(...))` passes typed arguments instead            |
