# BoolInput

<Ingress>
Handle boolean input with elegant checkboxes, switches, and toggles for true/false values in forms and interfaces.
</Ingress>

The `BoolInput` widget provides a checkbox, switch and toggle for boolean (true/false) input values. It allows users to easily switch between two states in a form or configuration interface.

## Basic Usage

Here's a simple example of a `BoolInput` used as a checkbox:

```csharp demo-below
public class BoolInputDemo : ViewBase
{
    public override object? Build()
    {
        var state = this.UseState(false);
        return Layout.Horizontal()
               | new BoolInput(state).Label("Accept Terms");
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

**Using extension methods from state:**

```csharp
var state = UseState(false);
var input = state.ToBoolInput(); // Creates BoolInput from state
```

The non-generic `BoolInput` constructor is the most convenient when you need a simple boolean input without nullable types or other boolean-like representations.

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
                | Text.Small("Have you booked return tickets?")
                | Text.Html($"<i>{status}</i>")
                | going.ToSwitchInput();        
    }    
}
```

## Variants

There are three variants of `BoolInput`s. The following blocks show how to create and use them.

### CheckBox

To make the bool input appear like a checkbox, this variant should be used.

```csharp demo-below
public class BoolInputDemo : ViewBase
{
    public override object? Build()
    {
        var state = UseState(false);
        var agreed = UseState(""); 
        return Layout.Horizontal()
               | new BoolInput(state.Value, e => 
                     {
                          if(e.Value)
                          {
                              agreed.Set("You are all set!");                
                          }
                          else
                          {
                              agreed.Set("");
                          }
                          state.Set(e.Value);
                     },variant: BoolInputs.Checkbox).Label("Agree to terms and conditions")
               | Text.InlineCode(agreed);
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
               | Layout.Horizontal()
                 | new BoolInput(read.Value, e => 
                 {
                    if(e.Value)
                       readMessage.Set("User has readonly access!");
                    else
                       readMessage.Set("");
                    read.Set(e.Value);
                 }, variant: BoolInputs.Switch).Label("Readonly")
                 | Text.Block(readMessage)
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
It appears as a pressable button that visually indicates its current state through styling and optional icons.
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
                | Layout.Horizontal()
                    |  isFavorite.ToToggleInput(isFavorite.Value ? Icons.Heart : Icons.HeartOff)
                                 .Label(isFavorite.Value ? "Remove from Favorites" : "Add to Favorites")
                    | Text.Block(isFavorite.Value ? "❤️ Favorited!" : "🤍 Not favourite!")            
                | Text.Small(isFavorite.Value 
                    ? "This article has been added to your favorites." 
                    : "Click the heart to save this article.");
    }
}
```

## Extension Functions

There are several extension functions that can be used to generate these boolean inputs. `ToBoolInput` creates a
`BoolInputs.CheckBox` variant. `ToSwitchInput` creates a `BoolInputs.Switch` variant, and `ToToggleInput` creates
a `BoolInputs.Toggle` variant.

```csharp demo-below
public class BoolInputVariants : ViewBase
{
    public override object? Build()
    {
        var agree = UseState(false);
        var dark = UseState(false);
        var boolVal3 = UseState(false);
        return Layout.Vertical()
                | agree.ToBoolInput().Label("Agree to terms") 
                | dark.ToSwitchInput().Label("Dark Theme");
    }
}
```

## Bool represented by integers

`BoolInput`s have been historically attempted to represent with integers. `0` indicates `false` and `1` indicates `true`.
The following example shows how integers can be used to represent bool inputs.

```csharp demo-below
public class BoolInputVariants2 : ViewBase
{
    public override object? Build()
    {
        // Initial state is false because 0 means false 
        var boolVal1 = UseState(0);
        // Initial state is true because 1 means true
        var boolVal2 = UseState(1);
        var boolVal3 = UseState(0);
        return Layout.Vertical()
                | Layout.Horizontal()
                   | boolVal1.ToBoolInput().Label("Legacy")
                   | Text.Block($"value of Legacy is set to {boolVal1.Value.ToString()}")
                | Layout.Horizontal()
                   | boolVal2.ToSwitchInput().Label("Legacy2")
                   | Text.Block($"value of Legacy2 is set to {boolVal2.Value.ToString()}") 
                | Layout.Horizontal()
                   | boolVal3.ToToggleInput(Icons.MoonStar).Label("Dark")
                   | Text.Block($"value of Dark is set to {boolVal3.Value.ToString()}");
    }
}
```

All values captured are integers; either 1 or 0.

<WidgetDocs Type="Ivy.BoolInput" ExtensionTypes="Ivy.BoolInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/BoolInput.cs"/>

## Examples

This is a set of few examples showing how to use `BoolInput`s in several situations.

### Round trip example

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
                | Layout.Vertical()
                   | Text.Label("Departure Date:")
                   | departureDate.ToDateTimeInput()
                                  .Variant(DateTimeInputs.Date)
                                  .Placeholder("Select departure date")
                // Return Date (only visible when round trip is on)
                | (Layout.Vertical()
                       | Text.Label("Return Date:")
                       | returnDate.ToDateTimeInput()
                                   .Variant(DateTimeInputs.Date)
                                   .Placeholder("Select return date")
                                   .Disabled(!isRoundTrip.Value))
                // Summary
                | Text.Small($"Round trip: {departureDate.Value:MMM dd} → {returnDate.Value:MMM dd}")
                | Text.Small($"One way: {departureDate.Value:MMM dd}");
    }
}
```
