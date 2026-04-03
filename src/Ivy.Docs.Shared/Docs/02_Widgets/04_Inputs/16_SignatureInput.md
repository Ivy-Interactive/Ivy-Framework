---
searchHints:
  - signature
  - sign
  - drawing
  - canvas
  - handwriting
  - pen
---

# SignatureInput

<Ingress>
Capture handwritten signatures directly in the browser with a canvas-based drawing widget for delivery orders, contracts, and approval workflows.
</Ingress>

The `SignatureInput` widget provides a canvas-based signature capture interface. Users can draw signatures using mouse or touch input, and the result is stored as a `byte[]` containing PNG image data.

## Basic Usage

```csharp demo-below
public class SignatureDemo : ViewBase
{
    public override object? Build()
    {
        var signature = UseState<byte[]?>(null);
        return signature.ToSignatureInput()
            .Placeholder("Draw your signature here");
    }
}
```

## Pen Customization

Configure pen color and thickness to match your application's style:

```csharp demo-below
public class SignaturePenDemo : ViewBase
{
    public override object? Build()
    {
        var signature = UseState<byte[]?>(null);
        return Layout.Grid().Columns(2).ColumnWidths(Size.Units(30), null)
            | Text.P("Blue, thick").Small() | signature.ToSignatureInput().Pen(Colors.Blue).PenThickness(4)
            | Text.P("Red, thin").Small() | signature.ToSignatureInput().Pen(Colors.Red).PenThickness(1);
    }
}
```

## Styling

`SignatureInput` supports disabled and invalid states:

```csharp demo-below
public class SignatureStylingDemo : ViewBase
{
    public override object? Build()
    {
        var signature = UseState<byte[]?>(null);
        return Layout.Grid().Columns(2).ColumnWidths(Size.Units(30), null)
            | Text.P("Disabled").Small() | signature.ToSignatureInput().Disabled()
            | Text.P("Invalid").Small() | signature.ToSignatureInput().Invalid("Signature is required");
    }
}
```

## Form Integration

Use `SignatureInput` inside a form with field labels and validation:

```csharp demo-below
public class SignatureFormDemo : ViewBase
{
    public override object? Build()
    {
        var signature = UseState<byte[]?>(null);
        return signature.ToSignatureInput()
            .Placeholder("Sign here")
            .WithField()
            .Label("Customer Signature")
            .Description("Please sign to confirm delivery");
    }
}
```

## Event Handling

Signature inputs support focus, blur, and manual `AutoFocus` behavior.

```csharp demo-tabs
public class SignatureInputEventsDemo : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState<byte[]?>(null);

        return Layout.Tabs(
            new Tab("OnFocus", Layout.Vertical()
                | Text.P("The OnFocus event fires when the signature canvas gains focus.")
                | state.ToSignatureInput().Placeholder("Focus me...")
                    .OnFocus(() => focusCount.Set(focusCount.Value + 1))
                | Text.Literal($"Focus Count {focusCount.Value}")
            ),
            new Tab("OnBlur", Layout.Vertical()
                | Text.P("The OnBlur event fires when the signature canvas loses focus.")
                | state.ToSignatureInput().Placeholder("Blur me...")
                    .OnBlur(() => blurCount.Set(blurCount.Value + 1))
                | Text.Literal($"Blur Count {blurCount.Value}")
            ),
            new Tab("AutoFocus", Layout.Vertical()
                | Text.P("The AutoFocus property automatically focuses the widget and shows a focus ring upon mounting.")
                | state.ToSignatureInput().Placeholder("AutoFocused SignatureInput")
                    .AutoFocus()
                | Text.Lead("Focused!")
            )
        ).Variant(TabsVariant.Tabs);
    }
}
```

<WidgetDocs Type="Ivy.SignatureInput" ExtensionTypes="Ivy.SignatureInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/SignatureInput.cs"/>
