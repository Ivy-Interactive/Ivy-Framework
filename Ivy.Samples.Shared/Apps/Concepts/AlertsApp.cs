using Ivy.Shared;
using Ivy.Views.Alerts;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Bell, searchHints: ["notification", "dialog", "modal", "message", "popup", "confirm"])]
public class AlertsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var (alertView, showAlert) = this.UseAlert();
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            new Button("YesNoCancel", _ => showAlert("Hello, World!", result =>
            {
                client.Toast(result.ToString());
            }, "Alert Title", AlertButtonSet.YesNoCancel)),
            alertView
        );
    }
}