using Ivy.IvyML.Studio.Helpers;

namespace Ivy.IvyML.Studio.Apps.Views;

public class PreviewView : ViewBase
{
    // Reused across renders; the constructor reflects over the widget assembly, so build it once.
    private static readonly XamlBuilder Builder = new();

    public override object Build()
    {
        var latest = Context.UseLatestWireframe();
        var snapshot = latest.Value;

        return Layout.Vertical().Gap(2).Padding(4).Height(Size.Full()).Width(Size.Full())
            | RenderPreview(snapshot.Content);
    }

    private static object RenderPreview(string? ivyml)
    {
        if (string.IsNullOrWhiteSpace(ivyml))
            return Layout.Center().Height(Size.Full()).Width(Size.Full())
                | Text.Muted("Your preview will appear here.");

        try
        {
            return Builder.Build(ivyml);
        }
        catch (Exception ex)
        {
            // Invalid or mid-write IvyML — surface the error instead of throwing out of Build().
            return Layout.Vertical().Gap(2)
                | Text.Strong("Couldn't render this wireframe").Color(Colors.Red)
                | new CodeBlock(ex.Message, Languages.Text).Width(Size.Full());
        }
    }
}
