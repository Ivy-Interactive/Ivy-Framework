using Ivy.Views;
using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Webhook, searchHints: ["callbacks", "endpoints", "api", "integration", "url", "trigger"])]
public class WebhookApp : SampleBase
{
    protected override object? BuildSample()
    {
        var counter = UseState(0);
        var url = UseWebhook(_ =>
        {
            counter.Set(counter.Value + 1);
        });

        return Layout.Vertical()
               | counter
               | url;
    }
}