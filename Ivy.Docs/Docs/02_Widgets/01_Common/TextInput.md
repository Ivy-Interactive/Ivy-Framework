---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# TextInput

The TextInput widget is a versatile input component that allows users to enter and edit text. It supports various input types and states, making it suitable for a wide range of form scenarios.

## Basic Usage

Here's a simple example of a text input with a placeholder that shows a toast message when the value changes:

```csharp demo-below
var state = this.UseState("Hello World");
Layout.Vertical()
    | new TextInput<string>(state.Value, e => {
        state.Set(e.Value);
        client.Toast($"Value changed to: {e.Value}");
    }, "Enter your name")
```

## Variants

TextInput supports several variants to handle different types of text input:

```csharp demo-below
Layout.Vertical()
    | new TextInput<string>("John Doe", placeholder: "Text Input")
    | new TextInput<string>("Lorem ipsum dolor sit amet...", placeholder: "Textarea Input").Variant(TextInputs.Textarea)
    | new TextInput<string>("john@example.com", placeholder: "Email Input").Variant(TextInputs.Email)
    | new TextInput<string>("+1 (555) 123-4567", placeholder: "Tel Input").Variant(TextInputs.Tel)
    | new TextInput<string>("https://example.com", placeholder: "URL Input").Variant(TextInputs.Url)
    | new TextInput<string>("secret123", placeholder: "Password Input").Variant(TextInputs.Password)
    | new TextInput<string>("search query", placeholder: "Search Input").Variant(TextInputs.Search)
```

## States

TextInput can be in different states to indicate its current condition:

```csharp demo-below
Layout.Vertical()
    | new TextInput<string>("Disabled value", placeholder: "Disabled Input").Disabled()
    | new TextInput<string>("Invalid value", placeholder: "Invalid Input").Invalid("This field is required")
```

## Data Binding

TextInput supports two-way data binding through state:

```csharp demo-below
var state = this.UseState("Initial value");
Layout.Vertical()
    | state.ToTextInput()
    | Text.Block($"Current value: {state.Value}")
```

## Events

TextInput provides several event handlers for different user interactions:

```csharp demo-below
var state = this.UseState("Type something");
Layout.Vertical()
    | new TextInput<string>(state.Value, 
        e => {
            state.Set(e.Value);
            client.Toast($"Value changed to: {e.Value}");
        }, 
        "Type to trigger onChange")
    | new TextInput<string>("Click me then click outside", 
        placeholder: "Click outside to trigger onBlur")
        .HandleBlur(_ => client.Toast("Input lost focus"))
```

## Helper Methods

TextInput provides convenient extension methods for different input types:

```csharp demo-below
var state = this.UseState("Sample text");
Layout.Vertical()
    | state.ToTextInput("Regular text input")
    | state.ToTextAreaInput("Textarea input")
    | state.ToPasswordInput("Password input")
    | state.ToSearchInput("Search input").ShortcutKey("Ctrl+K")
    | state.ToEmailInput("Email input")
    | state.ToUrlInput("URL input")
    | state.ToTelInput("Telephone input")
```

## Styling

TextInput can be customized with various styling options:

```csharp demo
new TextInput<string>("", placeholder: "Custom styled input")
    .ShortcutKey("Ctrl+K")
    .Invalid("Custom error message")
```

<WidgetDocs Type="Ivy.TextInput" ExtensionTypes="Ivy.TextInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/TextInput.cs"/> 