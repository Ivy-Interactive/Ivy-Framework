# Alerts & Notifications

<Ingress>
Communicate with users effectively using modal dialog alerts for important confirmations and toast notifications for feedback messages.
</Ingress>

## Types of Alerts

Ivy provides two main types of alerts:

1. **Dialog Alerts** - Modal dialogs for important confirmations and decisions
2. **Toast Notifications** - Non-blocking notifications for feedback and status updates

## Dialog Alerts

Dialog alerts are modal windows that require user interaction. They're perfect for confirmations, important messages, or collecting user decisions.

### Basic Dialog Alert

```csharp demo-below 
public class BasicDialogAlertDemo : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = this.UseAlert();
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
        var (alertView, showAlert) = this.UseAlert();
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
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

        return Layout.Vertical(
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

        return Layout.Vertical(
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

## Real-World Examples

### Form Submission with Feedback

```csharp demo-below 
public class FormSubmissionDemo : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = this.UseAlert();
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

## Best Practices

### When to Use Dialog Alerts

- Confirming destructive actions (delete, reset, etc.)
- Important decisions that affect data
- Critical error messages that require acknowledgment
- Multi-step processes requiring user choice

### When to Use Toast Notifications

- Success confirmations after actions
- Non-critical error messages
- Status updates and progress notifications
- Quick feedback that doesn't require user action

### Writing Good Alert Messages

1. **Be Clear**: Use simple, direct language
2. **Be Specific**: Explain exactly what will happen
3. **Be Actionable**: Make it clear what the user needs to do
4. **Be Concise**: Keep messages as short as possible while staying informative

### Accessibility Considerations

- Alert dialogs automatically focus and trap keyboard navigation
- Toast messages are announced by screen readers
- Button labels should be descriptive (avoid just "OK" when possible)
- Important alerts should be modal to ensure they're not missed
