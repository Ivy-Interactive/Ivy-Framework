# st.navigation

Configure the available pages in a multipage app. Returns the current page selected by the user, which must be executed with `.run()`.

## Streamlit

```python
import streamlit as st

pages = {
    "Your account": [
        st.Page("create_account.py", title="Create your account"),
        st.Page("manage_account.py", title="Manage your account"),
    ],
    "Resources": [
        st.Page("learn.py", title="Learn about us"),
    ],
}

pg = st.navigation(pages, position="sidebar", expanded=False)
pg.run()
```

## Ivy

In Ivy, multipage navigation is declared via the `[App]` attribute on `ViewBase` classes. Pages are automatically discovered and grouped by namespace. Navigation UI is rendered through layouts like `SidebarLayout`.

```csharp
// Pages are defined as separate classes with the [App] attribute.
// Namespace structure determines grouping (e.g. Apps.Account, Apps.Resources).

namespace MyProject.Apps.Account;

[App(title: "Create your account", icon: Icons.PersonAdd, order: 1)]
public class CreateAccountApp : ViewBase
{
    public override object? Build() => Text("Create Account Page");
}

[App(title: "Manage your account", icon: Icons.Settings, order: 2)]
public class ManageAccountApp : ViewBase
{
    public override object? Build() => Text("Manage Account Page");
}

namespace MyProject.Apps.Resources;

[App(title: "Learn about us", icon: Icons.Book)]
public class LearnApp : ViewBase
{
    public override object? Build() => Text("Learn Page");
}
```

## Parameters

| Parameter    | Documentation                                                                                                          | Ivy                                                                                                                              |
|--------------|------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| `pages`      | The available pages for the app. A list for flat navigation or a dict mapping section headers to lists of pages.       | Pages are declared as `[App]` classes. Grouping is automatic via namespaces or the `path` parameter on the `[App]` attribute.    |
| `position`   | Position of the navigation menu: `"sidebar"`, `"top"`, or `"hidden"`. Default `"sidebar"`.                             | Determined by the layout used: `SidebarLayout`, `HeaderLayout`, or omitting navigation entirely. Set `isVisible: false` to hide. |
| `expanded`   | Whether the sidebar navigation menu is expanded. Only used when `position="sidebar"`. Default `False`.                 | `groupExpanded` parameter on the `[App]` attribute controls whether a navigation group is expanded by default.                   |
