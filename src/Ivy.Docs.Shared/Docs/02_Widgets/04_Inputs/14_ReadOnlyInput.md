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
Display [form](../../01_Onboarding/02_Concepts/08_Forms.md) data in a consistent input-like style that maintains visual coherence while preventing user modification.
</Ingress>

The `ReadOnlyInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays data in an input-like format that cannot be edited by the user. It's useful for showing form values in a consistent style with other [inputs](../../01_Onboarding/02_Concepts/03_Widgets.md), while preventing modification.

## Basic Usage

Here's a simple example of a `ReadOnlyInput` displaying a value:

```csharp demo-below
public class ReadOnlyDemo : ViewBase
{    
    public override object? Build()
    {    
        var value = UseState(123.45);
        var readOnlyInput = value.ToReadOnlyInput();
        return readOnlyInput;
    }    
}    
```

## Event Handling

Read-only inputs support focus, blur, and manual `AutoFocus` behavior.

```csharp demo-tabs
public class ReadOnlyInputEventsDemo : ViewBase
{
    public override object? Build()
    {
        var value = UseState("Static Info");
        var show = UseState(false);
        var onFocusTriggered = UseState(false);
        var onBlurTriggered = UseState(false);

        return Layout.Vertical().Gap(4)
            | new Button("Mount with AutoFocus", () => {
                show.Set(true);
                onFocusTriggered.Set(false);
                onBlurTriggered.Set(false);
            }).Primary()
            | (show.Value ? Layout.Vertical().Gap(2)
                | value.ToReadOnlyInput()
                    .OnFocus<string>(() => onFocusTriggered.Set(true))
                    .OnBlur<string>(() => onBlurTriggered.Set(true))
                    .AutoFocus()
                | (onFocusTriggered.Value ? Callout.Success("OnFocus triggered (via AutoFocus)") : null)
                | (onBlurTriggered.Value ? Callout.Warning("OnBlur triggered") : null)
                | new Button("Reset Demo", () => show.Set(false)).Outline().Small()
                : null);
    }
}
```

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
        var total = UseState(price.Value * quantity.Value);
        
        UseEffect(() => {
            total.Set(price.Value * quantity.Value);
        }, price, quantity);
        
        return Layout.Vertical().Gap(2)
            | price.ToNumberInput()
                .WithField().Label("Price")
            | quantity.ToNumberInput()
                .WithField().Label("Quantity")
            | total.ToReadOnlyInput()
                .WithField().Label("Total");
    }
}
```

</Body>
</Details>
