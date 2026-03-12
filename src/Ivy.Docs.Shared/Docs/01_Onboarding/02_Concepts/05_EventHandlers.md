---
searchHints:
  - onblur
  - events
  - focus
  - blur
  - event-handlers
  - input-events
  - form-events
---

# Event Handlers

<Ingress>
Handle user interactions and input events in Ivy with event handlers like OnBlur, enabling [validation](./13_Forms.md), [data persistence](./19_Clients.md), and reactive user experiences.
</Ingress>

Event handlers allow you to respond to user interactions with [widgets](./03_Widgets.md) in your Ivy [applications](./15_Apps.md). They enable you to execute custom logic when users interact with UI elements, such as clicking buttons, changing input values, or moving focus between fields.

## Basic Usage

The simplest form of `OnBlur` handler performs an action when the input loses focus:

```csharp demo-tabs
public class BasicBlurExample : ViewBase
{
    public override object? Build()
    {
        var name = UseState("");
        var message = UseState("");
        
        return Layout.Vertical()
            | name.ToTextInput("Your Name")
                .Placeholder("Enter your name...")
                .OnBlur(_ => message.Set($"Hello, {name.Value}!"))
            | Text.P(message.Value);
    }
}
```

## OnBlur Event Handler

The `OnBlur` event handler is triggered when an input widget loses focus. This is particularly useful for validation, auto-saving data, analytics tracking, or performing cleanup operations when a user finishes interacting with a field.

### When OnBlur Fires

```mermaid
graph LR
    A[User types in field] --> B[User clicks away]
    B --> C[OnBlur fires]
```

### Available on Input Widgets

The `OnBlur` event handler is available on all input widgets that implement the `IAnyInput` interface:

| Input Widget | Description |
|--------------|-------------|
| [TextInput](../../02_Widgets/04_Inputs/02_TextInput.md) | Text, password, email, search, and textarea inputs |
| [NumberInput](../../02_Widgets/04_Inputs/03_NumberInput.md) | Number and slider inputs |
| [SelectInput](../../02_Widgets/04_Inputs/05_SelectInput.md) | Dropdown select inputs |
| [AsyncSelectInput](../../02_Widgets/04_Inputs/06_AsyncSelectInput.md) | Async dropdown inputs with server-side data |
| [BoolInput](../../02_Widgets/04_Inputs/04_BoolInput.md) | Checkbox and switch inputs |
| [DateTimeInput](../../02_Widgets/04_Inputs/07_DateTimeInput.md) | Date and time picker inputs |
| [DateRangeInput](../../02_Widgets/04_Inputs/08_DateRangeInput.md) | Date range picker inputs |
| [FileInput](../../02_Widgets/04_Inputs/10_FileInput.md) | File upload inputs |
| [ColorInput](../../02_Widgets/04_Inputs/09_ColorInput.md) | Color picker inputs |
| [CodeInput](../../02_Widgets/04_Inputs/11_CodeInput.md) | Code editor inputs |
| [FeedbackInput](../../02_Widgets/04_Inputs/13_FeedbackInput.md) | Star rating and feedback inputs |
| [ReadOnlyInput](../../02_Widgets/04_Inputs/14_ReadOnlyInput.md) | Read-only display inputs |

## Common Patterns

### Validation Pattern

Validate [fields](../../02_Widgets/04_Inputs/01_Field.md) when the user finishes editing using validation patterns:

```csharp demo-tabs
public class ValidationBlurExample : ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        var error = UseState(() => (string?)null);
        
        return email.ToTextInput()
            .Placeholder("your.email@example.com")
            .OnBlur(() => 
            {
                error.Set(string.IsNullOrWhiteSpace(email.Value) ? "Required" 
                    : !email.Value.Contains("@") ? "Invalid email" 
                    : null);
            })
            .Invalid(error.Value);
    }
}
```

### Auto-Save & Formatting Pattern

Perform actions like saving or formatting when focus is lost:

```csharp demo-tabs
public class AutoSaveFormatExample : ViewBase
{
    public override object? Build()
    {
        var phoneNumber = UseState("");
        var title = UseState("");
        var lastSaved = UseState(() => (DateTime?)null);
        var client = UseService<IClientProvider>();
        
        return Layout.Vertical()
            // Auto-save pattern
            | title.ToTextInput()
                .Placeholder("Document title")
                .OnBlur(async () => 
                {
                    await Task.Delay(500); // Save to database
                    lastSaved.Set(DateTime.Now);
                    client.Toast("Saved!");
                })
            | Text.Muted(lastSaved.Value != null ? $"Saved at {lastSaved.Value:HH:mm:ss}" : "")
            
            // Format pattern
            | phoneNumber.ToTextInput()
                .Placeholder("Enter 10-digit phone")
                .OnBlur(() => 
                {
                    var digits = new string(phoneNumber.Value.Where(char.IsDigit).ToArray());
                    if (digits.Length == 10)
                        phoneNumber.Set($"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}");
                });
    }
}
```

### Async Operations Pattern

Handle async operations like API validation:

```csharp demo-tabs
public class AsyncBlurExample : ViewBase
{
    public override object? Build()
    {
        var username = UseState("");
        var message = UseState("");
        
        return Layout.Vertical()
            | username.ToTextInput()
                .Placeholder("Choose username")
                .OnBlur(async () =>
                {
                    if (string.IsNullOrWhiteSpace(username.Value)) return;
                    
                    message.Set("Checking...");
                    await Task.Delay(1000); // API call
                    
                    var isAvailable = !username.Value.Equals("admin", StringComparison.OrdinalIgnoreCase);
                    message.Set(isAvailable ? "Available" : "Taken");
                })
            | Text.P(message.Value);
    }
}
```

## OnBlur Method Signatures

```csharp
// Simple action (most common)
input.OnBlur(() => Validate());

// With event parameter
input.OnBlur((Event<IAnyInput> e) => Log(e.Id));

// Async operation
input.OnBlur(async () => await SaveToApi());
```

## See Also

- [Forms](./13_Forms.md) - Building forms with validation
- [State](../../03_Hooks/02_Core/03_UseState.md) - Managing component state
- [Effects](../../03_Hooks/02_Core/04_UseEffect.md) - Performing side effects
- [Widgets](./03_Widgets.md) - Understanding Ivy widgets
