# st.logo

Renders a logo in the upper-left corner of the app and its sidebar. When using `icon_image`, a smaller icon replaces the full logo when the sidebar is collapsed.

## Streamlit

```python
import streamlit as st

st.logo(
    "https://example.com/logo-wide.png",
    size="medium",
    link="https://example.com",
    icon_image="https://example.com/logo-icon.png",
)
```

## Ivy

In Ivy, logos are placed in the sidebar header via `AppShellSettings.Header()`, using an `Image` widget inside a layout.

```csharp
var appShellSettings = new AppShellSettings()
    .Header(
        Layout.Vertical().Gap(2)
        | new Image("/ivy/img/logo.png")
        | Text.Lead("My App")
    )
    .DefaultApp<MyApp>();

server.UseAppShell(() => new DefaultSidebarAppShell(appShellSettings));
```

## Parameters

| Parameter    | Documentation                                                                                          | Ivy                                                                             |
|--------------|--------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `image`      | The logo image displayed in the sidebar and upper-left corner. Supports URLs, file paths, and emojis.  | `new Image(src)` placed inside `AppShellSettings.Header()`                      |
| `size`       | Max height of the logo: `"small"` (20px), `"medium"` (24px), or `"large"` (32px). Default `"medium"`.  | Not supported (control size via `Image.Width` / `Image.Height`)                 |
| `link`       | External URL that opens when the logo is clicked. Must start with `http://` or `https://`.             | Not supported                                                                   |
| `icon_image` | Smaller image shown when the sidebar is collapsed. Ideal for square icons.                             | Not supported                                                                   |
