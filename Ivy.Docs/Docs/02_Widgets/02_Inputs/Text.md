---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TextInput

The TextInput widget provides a standard text entry field. It supports various text input types including single-line text, multi-line text, password fields, email, phone number, URL and offers features like placeholder text, validation, and text formatting. The following text describes this widget.

## Basic Usage

Here's a simple example of a text input with a placeholder:

```csharp
var withoutValue = UseState((string?)null);
new TextInput<string>(withoutValue, placeholder: "Enter text here...");
```

```csharp demo
new TextInput<string>(withoutValue, placeholder: "Enter text here...")
```

## Variants

TextInputs come in several variants to suit different use cases:

```csharp demo-tabs
Layout.Horizontal()
    | new TextInput<string>(withoutValue, placeholder: "Text")
    | new TextInput<string>(withoutValue, placeholder: "Password").Variant(TextInputs.Password)
    | new TextInput<string>(withoutValue, placeholder: "Textarea").Variant(TextInputs.Textarea)
    | new TextInput<string>(withoutValue, placeholder: "Search").Variant(TextInputs.Search)
    | new TextInput<string>(withoutValue, placeholder: "jdoe@somewhere.com").Variant(TextInputs.Email)
    | new TextInput<string>(withoutValue, placeholder: "+1-234-123345").Variant(TextInputs.Tel)
    | new TextInput<string>(withoutValue, placeholder: "https://ivy.app/").Variant(TextInputs.Url)
```

## Event Handling

TextInputs can handle `change` and `blur` events: 


```csharp
var onChangedState = UseState("");
new TextInput<string>(onChangedState.Value, e => onChangedState.Set(e.Value));
```

The following code demonstrates blurring of the input 

```csharp
var blurCount = UseState(0);
new TextInput<string>(state, placeholder: "Type and click away")
    .OnBlur(_ => blurCount.Set(blurCount.Value + 1))
```

## Styling

TextInputs can be customized with various styling options:

```csharp
new TextInput<string>(withoutValue, placeholder: "Styled Input")
    .Disabled()
    .Invalid("Invalid input")
```

Here are some more examples of how we can use different styling options. 

```csharp demo-tabs
Layout.Horizontal()
    | new TextInput<string>(UseState(""), placeholder: "Normal Input")
    | new TextInput<string>(UseState(""), placeholder: "Disabled Input").Disabled()
    | new TextInput<string>(UseState("invalid@"), placeholder: "Email").Invalid("Invalid email address")
```


# Shortcuts
We can use associate keyboard shortcuts to text inputs the following way. 

```csharp demo

var name = UseState("");
var email = UseState("");
var message = UseState("");

Layout.Vertical()
    | new Text("Keyboard Shortcuts Demo")
    | new Text("Ctrl+N - Focus Name, Ctrl+E - Focus Email, Ctrl+M - Focus Message")
    
    | new TextInput<string>(name, placeholder: "Name (Ctrl+N)")
        .ShortcutKey("Ctrl+N")
    
    | new TextInput<string>(email, placeholder: "Email (Ctrl+E)")
        .ShortcutKey("Ctrl+E")
        .Variant(TextInputs.Email)
    
    | new TextInput<string>(message, placeholder: "Message (Ctrl+M)")
        .ShortcutKey("Ctrl+M")
        .Variant(TextInputs.Textarea)

```

# Dynamic Form Generation
We can generate forms based on the inputs dynamically coming from another part of the program or elsewhere. 
The following code demonstrates this. 

```csharp demo-below
var formFields = new[]
{
    new { Name = "email", Type = TextInputs.Email, State = UseState("") },
    new { Name = "phone", Type = TextInputs.Tel, State = UseState("") },
    new { Name = "website", Type = TextInputs.Url, State = UseState("") }
};

Layout.Vertical()
    | formFields.Select(field => field.State.ToTextInput(field.Name, variant: field.Type))
```

# TextInput Extension Methods

| Method Name | Purpose | Use Case | Example |
|-------------|---------|----------|---------|
| `ToTextAreaInput()` | Creates a multi-line text input | Long text content, comments, descriptions | `bioState.ToTextAreaInput("Tell us about yourself...")` |
| `ToSearchInput()` | Creates a search-optimized input | Search bars, filters, lookup fields | `searchState.ToSearchInput("Search products...")` |
| `ToPasswordInput()` | Creates a password input with hidden text | Login forms, registration, security | `passwordState.ToPasswordInput("Enter password")` |
| `ToEmailInput()` | Creates an email input with validation | Contact forms, user registration | `emailState.ToEmailInput("Enter your email")` |
| `ToUrlInput()` | Creates a URL input with validation | Website links, social profiles | `websiteState.ToUrlInput("Enter website URL")` |
| `ToTelInput()` | Creates a telephone number input | Contact information, phone verification | `phoneState.ToTelInput("Phone number")` |

## Parameters

All methods accept the same optional parameters:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `state` | `IAnyState` | Required | The state object to bind the input to |
| `placeholder` | `string?` | `null` | Text shown when input is empty |
| `disabled` | `bool` | `false` | Whether the input is disabled |

## Usage Example

```csharp
// Dynamic form generation using extension methods
var formData = new Dictionary<string, IAnyState>
{
    ["email"] = UseState(""),
    ["phone"] = UseState(""),
    ["website"] = UseState(""),
    ["password"] = UseState(""),
    ["bio"] = UseState(""),
    ["search"] = UseState("")
};

Layout.Vertical()
    | formData["email"].ToEmailInput("Email Address")
    | formData["phone"].ToTelInput("Phone Number") 
    | formData["website"].ToUrlInput("Website")
    | formData["password"].ToPasswordInput("Password")
    | formData["bio"].ToTextAreaInput("Bio")
    | formData["search"].ToSearchInput("Search...")
```

<WidgetDocs Type="Ivy.TextInput" ExtensionTypes="Ivy.TextInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/TextInput.cs"/>
