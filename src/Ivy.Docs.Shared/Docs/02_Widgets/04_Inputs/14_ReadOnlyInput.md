---
searchHints:
  - disabled
  - readonly
  - display
  - static
  - non-editable
  - locked
---

# ReadOnlyInput

<Ingress>
Display [form](../../01_Onboarding/02_Concepts/13_Forms.md) data in a consistent input-like style that maintains visual coherence while preventing user modification.
</Ingress>

The `ReadOnlyInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays data in an input-like format that cannot be edited by the user. It's useful for showing form values in a consistent style with other [inputs](../../01_Onboarding/02_Concepts/03_Widgets.md), while preventing modification.

## Basic Usage

Here's a simple example of a `ReadOnlyInput` displaying a value:

```csharp demo-below
public class ReadOnlyDemo : ViewBase
{    
    public override object? Build()
    {    
        double value = 123.45;
        var readOnlyInput = new ReadOnlyInput<double>(value);
        return readOnlyInput;
    }    
}    
```

<WidgetDocs Type="Ivy.ReadOnlyInput" ExtensionTypes="Ivy.ReadOnlyInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/ReadOnlyInput.cs"/>

## Examples

<Details>
<Summary>
ReadOnlyInput can be used to display computed or derived values in a form alongside editable inputs.
</Summary>
<Body>

```csharp demo-tabs
public class ReadOnlyFormDemo : ViewBase
{
    public override object? Build()
    {
        var price = UseState(100.0);
        var quantity = UseState(5);
        var total = price.Value * quantity.Value;
        
        return Layout.Vertical().Gap(2)
            | new NumberInput<double>(price)
                .WithField().Label("Price")
            | new NumberInput<int>(quantity)
                .WithField().Label("Quantity")
            | new ReadOnlyInput<double>(total)
                .WithField().Label("Total");
    }
}
```

</Body>
</Details>
