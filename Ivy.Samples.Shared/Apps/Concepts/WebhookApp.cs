using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Webhook)]
public class WebhookApp : ViewBase
{
    public override object? Build()
    {
        var counter = UseState(0);
        var url = this.UseWebhook(_ =>
        {
            counter.Set(counter.Value + 1);
        });

        return Layout.Vertical()
               | counter
               | url;
    }
}