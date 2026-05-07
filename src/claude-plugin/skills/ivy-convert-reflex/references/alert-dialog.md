# Alert Dialog

A modal confirmation dialog that interrupts the user and expects a response. It overlays the current content and requires the user to confirm or cancel before continuing.

## Reflex

```python
rx.alert_dialog.root(
    rx.alert_dialog.trigger(
        rx.button("Revoke access", color_scheme="red"),
    ),
    rx.alert_dialog.content(
        rx.alert_dialog.title("Revoke access"),
        rx.alert_dialog.description(
            "Are you sure? This application will no longer be accessible and any existing sessions will be expired.",
            size="2",
        ),
        rx.flex(
            rx.alert_dialog.cancel(
                rx.button("Cancel", variant="soft", color_scheme="gray"),
            ),
            rx.alert_dialog.action(
                rx.button("Revoke access", color_scheme="red", variant="solid"),
            ),
            spacing="3",
            margin_top="16px",
            justify="end",
        ),
        style={"max_width": 450},
    ),
)
```

## Ivy

```csharp
public class RevokeAccessDialog : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);

        return Layout.Vertical(
            new Button("Revoke access", _ => isOpen.Set(true)),
            isOpen.Value
                ? new Dialog(
                    _ => isOpen.Set(false),
                    new DialogHeader("Revoke access"),
                    new DialogBody(
                        Text.P("Are you sure? This application will no longer be accessible and any existing sessions will be expired.")
                    ),
                    new DialogFooter(
                        new Button("Cancel", _ => isOpen.Set(false)).Outline(),
                        new Button("Revoke access", _ =>
                        {
                            isOpen.Set(false);
                        })
                    )
                )
                : null
        );
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `open` | Controls whether the dialog is open (`bool` on `alert_dialog.root`) | State-controlled via conditional rendering (`isOpen.Value ? new Dialog(...) : null`) |
| `default_open` | Sets the initial open state (`bool` on `alert_dialog.root`) | Not supported |
| `on_open_change` | Event fired when the open state changes (on `alert_dialog.root`) | `OnClose` event on `Dialog` constructor (close only) |
| `size` | Controls the content size (`"1"`, `"2"`, etc. on `alert_dialog.content`) | `Width` / `Height` properties on `Dialog` |
| `force_mount` | Forces the dialog to remain in the DOM when closed (`bool` on `alert_dialog.content`) | Not supported |
| `on_open_auto_focus` | Event fired when dialog opens and focus is set (on `alert_dialog.content`) | Not supported |
| `on_close_auto_focus` | Event fired when dialog closes and focus returns (on `alert_dialog.content`) | Not supported |
| `on_escape_key_down` | Event fired when escape key is pressed (on `alert_dialog.content`) | Not supported |
| `alert_dialog.trigger` | Wraps a control that opens the dialog declaratively | Not supported (open state managed manually via button `on_click`) |
| `alert_dialog.title` | Accessible title announced when the dialog is opened | `DialogHeader` with a `string` title |
| `alert_dialog.description` | Accessible description announced when the dialog is opened | `description` parameter in `ToDialog()` extension method |
| `alert_dialog.action` | Wraps the confirm/action control (visually distinct from cancel) | Button in `DialogFooter` (no semantic distinction from cancel) |
| `alert_dialog.cancel` | Wraps the cancel control (visually distinct from action) | Button in `DialogFooter` (no semantic distinction from action) |
