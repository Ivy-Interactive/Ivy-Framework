# Dropdown Menu

A dropdown menu presents a list of selectable options that appear near a trigger element (typically a button). It supports items, separators, nested submenus, and keyboard shortcut hints.

## Reflex

```python
rx.menu.root(
    rx.menu.trigger(
        rx.button("Options", variant="soft"),
    ),
    rx.menu.content(
        rx.menu.item("Edit", shortcut="⌘ E"),
        rx.menu.item("Duplicate", shortcut="⌘ D"),
        rx.menu.separator(),
        rx.menu.item("Archive", shortcut="⌘ N"),
        rx.menu.sub(
            rx.menu.sub_trigger("More"),
            rx.menu.sub_content(
                rx.menu.item("Move to project…"),
                rx.menu.item("Move to folder…"),
                rx.menu.separator(),
                rx.menu.item("Advanced options…"),
            ),
        ),
        rx.menu.separator(),
        rx.menu.item("Share"),
        rx.menu.item("Add to favorites"),
        rx.menu.separator(),
        rx.menu.item("Delete", shortcut="⌘ ⌫", color="red"),
    ),
)
```

## Ivy

```csharp
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value),
    new Button("Options"),
    MenuItem.Default("Edit").Tag("edit"),
    MenuItem.Default("Duplicate").Tag("duplicate"),
    MenuItem.Separator(),
    MenuItem.Default("Archive").Tag("archive"),
    MenuItem.Default("More")
        .Children(
            MenuItem.Default("Move to project…"),
            MenuItem.Default("Move to folder…"),
            MenuItem.Separator(),
            MenuItem.Default("Advanced options…")
        ),
    MenuItem.Separator(),
    MenuItem.Default("Share"),
    MenuItem.Default("Add to favorites"),
    MenuItem.Separator(),
    MenuItem.Default("Delete").Tag("delete"))
```

## Parameters

### Root / Constructor

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Trigger element | `rx.menu.trigger(child)` wraps the trigger | Passed as second constructor argument |
| Items | Nested `rx.menu.item(...)` inside `rx.menu.content` | `MenuItem[]` passed as third+ constructor arguments |
| `default_open` | `bool` — start open | Not supported |
| `open` | `bool` — controlled open state | `Visible` (`bool`, read-only) |
| `modal` | `bool` — render as modal overlay | Not supported |
| `dir` | `"ltr" \| "rtl"` — text direction | Not supported |
| `on_open_change` | Event fired when open state changes | Not supported |
| `on_select` | Per-item via `rx.menu.item(on_select=...)` | `OnSelect` event on the root (`Func<Event<DropDownMenu, object>, ValueTask>`) |

### Content / Positioning

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `size` | `"1" \| "2"` | `Scale` (`Scale?`) |
| `variant` | `"solid" \| "soft"` | Not supported |
| `color_scheme` | Radix color name (e.g. `"tomato"`, `"red"`) | Not supported |
| `high_contrast` | `bool` | Not supported |
| `side` | `"top" \| "right" \| "bottom" \| "left"` | `.Top()`, `.Right()`, `.Bottom()`, `.Left()` fluent methods |
| `side_offset` | `int \| float` | Not supported |
| `align` | `"start" \| "center" \| "end"` | `.Align(AlignOptions)` |
| `align_offset` | `int \| float` | `.AlignOffset(int)` |
| `avoid_collisions` | `bool` | Not supported |
| `collision_padding` | `float \| int \| dict` | Not supported |
| `sticky` | `"partial" \| "always"` | Not supported |
| `hide_when_detached` | `bool` | Not supported |
| `loop` | `bool` — loop keyboard focus | Not supported |
| `force_mount` | `bool` — force DOM mount | Not supported |
| Header | Not supported | `.Header(Text)` — adds a contextual header above items |
| Width / Height | Not supported | `Width` / `Height` (`Size`) |

### Content Events

| Event | Reflex | Ivy |
|-------|--------|-----|
| `on_close_auto_focus` | Fired when menu closes and focus returns | Not supported |
| `on_escape_key_down` | Fired on Escape key press | Not supported |
| `on_pointer_down_outside` | Fired on click outside the menu | Not supported |
| `on_focus_outside` | Fired when focus moves outside | Not supported |
| `on_interact_outside` | Fired on any interaction outside | Not supported |

### Item

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Label / text | First positional arg to `rx.menu.item("Edit")` | `MenuItem.Default("Edit")` |
| `shortcut` | `str` — displays shortcut hint (e.g. `"⌘ E"`) | Not supported |
| `disabled` | `bool` | Not supported |
| `color` | CSS color string (e.g. `"red"`) | Not supported |
| `text_value` | `str` — override typeahead text | Not supported |
| Tag / identifier | Not supported | `.Tag(string)` — value passed in `OnSelect` event |
| Checkbox item | Not supported | `MenuItem.Checkbox("Label")` with optional `.Checked()` |

### Submenu

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Nested submenus | `rx.menu.sub` + `rx.menu.sub_trigger` + `rx.menu.sub_content` | `.Children(MenuItem[])` on any `MenuItem` |

### Separator

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Visual divider | `rx.menu.separator()` | `MenuItem.Separator()` |

### Fluent API

| Feature | Reflex | Ivy |
|---------|--------|-----|
| Attach to button | Not supported — must compose `rx.menu.root(rx.menu.trigger(...), ...)` | `button.WithDropDown(items...)` extension method |
