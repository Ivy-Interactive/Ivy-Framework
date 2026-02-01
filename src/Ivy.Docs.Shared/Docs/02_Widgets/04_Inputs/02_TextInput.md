---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - input
  - textbox
  - password
  - textarea
  - email
  - search
---


# TextInput

<Ingress>
Capture user text input with validation, formatting, and interactive features like autocomplete and real-time feedback.
</Ingress>

The `TextInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a standard text entry field. It supports various text [input](../../01_Onboarding/02_Concepts/03_Widgets.md) types including single-line text, multi-line text, password fields, email, phone number, URL and offers features like placeholder text, validation, shortcut keys and text formatting.

## Basic Usage

Here's a simple example of a text input with a placeholder:

```csharp demo-tabs
public class BasicUsageDemo : ViewBase
{
    public override object? Build()
    { 
        var text = UseState("");
        return new TextInput(text).Placeholder("Enter text here...");
    }
}
```

<Callout Type="tip">
The TextInput class now defaults to `string` type, so you can use `new TextInput(...)` instead of `new TextInput&lt;string&gt;(...)`. The generic version is still available if you need to work with other string-compatible types.
</Callout>

## Variants

`TextInput`s come in several variants to suit different use cases:
The following blocks shows how to use these.

### Password

For capturing passwords, `TextInputs.Password` variant needs to be used. The following code shows how to capture
a new password.

See it in action here.

```csharp demo-below
public class PasswordCaptureDemo: ViewBase
{
    public override object? Build()
    {
        var password = UseState("");
        return new TextInput(password)
                     .Placeholder("Password")
                     .Variant(TextInputs.Password)
                     .WithField()
                     .Label("Enter Password");         
    }
}
```

### TextArea

When a multiline text is needed, `TextInputs.Textarea` variant should be used. A common use-case is for capturing address
that typically spans over multiple lines. The following demo shows how to use it.

See it in action here.

```csharp demo-below
public class CaptureAddressDemo: ViewBase
{
    public override object? Build()
    {
        var address = UseState("");
        return new TextInput(address)
                               .Placeholder("Åkervägen 9, \n132 39 Saltsjö-Boo, \nSweden")
                               .Variant(TextInputs.Textarea)
                               .Height(30)
                               .Width(100)
                               .WithField()
                               .Label("Address");         
    }
}
```

Please note that how the newlines (`\n`) are recognized and used to create newlines in the textarea.

### Search

When it is necessary to find an element from a collection of items, it is better to give users a visual clue.  
Using the `TextInputs.Search` variant, this visual clue (with a looking glass icon) becomes obvious.
The following demo shows how to add such an text input.

See it in action here.

```csharp demo-below
public class SearchBarDemo: ViewBase
{
    public override object? Build()
    {
        var searchThis = UseState("");
        return new TextInput(searchThis)
                               .Placeholder("search for?")
                               .Variant(TextInputs.Search)
                               .WithField()
                               .Label("Search");
    }
}
```

### Email

To capture the emails `TextInputs.Email` variant should be used.  

See it in action here.

```csharp demo-below
public class EmailEnterDemo: ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        return new TextInput(email)
                       .Placeholder("user@domain.com")
                       .Variant(TextInputs.Email)
                       .WithField()
                       .Label("Email");
    }
}
```

### Telephone

To capture the phone numbers `TextInputs.Tel` variant needs to be used.  

see it in action here.

```csharp demo-below
public class PhoneEnterDemo: ViewBase
{
    public override object? Build()
    {
        var tel = UseState("");
        return new TextInput(tel)
                      .Placeholder("+1-123-3456")
                      .Variant(TextInputs.Tel)
                      .WithField()
                      .Label("Phone");
    }
}
```

### URL

To capture the URLs/Links  `TextInputs.Url` variant needs to be used.

see it in action here.

```csharp demo-below
public class URLEnterDemo: ViewBase
{
    public override object? Build()
    {
        var url = UseState("");
        return new TextInput(url)
                      .Placeholder("https://ivy.app/")
                      .Variant(TextInputs.Url)
                      .WithField()
                      .Label("Website");
    }
}
```

## Event Handling

Use the [`OnChange`](../../01_Onboarding/02_Concepts/05_EventHandlers.md) callback to react to text input changes. The callback receives an event with the current value.

```csharp demo-tabs
public class EventsDemoApp : ViewBase
{
    public override object? Build()
    {
        var name = UseState("");
        return Layout.Vertical()
            | new TextInput(name.Value, e => name.Set(e.Value))
                  .Placeholder("Enter your name...").WithField().Label("Name")
            | (name.Value.Length > 0 ? $"Hello, {name.Value}!" : "");
    }
}
```

## Styling

`TextInput` can be styled to provide visual feedback to users about the input state. Use `.Invalid()` for [validation](../../01_Onboarding/02_Concepts/08_Forms.md) error messages.

```csharp demo-below
public class TextInputStylingDemo : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        return Layout.Vertical()
            | new TextInput(text).Placeholder("Invalid input").Invalid("This field has an error")
            | new TextInput(text).Placeholder("Disabled input").Disabled();
    }
}
```

Hover over the `i` icon on the invalid input to see the error message.

## Prefix and Suffix

In certain scenarios, it is beneficial to prepend or append static content—such as text fragments or icons—to an input field. This practice is particularly useful for displaying the protocol in a URL field, a currency symbol, or an icon that denotes the expected input.

```csharp demo-below
public class UrlInputDemo : ViewBase
{
    public override object? Build()
    {
        var domain = UseState("example");
        return domain.ToTextInput()
                     .Prefix(Icons.Globe)
                     .Suffix(".com");
    }
}
```

The `Prefix` and `Suffix` methods accept either a `string` or an `IWidget`, thereby providing complete flexibility for augmenting the contextual information of the input.

## Shortcuts

We can use associate keyboard shortcuts to text inputs the following way.

```csharp
 new TextInput(name)
    .Placeholder("Name (Ctrl+S)")
    .ShortcutKey("Ctrl+S")   
