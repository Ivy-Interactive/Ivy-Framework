# Dialog

A modal overlay that interrupts the current workflow to display focused content, request information, or ask for confirmation. The dialog appears on top of the page content and typically includes a title, description, body content, and action buttons.

## Reflex

```python
rx.dialog.root(
    rx.dialog.trigger(rx.button("Edit Profile", size="4")),
    rx.dialog.content(
        rx.dialog.title("Edit Profile"),
        rx.dialog.description(
            "Change your profile details and preferences.",
            size="2",
            margin_bottom="16px",
        ),
        rx.flex(
            rx.text("Name", as_="div", size="2", margin_bottom="4px", weight="bold"),
            rx.input(default_value="Freja Johnson", placeholder="Enter your name"),
            rx.text("Email", as_="div", size="2", margin_bottom="4px", weight="bold"),
            rx.input(default_value="freja@example.com", placeholder="Enter your email"),
            direction="column",
            spacing="3",
        ),
        rx.flex(
            rx.dialog.close(
                rx.button("Cancel", color_scheme="gray", variant="soft"),
            ),
            rx.dialog.close(
                rx.button("Save"),
            ),
            spacing="3",
            margin_top="16px",
            justify="end",
        ),
    ),
)
```

## Ivy

```csharp
public class EditProfileDialog : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);

        return Layout.Vertical(
            new Button("Edit Profile", _ => isOpen.Set(true)),
            isOpen.Value
                ? new Dialog(
                    _ => isOpen.Set(false),
                    new DialogHeader("Edit Profile"),
                    new DialogBody(
                        Text.P("Change your profile details and preferences."),
                        UseState("Freja Johnson").ToTextInput().Placeholder("Enter your name"),
                        UseState("freja@example.com").ToTextInput().Placeholder("Enter your email")
                    ),
                    new DialogFooter(
                        new Button("Cancel", _ => isOpen.Set(false)).Outline(),
                        new Button("Save", _ => isOpen.Set(false))
                    )
                )
                : null
        );
    }
}
```

## Parameters

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Open state | `open` prop on `rx.dialog.root` | Manual `UseState(bool)` + conditional rendering |
| Default open | `default_open` prop on `rx.dialog.root` | Set initial `UseState` value |
| Title | `rx.dialog.title("...")` sub-component | `new DialogHeader("...")` section |
| Description | `rx.dialog.description("...")` sub-component | Passed as content in `DialogBody` or via `ToDialog(description:)` |
| Body content | Placed inside `rx.dialog.content` | `new DialogBody(...)` section |
| Footer / actions | Placed inside `rx.dialog.content` manually | `new DialogFooter(...)` section |
| Trigger | `rx.dialog.trigger(...)` wraps the opening control | Manual button with `isOpen.Set(true)` |
| Close | `rx.dialog.close(...)` wraps closing controls | `OnClose` callback on `Dialog` constructor |
| Size | `size` prop on `rx.dialog.content` (`"1"` - `"4"`) | `Width` / `Height` properties |
| On open change | `on_open_change` event on `rx.dialog.root` | Not supported (use `OnClose` + open state) |
| On open auto focus | `on_open_auto_focus` event on `rx.dialog.content` | Not supported |
| On close auto focus | `on_close_auto_focus` event on `rx.dialog.content` | Not supported |
| On escape key down | `on_escape_key_down` event on `rx.dialog.content` | Not supported |
| On pointer down outside | `on_pointer_down_outside` event on `rx.dialog.content` | Not supported |
| On interact outside | `on_interact_outside` event on `rx.dialog.content` | Not supported |
| Scale | Not supported | `Scale` property |
| Visibility | Controlled via `open` prop | `Visible` property |
| Form dialog shorthand | Not built-in (compose with `rx.form`) | `.ToForm().ToDialog()` extension method |
| Content dialog shorthand | Not built-in | `.ToDialog()` extension method |
