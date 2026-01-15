---
searchHints:
  - checkbox
  - switch
  - toggle
  - boolean
  - true-false
  - option
---

# BoolInput

<Ingress>
Handle boolean input with elegant checkboxes, switches, and toggles for true/false values in [forms](../../01_Onboarding/02_Concepts/13_Forms.md) and [interfaces](../../01_Onboarding/02_Concepts/02_Views.md).
</Ingress>

The `BoolInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a checkbox, switch and toggle for boolean (true/false) input values. It allows users to easily switch between two states in a form or configuration interface.

## Basic Usage

Here's a simple example of a `BoolInput` used as a checkbox:

```csharp demo-below
public class BoolInputDemo : ViewBase
{
    public override object? Build()
    {
        var state = UseState(false);
        return new BoolInput(state).Label("Accept Terms");
    }
}
```

### Creating BoolInput Instances

You can create `BoolInput` instances in several ways:

**Using the non-generic constructor (defaults to `bool` type):**

```csharp
var input = new BoolInput(); // Creates BoolInput<bool> with default values
var labeledInput = new BoolInput("My Label"); // With custom label
```

**Using the generic constructor for specific types:**

```csharp
var nullableInput = new BoolInput<bool?>(); // For nullable boolean
var intInput = new BoolInput<int>(); // For integer-based boolean (0/1)
```

**Using extension methods from [state](../../03_Hooks/Core/03_UseState.md):**

```csharp
var state = UseState(false);
var input = state.ToBoolInput(); // Creates BoolInput from state
```

The non-generic `BoolInput` constructor is the most convenient when you need a simple boolean input without nullable types or other boolean-like representations.


<Callout variant="info" title="Extension Methods">
Use extension methods `ToBoolInput()`, `ToSwitchInput()`, and `ToToggleInput()` to quickly create BoolInput from state. See examples in the Variants section below.
</Callout>

## Nullable Bool Inputs

Null values are supported for boolean values. The following example shows it in action.
These values are useful in situations where boolean values can be either not set (`null`)
or set (`true` or `false`). These can be really handy to capture different answers from
questions in a survey.

```csharp demo-below
public class NullableBoolDemo: ViewBase
{
    public override object? Build()
    {
        var going = UseState((bool?)null);
        var status = UseState("");
        if(going.Value == null)
            status.Set("Not answered");
        else 
            status.Set(going.Value == true ? "Yes!" : "No, not yet!");
        return Layout.Vertical()
                | Text.Html($"<i>{status}</i>")
                | going.ToSwitchInput()
                       .WithField()
                       .Label("Have you booked return tickets?");        
    }    
}
```

## Variants

There are three variants of `BoolInput`s. The following blocks show how to create and use them.

### CheckBox

To make the bool input appear like a checkbox, this variant should be used.

```csharp demo-below
public class CheckBoxDemo : ViewBase
{
    public override object? Build()
    {
        var agreed = UseState(false);
        
        return Layout.Horizontal()
            | agreed.ToBoolInput()
                .Variant(BoolInputs.Checkbox)
                .Label("Agree to terms and conditions")
            | (agreed.Value ? Text.InlineCode("You are all set!") : null);
    }
}
```

### Switch

To make the bool input appear like a switch, this variant should be used. This is most suitable for toggling
some settings values on and off.  

```csharp demo-below
public class BoolInputDemo : ViewBase
{
    public override object? Build()
    {
        var read = UseState(false);
        var readMessage  = UseState("");
        var write = UseState(false);
        var delete = UseState(false);
        var dark =  UseState(false);
        var roundTrip = UseState(false);
        
        return Layout.Vertical()
               | (Layout.Horizontal()
                 | new BoolInput(read.Value, e => 
                 {
                    if(e.Value)
                       readMessage.Set("User has readonly access!");
                    else
                       readMessage.Set("");
                    read.Set(e.Value);
                 }, variant: BoolInputs.Switch).Label("Readonly")
                 | Text.Block(readMessage))
              | new BoolInput(write, variant: BoolInputs.Switch)
                   .Label("Can write")
                   .Disabled(read.Value)
              | new BoolInput(delete, variant: BoolInputs.Switch).Label("Can delete")
                   .Disabled(read.Value)
              | new BoolInput(dark, variant: BoolInputs.Switch)
                   .Label("Round trip");
    }
}
```

### Toggle

`Toggle` is a button-style boolean input that switches between two states (on/off, enabled/disabled) with a single click.
It appears as a pressable [button](../03_Common/01_Button.md) that visually indicates its current state through styling and optional icons.
This is represented by `BoolInputs.Toggle`

`ToToggleInput` extension function can be used to create such a `BoolInput.Toggle` variant.
The following is a small demo showing how such a control may be used.

```csharp demo-below
public class SingleToggleDemo : ViewBase 
{
    public override object? Build()
    {        
        var isFavorite = UseState(false);        
        return Layout.Vertical()            
                | (Layout.Horizontal()
                    |  isFavorite.ToToggleInput(isFavorite.Value ? Icons.Heart : Icons.HeartOff)
                                 .Label(isFavorite.Value ? "Remove from Favorites" : "Add to Favorites")
                    | Text.Block(isFavorite.Value ? "❤️ Favorited!" : "🤍 Not favourite!"))            
                | Text.P(isFavorite.Value 
                    ? "This article has been added to your favorites." 
                    : "Click the heart to save this article.").Small();
    }
}
```


<Callout variant="info" title="Legacy Integer Support">
BoolInput also supports integer-based boolean values (0 = false, 1 = true) for compatibility with legacy systems. Simply use `UseState(0)` or `UseState(1)` with standard extension methods like `ToBoolInput()`, `ToSwitchInput()`, or `ToToggleInput()`.
</Callout>

<WidgetDocs Type="Ivy.BoolInput" ExtensionTypes="Ivy.BoolInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/BoolInput.cs"/>

## Examples

<Details>
<Summary>
Round trip example
</Summary>
<Body>
The following example shows a demo of how `Switch` variant can be used in a possible situation where it makes sense
to do so.

```csharp demo-tabs
public class SimpleFlightBooking : ViewBase
{
    public override object? Build()
    {        
        var isRoundTrip = UseState(false);
        var departureDate = UseState(DateTime.Today.AddDays(1));
        var returnDate = UseState(DateTime.Today.AddDays(7));

        return Layout.Vertical()
                | Text.P("Book Flight")
                // Round Trip Switch
                | isRoundTrip.ToSwitchInput().Label("Round Trip")
                // Departure Date (always visible)
                | departureDate.ToDateTimeInput()
                              .Variant(DateTimeInputs.Date)
                              .Placeholder("Select departure date")
                              .WithField()
                              .Label("Departure Date:")
                // Return Date (only visible when round trip is on)
                | returnDate.ToDateTimeInput()
                           .Variant(DateTimeInputs.Date)
                           .Placeholder("Select return date")
                           .Disabled(!isRoundTrip.Value)
                           .WithField()
                           .Label("Return Date:")
                // Summary
                | Text.P($"Round trip: {departureDate.Value:MMM dd} → {returnDate.Value:MMM dd}").Small()
                | Text.P($"One way: {departureDate.Value:MMM dd}").Small();
    }
}
```

</Body>
</Details>
