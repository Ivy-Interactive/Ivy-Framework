
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.PenTool, group: ["Widgets", "Inputs"], searchHints: ["signature", "sign", "drawing", "canvas", "handwriting", "pen"])]
public class SignatureInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H2("Basic Signature")
               | new SignatureInputBasic()
               | Text.H2("Custom Styling")
               | new SignatureInputStyling()
               | Text.H2("States")
               | new SignatureInputStates()
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
