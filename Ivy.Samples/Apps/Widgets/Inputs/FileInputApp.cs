using System.Text;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.Upload)]
public class FileInputApp : ViewBase
{
    public override object? Build()
    {
        var file = this.UseState((FileInput?)null);
        return
            Layout.Vertical()
            | file.ToFileInput()
            | file.ToDetails().Remove(e => e.Content)
            | file.Value?.ToPlainText()
            ;
    }
}