---
searchHints:
  - dialog
  - toast
  - notification
  - modal
  - confirm
  - message
---

# Alerts & Notifications

<Ingress>
Communicate with users effectively using modal dialog alerts for important confirmations and toast notifications for feedback messages.
</Ingress>

## Types of Alerts

Ivy provides two main types of alerts:

1. **Dialog Alerts** - Modal dialogs for important confirmations and decisions
2. **Toast Notifications** - Non-blocking notifications for [feedback](../../02_Widgets/04_Inputs/13_Feedback.md) and status updates

## Dialog Alerts

Dialog alerts are modal windows that require [user interaction](./07_EventHandlers.md). They're perfect for confirmations, important messages, or collecting user decisions.

### Basic Dialog Alert

```csharp demo-below
public class BasicDialogAlertDemo : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = UseAlert();
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            new Button("Show Alert", _ => 
                showAlert("Are you sure you want to continue?", result => {
                    client.Toast($"You selected: {result}");
                }, "Alert title")
            ),
            alertView
        );
    }
}
```

### Alert Button Sets

Dialog alerts support different button combinations:

```csharp demo-below
public class AlertButtonSetsDemo : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = UseAlert();
        var client = UseService<IClientProvider>();

        return Layout.Horizontal(
            new Button("Ok Only", _ => 
                showAlert("This is an info message", _ => {}, "Information", AlertButtonSet.Ok)
            ),
            new Button("Ok/Cancel", _ => 
                showAlert("Do you want to save changes?", result => {
                    client.Toast($"Result: {result}");
                }, "Confirm Save", AlertButtonSet.OkCancel)
            ),
            new Button("Yes/No", _ => 
                showAlert("Do you like Ivy?", result => {
                    client.Toast($"Answer: {result}");
                }, "Quick Poll", AlertButtonSet.YesNo)
            ),
            new Button("Yes/No/Cancel", _ => 
                showAlert("Save changes before closing?", result => {
                    client.Toast($"Choice: {result}");
                }, "Unsaved Changes", AlertButtonSet.YesNoCancel)
            ),
            alertView
        );
    }
}
```

## Toast Notifications

Toast notifications are lightweight, non-blocking messages that appear temporarily and then disappear automatically. They're perfect for providing quick feedback about user actions.

### Basic Toast Notifications

```csharp demo-below
public class BasicToastDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.Horizontal(
            new Button("Success Toast", _ => 
                client.Toast("Operation completed successfully!", "Success")
            ),
            new Button("Info Toast", _ => 
                client.Toast("Here's some helpful information", "Info")
            ),
            new Button("Simple Toast", _ => 
                client.Toast("Just a simple message")
            )
        );
    }
}
```

### Toast with Exception Handling

```csharp demo-below
public class ToastExceptionDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.Horizontal(
            new Button("Simulate Error", _ => {
                try {
                    throw new InvalidOperationException("Something went wrong!");
                } catch (Exception ex) {
                    client.Toast(ex); // Automatically formats exception
                }
            }),
            new Button("Custom Error Toast", _ => 
                client.Toast("Custom error message", "Error")
            )
        );
    }
}
```

## Examples

### Form Submission with Feedback

```csharp demo-below
public class FormSubmissionDemo : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = UseAlert();
        var client = UseService<IClientProvider>();
        var isSubmitting = UseState(false);

        return Layout.Vertical(
            new Button(
                isSubmitting.Value ? "Submitting..." : "Submit Form", 
                _ => {
                    showAlert("Are you ready to submit this form?", async result => {
                        if (result == AlertResult.Ok) {
                            isSubmitting.Set(true);
                            
                            // Simulate API call
                            await Task.Delay(2000);
                            
                            isSubmitting.Set(false);
                            client.Toast("Form submitted successfully!", "Success");
                        }
                    }, "Confirm Submission", AlertButtonSet.OkCancel);
                }
            ).Disabled(isSubmitting.Value),
            alertView
        );
    }
}
```
