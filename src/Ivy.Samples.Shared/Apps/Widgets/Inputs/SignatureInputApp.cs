
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.PenTool, group: ["Widgets", "Inputs"], searchHints: ["signature", "sign", "drawing", "canvas", "handwriting", "pen"])]
public class SignatureInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Signature Input")
               | Text.H2("Basic Signature")
               | new SignatureInputBasic()
               | Text.H2("Custom Styling")
               | new SignatureInputStyling()
               | Text.H2("States")
               | new SignatureInputStates()
               | Text.H2("Events")
               | new SignatureInputEvents()
            ;
    }
}

public class SignatureInputBasic : ViewBase
{
    public override object Build()
    {
        var signature = UseState<byte[]?>(() => null);

        return Layout.Vertical()
               | signature.ToSignatureInput()
                   .Placeholder("Draw your signature here")
                   .WithField()
                   .Label("Signature")
                   .Description("Sign in the box above")
               | Text.P(signature.Value != null
                   ? $"Signature captured ({signature.Value.Length} bytes)"
                   : "No signature yet");
    }
}

public class SignatureInputStyling : ViewBase
{
    public override object Build()
    {
        var sig1 = UseState<byte[]?>(() => null);
        var sig2 = UseState<byte[]?>(() => null);
        var sig3 = UseState<byte[]?>(() => null);

        return Layout.Grid().Columns(3)
               | Text.Monospaced("Blue pen, thick")
               | Text.Monospaced("Red pen, thin")
               | Text.Monospaced("Default")

               | sig1.ToSignatureInput()
                   .Pen(Colors.Blue)
                   .PenThickness(4)
                   .Placeholder("Blue signature")
               | sig2.ToSignatureInput()
                   .Pen(Colors.Red)
                   .PenThickness(1)
                   .Placeholder("Red signature")
               | sig3.ToSignatureInput()
                   .Placeholder("Default signature");
    }
}

public class SignatureInputStates : ViewBase
{
    public override object Build()
    {
        var disabledSig = UseState<byte[]?>(() => null);
        var invalidSig = UseState<byte[]?>(() => null);

        return Layout.Grid().Columns(2)
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")

               | disabledSig.ToSignatureInput()
                   .Disabled()
                   .Placeholder("Cannot draw")
               | invalidSig.ToSignatureInput()
                   .Invalid("Signature is required")
                   .Placeholder("Draw here");
    }
}

public class SignatureInputEvents : ViewBase
{
    public override object Build()
    {
        var onBlurState = UseState<byte[]?>(() => null);
        var onBlurLabel = UseState("");
        var onFocusState = UseState<byte[]?>(() => null);
        var onFocusLabel = UseState("");

        return Layout.Vertical()
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("The blur event fires when the signature pad loses focus.").Small()
                    | onBlurState.ToSignatureInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                    | (onBlurLabel.Value != ""
                        ? Callout.Success(onBlurLabel.Value)
                        : Callout.Info("Interact then click away to see blur events"))
            ).Title("OnBlur Handler")
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("The focus event fires when you click on or tab into the signature pad.").Small()
                    | onFocusState.ToSignatureInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                    | (onFocusLabel.Value != ""
                        ? Callout.Success(onFocusLabel.Value)
                        : Callout.Info("Click or tab into the signature pad to see focus events"))
            ).Title("OnFocus Handler");
    }
}
