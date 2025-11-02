---
searchHints:
  - label
  - wrapper
  - form-field
  - input
  - description
  - help
---

# Field

<Ingress>
Group any input with a label, description, help text, and required indicator for a consistent, accessible form design.
</Ingress>

The `Field` widget acts as a **wrapper** around any input (such as `TextInput`, `Select`, `DateTime`, etc.).  
It provides a standardized way to display a label, optional description, help text tooltips, and visual cues like a required asterisk.  

This makes forms easier to build and ensures inputs remain consistent in layout and accessibility.

## Basic Usage

Here's how to wrap a `TextInput` in a `Field`:

```csharp demo-tabs
public class BasicFieldDemo : ViewBase
{
    public override object? Build()
    {
        var name = UseState("");
        return new Field(
            new TextInput(name)
                .Placeholder("Enter your name")
        )
        .Label("Name")
        .Description("Your full name as it appears on official documents")
        .Required();
    }
}
```

<Callout Type="info">
`Field` does not provide inputs by itself - it always wraps an input widget like `TextInput`, `Select`, or `Checkbox`.
</Callout>

## Properties

A `Field` supports the following common properties:

* **Label(string)** - The display label above the input.
* **Description(string)** - An optional helper text shown below the input.
* **Help(string)** - An optional help text displayed as a tooltip on an info icon next to the label.
* **Required(bool)** - Marks the input as required (adds an asterisk or style cue).

### Properties Usage

```csharp demo-below
public class FieldExample : ViewBase
{
    public override object? Build()
    {
        var username = UseState("");
        return username.ToTextInput()
            .Placeholder("Enter username")
            .WithField()
            .Label("Username")
            .Description("Must be at least 8 characters long")
            .Help("Your username must be unique and contain only letters, numbers, and underscores")
            .Required();
    }
}
```

### Wrapping Other Inputs

Since `Field` works generically, it can wrap **any widget**:

```csharp demo-below
public class MixedInputsDemo : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Today);

        var accepted = UseState(false);
        var options = new List<string>() { "I read the terms and conditions and I agree"};
        var selectedNotice = UseState(new string[]{});
        return Layout.Vertical()
            | dateState.ToDateTimeInput()
                .Variant(DateTimeInputs.Date)
                .WithField()
                .Label("Date of birth")
            | selectedNotice.ToSelectInput(options.ToOptions())
                .Variant(SelectInputs.List)
                .WithField()
                .Label("Terms & Conditions")
                .Description("You must agree before continuing")
                .Required();
    }
}
```

<Callout Tip="Info">
Use `Field` whenever you want **consistent form layout** across your application with labels, description and required asterisk.
</Callout>

<WidgetDocs Type="Ivy.Field" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/Field.cs"/>
