# ReadOnlyInput

<Ingress>
Display form data in a consistent input-like style that maintains visual coherence while preventing user modification.
</Ingress>

The `ReadOnlyInput` widget displays data in an input-like format that cannot be edited by the user. It's useful for showing form values in a consistent style with other inputs, while preventing modification.

## Basic Usage

Here's a simple example of a `ReadOnlyInput` displaying a value:

```csharp demo-below 
public class ReadOnlyDemo : ViewBase
{    
    public override object? Build()
    {    
        double value = 123.45;
        var readOnlyInput = new ReadOnlyInput<double>(value);
        return Layout.Vertical()
                | readOnlyInput;
    }    
}    
```

## Examples

### Displaying a Value

```csharp
double value = 123.45;
var readOnlyInput = new ReadOnlyInput<double>(value);
```

### Handling Events

```csharp
var state = UseState(123.45);
var readOnlyInput = new ReadOnlyInput<double>(state)
    .OnBlur(e => Console.WriteLine("Input blurred"));
```

### Using ShowCopyButton

```csharp
var readOnlyInput = new ReadOnlyInput<string>("ReadOnly Text")
    .ShowCopyButton(true);
```

<WidgetDocs Type="Ivy.ReadOnlyInput" ExtensionTypes="Ivy.ReadOnlyInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/ReadOnlyInput.cs"/>
