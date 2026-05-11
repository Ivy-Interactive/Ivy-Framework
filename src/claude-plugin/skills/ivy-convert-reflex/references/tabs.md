# Tabs

A tabbed interface that organizes content into layered sections (tab panels), displayed one at a time. Users switch between related content groups via clickable tab triggers.

## Reflex

```python
import reflex as rx

rx.tabs.root(
    rx.tabs.list(
        rx.tabs.trigger("Profile", value="profile"),
        rx.tabs.trigger("Security", value="security"),
        rx.tabs.trigger("Preferences", value="preferences"),
    ),
    rx.tabs.content("User profile information", value="profile"),
    rx.tabs.content("Security settings", value="security"),
    rx.tabs.content("User preferences", value="preferences"),
    default_value="profile",
)
```

## Ivy

```csharp
Layout.Tabs(
    new Tab("Profile", "User profile information"),
    new Tab("Security", "Security settings"),
    new Tab("Preferences", "User preferences")
)
```

## Parameters

| Parameter         | Documentation                                                                 | Ivy                                                        |
|-------------------|-------------------------------------------------------------------------------|------------------------------------------------------------|
| `default_value`   | Initial active tab value (uncontrolled)                                       | `SelectedIndex` (int-based instead of string-based)        |
| `value`           | Controlled active tab value, paired with `on_change`                          | `SelectedIndex` + `OnSelect` event                         |
| `on_change`       | Fires when the active tab changes                                             | `OnSelect`                                                 |
| `orientation`     | `"horizontal"` or `"vertical"` layout direction                               | Not supported                                              |
| `dir`             | Text direction (`"ltr"` / `"rtl"`)                                            | Not supported                                              |
| `activation_mode` | `"automatic"` or `"manual"` tab activation                                    | Not supported                                              |
| `disabled`        | Disables an individual tab trigger                                            | Not supported                                              |
| `force_mount`     | Forces tab content to mount even when inactive                                | Not supported                                              |
| `loop`            | Whether keyboard navigation loops from last to first trigger                  | Not supported                                              |
| `size`            | Tab list size (`"1"` or `"2"`)                                                | `Scale`                                                    |
| `color_scheme`    | Color scheme for tab triggers                                                 | Not supported                                              |
| N/A               | Closable tabs with close handler                                              | `OnClose`                                                  |
| N/A               | Refresh handler per tab                                                       | `OnRefresh`                                                |
| N/A               | Drag-and-drop tab reordering                                                  | `OnReorder`                                                |
| N/A               | Add button to create new tabs                                                 | `OnAddButtonClick` / `AddButtonText`                       |
| N/A               | Tab icons                                                                     | `Tab.Icon()`                                               |
| N/A               | Tab badges                                                                    | `Tab.Badge()`                                              |
| N/A               | Display variant (Tabs vs Content)                                             | `Variant` (`TabsVariant.Tabs` / `TabsVariant.Content`)    |