```

The following demo shows this in action with multiple `TextInput`s each
with different shortcut keys.

```csharp demo-below
public class ShortCutDemo : ViewBase
{
    public override object? Build()
    { 
        var name = UseState("");
        var email = UseState("");
        var message = UseState("");
        return Layout.Vertical()
                | Text.Inline("Keyboard Shortcuts Demo")
                | Text.Inline("Ctrl+J - Focus Name, Ctrl+E - Focus Email, Ctrl+M - Focus Message")  
                | new TextInput(name)
                      .Placeholder("Name (Ctrl+J)")
                      .ShortcutKey("Ctrl+J")    
                | new TextInput(email)
                      .Placeholder("Email (Ctrl+E)")
                      .ShortcutKey("Ctrl+E")
                      .Variant(TextInputs.Email)    
                | new TextInput(message)
                      .Placeholder("Message (Ctrl+M)")
                      .ShortcutKey("Ctrl+M")
                      .Variant(TextInputs.Textarea);
    }
}
```

## Helper functions

There are several helper functions to create `TextInput` variants from state instances. Instead of employing the constructor to create a
`TextInput`, these functions should be used. The following is an example of how Ivy can be employed to generate [UI](../../01_Onboarding/02_Concepts/02_Views.md) idiomatically using
these functions.

```csharp demo-below
public class DataCaptureUsingExtensionDemo: ViewBase
{
    public override object? Build()
    {
        var userName = UseState("");
        var password = UseState("");
        var email = UseState("");
        var tel = UseState("");
        var address = UseState("");
        var website = UseState("");
        return Layout.Vertical()
                | userName.ToTextInput()
                          .Placeholder("User name")
                          .WithField()
                          .Label("Username")
                | password.ToPasswordInput(placeholder: "Password")
                          .Disabled(userName.Value.Length == 0)
                          .WithField()
                          .Label("Password")
                | email.ToEmailInput()
                       .Placeholder("Email")
                       .WithField()
                       .Label("Email")
                | tel.ToTelInput()
                     .Placeholder("Mobile")
                     .WithField()
                     .Label("Mobile")
                | address.ToTextAreaInput()
                         .Placeholder("Address Line1\nAddress Line2\nAddress Line 3")
                         .Height(40)
                         .Width(100)
                         .WithField()
                         .Label("Address")
                | website.ToUrlInput()
                         .Placeholder("https://ivy.app/")
                         .WithField()
                         .Label("Website");                             
    }
}
```

There is also another extension function to create a `TextInput.Search` variant.

Here is how it can be used.

```csharp demo-below

