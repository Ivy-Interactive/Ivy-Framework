# Popover

A popover displays rich content in an overlay positioned relative to a trigger element, toggled by clicking the trigger. It consists of a root container, a trigger button, a content area, and an optional close button.

## Reflex

```python
rx.popover.root(
    rx.popover.trigger(
        rx.button("Open Popover"),
    ),
    rx.popover.content(
        rx.text("Popover content goes here"),
        rx.popover.close(
            rx.button("Close"),
        ),
    ),
)
```

## Ivy

Ivy has no direct Popover widget. The closest equivalent is `DropDownMenu`, which is a click-triggered overlay positioned relative to a trigger element with side/align control. However, it is specialised for menu items rather than arbitrary content.

```csharp
new DropDownMenu(
    @evt => client.Toast("Selected: " + @evt.Value),
    new Button("Open Menu"),
    MenuItem.Default("Profile"),
    MenuItem.Default("Settings"),
    MenuItem.Separator(),
    MenuItem.Default("Logout")
).Bottom().Header(Text.P("Account"));
```

## Parameters

### popover.root

| Parameter        | Documentation                             | Ivy                                      |
|------------------|-------------------------------------------|------------------------------------------|
| `open`           | Controls visibility state (bool)          | Managed via `UseState` + conditional rendering |
| `modal`          | Modal behaviour (bool, default false)     | Not supported                            |
| `default_open`   | Initial open state (bool)                 | Not supported                            |
| `on_open_change` | Triggered when open state changes (event) | `OnSelect` (fires on item selection)     |

### popover.content

| Parameter           | Documentation                                      | Ivy (`DropDownMenu`)                    |
|---------------------|----------------------------------------------------|-----------------------------------------|
| `size`              | Content sizing (`"1"`, `"2"`, ...)                 | `Width` / `Height` / `Scale`            |
| `side`              | Positioning side (`"top"`, `"right"`, `"bottom"`, `"left"`) | `Side` (`.Top()`, `.Right()`, `.Bottom()`, `.Left()`) |
| `side_offset`       | Distance from trigger (int)                        | Not supported                           |
| `align`             | Alignment (`"start"`, `"center"`, `"end"`)         | `Align` / `AlignOffset`                 |
| `align_offset`      | Alignment offset (int)                             | `AlignOffset`                           |
| `avoid_collisions`  | Auto-adjust positioning to stay in viewport (bool) | Not supported                           |
| `sticky`            | Sticky behaviour (`"partial"`, `"always"`)         | Not supported                           |
| `hide_when_detached`| Hide when trigger scrolls out of view (bool)       | Not supported                           |

### popover.trigger / popover.close

| Parameter | Documentation                  | Ivy                                                  |
|-----------|--------------------------------|------------------------------------------------------|
| `trigger` | Any child element acts as trigger | First non-`MenuItem` argument to `DropDownMenu` constructor |
| `close`   | Button that closes the popover | Not supported (menu closes automatically on selection) |
