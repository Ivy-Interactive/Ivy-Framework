---
searchHints:
  - bool
  - checkbox
  - switch
  - toggle
  - boolean
  - true-false
  - option
---

# BoolInput

<Ingress>
Handle boolean input with elegant checkboxes, switches, and toggles for true/false values in [forms](../../01_Onboarding/02_Concepts/08_Forms.md) and [interfaces](../../01_Onboarding/02_Concepts/02_Views.md).
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
        return state.ToBoolInput().Label("Accept Terms");
    }
}
```

### Creating BoolInput Instances

You create `BoolInput` instances using extension methods on [state](../../03_Hooks/02_Core/03_UseState.md):

```csharp
var state = UseState(false);
var input = state.ToBoolInput(); // Creates BoolInput from state

var nullableState = UseState<bool?>(null);
var nullableInput = nullableState.ToBoolInput(); // For nullable boolean

var intState = UseState(0);
var intInput = intState.ToBoolInput(); // For integer-based boolean (0/1)
```


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
                .Variant(BoolInputVariant.Checkbox)
                .Label("Agree to terms and conditions")
            | (agreed.Value ? Text.Monospaced("You are all set!") : null);
    }
}
```

### Switch

To make the bool input appear like a switch, this variant should be used. This is most suitable for toggling
some settings values on and off.  

```csharp demo-below
public class BoolInputSwitchDemo : ViewBase
{
    public override object? Build()
    {
        var read = UseState(false);
        var readMessage  = UseState("");
        var write = UseState(false);
        var delete = UseState(false);
        var dark =  UseState(false);
        
        return Layout.Vertical()
               | (Layout.Horizontal()
                 | read.ToSwitchInput(Icons.Eye).Label("Readonly")
                 | Text.Block(readMessage))
               | write.ToSwitchInput(Icons.Pencil)
                   .Label("Can write")
                   .Disabled(read.Value)
               | delete.ToSwitchInput(Icons.Trash)
                   .Label("Can delete")
                   .Disabled(read.Value)
               | dark.ToSwitchInput(Icons.Moon)
                   .Label("Dark Mode");
    }
}
```

The `ToSwitchInput` extension method also supports an optional `icon` parameter, allowing you to display an icon inside the switch thumb.

### Toggle

`Toggle` is a button-style boolean input that switches between two states (on/off, enabled/disabled) with a single click.
It appears as a pressable [button](../03_Common/01_Button.md) that visually indicates its current state through styling and optional icons.
This is represented by `BoolInputVariant.Toggle`

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

## Styling and States

Customize the `BoolInput` with various styling options:

```csharp demo-tabs
public class BoolInputStylingDemo : ViewBase
{
    public override object? Build()
    {
        var normalBool = UseState(true);
        var invalidBool = UseState(false);
        var loadingBool = UseState(true);
        var disabledBool = UseState(false);

        var isLoading = UseState(true);

        return Layout.Vertical()
            | normalBool.ToSwitchInput()
                .Label("Normal BoolInput")

            | invalidBool.ToSwitchInput()
                .Label("Invalid BoolInput")
                .Invalid("This field has an error")

            | loadingBool.ToSwitchInput()
                .Label("Loading BoolInput")
                .Loading(isLoading.Value)

            | disabledBool.ToSwitchInput()
                .Label("Disabled BoolInput")
                .Disabled(true);
    }
}
```

<Callout variant="info" title="Legacy Integer Support">
BoolInput also supports integer-based boolean values (0 = false, 1 = true) for compatibility with legacy systems. Simply use `UseState(0)` or `UseState(1)` with standard extension methods like `ToBoolInput()`, `ToSwitchInput()`, or `ToToggleInput()`.
</Callout>

<WidgetDocs Type="Ivy.BoolInput" ExtensionTypes="Ivy.BoolInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/BoolInput.cs"/>

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
                              .Variant(DateTimeInputVariant.Date)
                              .Placeholder("Select departure date")
                              .WithField()
                              .Label("Departure Date:")
                // Return Date (only visible when round trip is on)
                | returnDate.ToDateTimeInput()
                           .Variant(DateTimeInputVariant.Date)
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
