# Context Menu

A popup menu that appears on right-click (or long-press), typically used to offer contextual actions related to the element the user interacted with. It supports nested submenus, separators, keyboard shortcuts, and item-level styling.

## Reflex

```python
rx.context_menu.root(
    rx.context_menu.trigger(
        rx.button("Right click me"),
    ),
    rx.context_menu.content(
        rx.context_menu.item("Edit", shortcut="⌘ E"),
        rx.context_menu.item("Duplicate", shortcut="⌘ D"),
        rx.context_menu.separator(),
        rx.context_menu.item("Archive", shortcut="⌘ N"),
        rx.context_menu.sub(
            rx.context_menu.sub_trigger("More"),
            rx.context_menu.sub_content(
                rx.context_menu.item("Move to project…"),
                rx.context_menu.item("Move to folder…"),
                rx.context_menu.separator(),
                rx.context_menu.item("Advanced options…"),
            ),
        ),
        rx.context_menu.separator(),
        rx.context_menu.item("Share"),
        rx.context_menu.item("Add to favorites"),
        rx.context_menu.separator(),
        rx.context_menu.item("Delete", shortcut="⌘ ⌫", color="red"),
    ),
)
```

## Ivy

Ivy does not have a dedicated context menu (right-click) widget. The closest equivalent is `DropDownMenu`, which provides a similar menu structure but is activated by clicking a trigger element rather than right-clicking.

```csharp
new DropDownMenu(
    onSelect: @evt => client.Toast("Selected: " + @evt.Value),
    trigger: new Button("User Menu"),
    items: new[]
    {
        MenuItem.Default("Edit"),
        MenuItem.Default("Duplicate"),
        MenuItem.Separator(),
        MenuItem.Default("Archive"),
        MenuItem.Separator(),
        MenuItem.Default("Share"),
        MenuItem.Default("Add to favorites"),
        MenuItem.Separator(),
        MenuItem.Default("Delete")
    })
    .Header(Text.Muted("Options"))
    .Bottom()
```

## Parameters

### Root / Container

| Parameter          | Reflex (`context_menu.root`)          | Ivy (`DropDownMenu`)               |
|--------------------|---------------------------------------|------------------------------------|
| `modal`            | `bool` - Modal behavior               | Not supported                      |
| `dir`              | `"ltr"` \| `"rtl"` - Text direction   | Not supported                      |
| `on_open_change`   | Event - Fired when open state changes | Not supported                      |
| `trigger`          | Via `context_menu.trigger` child       | Constructor param `trigger`        |
| `items`            | Via `context_menu.content` children    | Constructor param `items`          |
| `on_select`        | Via `context_menu.item.on_select`      | Constructor param `onSelect`       |

### Menu Content / Positioning

| Parameter            | Reflex (`context_menu.content`)                      | Ivy (`DropDownMenu`)                  |
|----------------------|------------------------------------------------------|---------------------------------------|
| `size`               | `"1"` \| `"2"` - Menu size                           | `Scale` property                      |
| `variant`            | `"solid"` \| `"soft"`                                | Not supported                         |
| `color_scheme`       | Color scheme string (e.g. `"tomato"`)                | Not supported                         |
| `high_contrast`      | `bool` - High contrast mode                          | Not supported                         |
| `side`               | `"top"` \| `"right"` \| `"bottom"` \| `"left"`      | `Side` (`SideOptions`)                |
| `side_offset`        | `int` \| `float` - Offset from anchor                | Not supported                         |
| `align`              | `"start"` \| `"center"` \| `"end"`                   | `Align` (`AlignOptions`)              |
| `align_offset`       | `int` \| `float` - Alignment offset                  | `AlignOffset` (`int`)                 |
| `avoid_collisions`   | `bool` - Avoid viewport edge collisions              | Not supported                         |
| `sticky`             | `"partial"` \| `"always"`                            | Not supported                         |
| `width`              | Not supported                                        | `Width` (`Size`)                      |
| `height`             | Not supported                                        | `Height` (`Size`)                     |
| `visible`            | Not supported                                        | `Visible` (`bool`)                    |

### Menu Items

| Parameter        | Reflex (`context_menu.item`)              | Ivy (`MenuItem`)                                  |
|------------------|-------------------------------------------|---------------------------------------------------|
| `shortcut`       | `str` - Display keyboard shortcut         | Not supported                                     |
| `disabled`       | `bool` - Disable the item                 | Not supported                                     |
| `color`          | Color string (e.g. `"red"`)              | Not supported                                     |
| `text_value`     | `str` - Typeahead text                    | Not supported                                     |
| `on_select`      | Event - Fired when item is selected       | Handled via root `onSelect` with `@evt.Value`     |
| Separator        | `context_menu.separator()`                | `MenuItem.Separator()`                             |
| Checkbox item    | Not supported                             | `MenuItem.Checkbox("label").Checked()`             |
| Header           | Not supported                             | `.Header(Text.Muted("..."))`                       |

### Submenus

| Parameter          | Reflex (`context_menu.sub`)             | Ivy (`DropDownMenu`)   |
|--------------------|-----------------------------------------|------------------------|
| `open`             | `bool` - Controlled open state          | Not supported          |
| `default_open`     | `bool` - Default open state             | Not supported          |
| `on_open_change`   | Event - Fired when submenu open changes | Not supported          |
| Nested menus       | Via `sub` / `sub_trigger` / `sub_content` | Not supported        |

### Activation Method

| Feature              | Reflex                  | Ivy                       |
|----------------------|-------------------------|---------------------------|
| Right-click trigger  | Yes (native behavior)   | Not supported             |
| Click trigger        | Not supported            | Yes (native behavior)    |
