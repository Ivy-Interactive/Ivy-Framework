# st.sidebar

Adds a sidebar to the app pinned to the left side. Widgets and content placed in the sidebar help organize interactive elements, letting users focus on the main content area. The sidebar is resizable and supports most Streamlit elements using either object notation (`st.sidebar.selectbox(...)`) or context manager notation (`with st.sidebar:`).

## Streamlit

```python
import streamlit as st

# Object notation
add_selectbox = st.sidebar.selectbox(
    "How would you like to be contacted?",
    ("Email", "Home phone", "Mobile phone")
)

# Context manager notation
with st.sidebar:
    add_radio = st.radio(
        "Choose a shipping method",
        ("Standard (5-15 days)", "Express (2-5 days)")
    )
```

## Ivy

```csharp
new SidebarLayout(
    mainContent: new Text("Main content goes here"),
    sidebarContent: new Column(
        new SelectBox("How would you like to be contacted?",
            new[] { "Email", "Home phone", "Mobile phone" }),
        new RadioGroup("Choose a shipping method",
            new[] { "Standard (5-15 days)", "Express (2-5 days)" })
    ),
    sidebarHeader: new Text("My App"),
    sidebarFooter: new Text("v1.0")
).MainAppSidebar()
```

## Parameters

| Parameter          | Documentation                                                                                       | Ivy                          |
|--------------------|-----------------------------------------------------------------------------------------------------|------------------------------|
| mainContent        | Not an explicit parameter; everything outside `with st.sidebar:` is main content                    | `mainContent`                |
| sidebarContent     | Any element added via `st.sidebar.*` or inside `with st.sidebar:` appears in the sidebar            | `sidebarContent`             |
| sidebarHeader      | Not supported; must be manually added as the first element in the sidebar                           | `sidebarHeader`              |
| sidebarFooter      | Not supported; must be manually added as the last element in the sidebar                            | `sidebarFooter`              |
| MainAppSidebar     | Sidebar is always present and resizable by dragging its right border                                | `.MainAppSidebar()` property |
| MainContentPadding | Not supported                                                                                       | `MainContentPadding`         |
| Scale              | Not supported                                                                                       | `Scale`                      |
| Visible            | Not supported; sidebar is always visible when it has content                                        | `Visible`                    |