public class BasicFilter : ViewBase 
{      
    public override object Build()
    {         
        var searchState = UseState("");
        var result = UseState("");
        var fruits = new[] { 
            "Apple", "Banana", "Cherry", "Date", "Elderberry", 
            "Stawberry", "Blueberry", "Watermelon", "Muskmelon",
            "Fig", "Grape", "Kiwi", "Lemon", "Mango" 
        };

        var filtered = fruits
            .Where(fruit => fruit.ToLower().Contains(searchState.Value.ToLower()))
            .ToArray();
            
        var content = string.Join("\n", filtered);
        
        return Layout.Vertical()
            | searchState.ToSearchInput().Placeholder("Which fruit you like?")
            | result.ToTextAreaInput(content);
    }     
}
```

<WidgetDocs Type="Ivy.TextInput" ExtensionTypes="Ivy.TextInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/TextInput.cs"/>

## Examples

<Details>
<Summary>
Validate Email Demo
</Summary>
<Body>

In this example, if the email format is wrong, the input is invalidated and a message is shown.

```csharp demo-tabs
public class EmailValidationDemo : ViewBase 
{      
    // Email regex pattern
    private static readonly System.Text.RegularExpressions.Regex EmailRegex = new 
        System.Text.RegularExpressions.Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        System.Text.RegularExpressions.RegexOptions.Compiled | 
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
    );

    public override object? Build()
    {         
        var onChangedState = UseState("");         
        var invalidState = UseState("");         
        
        return new TextInput(onChangedState.Value, e =>                    
              {                        
                onChangedState.Set(e.Value);
                if (string.IsNullOrWhiteSpace(e.Value))
                {
                    invalidState.Set("");
                }
                else if (!EmailRegex.IsMatch(e.Value))                        
                {                             
                    invalidState.Set("Invalid email address");
                }                        
                else                        
                {                         
                    invalidState.Set(""); 
                }                    
              })
              .Invalid(invalidState.Value)
              .WithField()
              .Label("Email");
         
    }     
}
```

</Body>
</Details>

<Details>
<Summary>
Conditional Enabling/Disabling of Text Inputs
</Summary>
<Body>
In this demo, password field is enabled only when the username field has a value.

```csharp demo-tabs

public class LoginForm : ViewBase 
{      
    public override object Build()
    {         
        var usernameState = UseState("");         
        var passwordState = UseState("");         
        
        return Layout.Vertical()
            | Text.Label("Login")
            | Layout.Vertical()
                | usernameState.ToTextInput()
                    .Placeholder("Enter your username")
                    .WithField()
                    .Label("Username")
                | passwordState.ToPasswordInput()
                    .Placeholder("Enter your password")
                     // Disabled when username is empty
                    .Disabled(string.IsNullOrWhiteSpace(usernameState.Value))
                    .WithField()
                    .Label("Password")
                | new Button("Login")
                    .Disabled(string.IsNullOrWhiteSpace(usernameState.Value) || 
                        string.IsNullOrWhiteSpace(passwordState.Value));                             
    }     
}
```

</Body>
</Details>
