# Toast

Non-blocking notification that appears temporarily and disappears automatically. Used to provide quick feedback about user actions without interrupting workflow.

## Reflex

```python
import reflex as rx

rx.button(
    "Show Toast",
    on_click=rx.toast(
        "Operation completed!",
        description="Additional details here",
        duration=5000,
        position="top-center",
        close_button=True,
    ),
)

# Presets
rx.button("Success", on_click=rx.toast.success("Saved!"))
rx.button("Error", on_click=rx.toast.error("Failed!"))
rx.button("Warning", on_click=rx.toast.warning("Careful!"))
rx.button("Info", on_click=rx.toast.info("FYI"))
```

## Ivy

```csharp
public class ToastDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.Horizontal(
            new Button("Success", _ =>
                client.Toast("Operation completed!", "Success")
            ),
            new Button("Error", _ =>
                client.Toast("Something failed!", "Error")
            ),
            new Button("Info", _ =>
                client.Toast("Here's some info", "Info")
            ),
            new Button("Simple", _ =>
                client.Toast("Just a message")
            )
        );
    }
}
```

## Parameters

| Parameter      | Documentation                                      | Ivy                                          |
|----------------|-----------------------------------------------------|----------------------------------------------|
| message        | The main text content of the toast                  | First argument to `client.Toast()`           |
| description    | Secondary text rendered underneath the title        | Not supported                                |
| close_button   | Display a close button on the toast                 | Not supported                                |
| invert         | Dark toast in light mode and vice versa             | Not supported                                |
| important      | Control screen reader sensitivity                   | Not supported                                |
| duration       | Auto-close delay in milliseconds                    | Not supported (auto-managed)                 |
| position       | Toast placement location (e.g. `"bottom-right"`)   | Not supported (fixed position)               |
| dismissible    | Allow user to dismiss the toast                     | Not supported                                |
| action         | Primary button with callback                        | Not supported                                |
| cancel         | Secondary button with callback                      | Not supported                                |
| id             | Custom toast identifier                             | Not supported                                |
| unstyled       | Remove default styling                              | Not supported                                |
| style          | Custom CSS styling                                  | Not supported                                |
| on_dismiss     | Callback when toast is closed or swiped away        | Not supported                                |
| on_auto_close  | Callback when toast auto-closes after timeout       | Not supported                                |
| type/presets   | `success`, `error`, `warning`, `info` presets       | Second argument: `"Success"`, `"Error"`, `"Info"` |
| exception      | Not supported                                       | `client.Toast(exception)` auto-formats errors |
