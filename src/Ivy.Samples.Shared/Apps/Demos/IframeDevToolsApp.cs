using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.CodeXml, group: ["Demos"], searchHints: ["devtools", "iframe", "testing", "visual", "edit"])]
public class IframeDevToolsApp : ViewBase
{
    public override object? Build()
    {
        var iframeUrl = UseState("/hello");
        var devToolsEnabled = UseState(false);
        var messages = UseState(Array.Empty<string>());
        var outboundToken = UseState<string?>(null);

        async ValueTask HandleToggle(Event<Button> _)
        {
            var next = !devToolsEnabled.Value;
            devToolsEnabled.Set(next);
            outboundToken.Set(next ? "true" : "false");
        }

        async ValueTask HandleMessage(Event<Iframe, (string type, JsonNode payload)> e)
        {
            if (e.Value.type == "DEVTOOLS_APPLY_CHANGES")
            {
                var json = e.Value.payload?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? "null";
                messages.Set([.. messages.Value, json]);
            }
        }

        var iframe = new Iframe(iframeUrl.Value)
            .Height(Size.Units(120))
            .OutboundMessageType("DEVTOOLS_SET_ENABLED")
            .OutboundMessageToken(outboundToken.Value ?? "false");
        iframe.OnMessageReceived = HandleMessage;

        return Layout.Vertical()
            | (Layout.Horizontal()
                | iframeUrl.ToInput(placeholder: "Iframe URL, e.g. /hello")
                | new Button(devToolsEnabled.Value ? "DevTools: On" : "DevTools: Off", HandleToggle)
                    .Icon(devToolsEnabled.Value ? Icons.EyeOff : Icons.Eye)
                    .Variant(devToolsEnabled.Value ? ButtonVariant.Primary : ButtonVariant.Outline))
            | iframe
            | (messages.Value.Length > 0
                ? Layout.Vertical()
                    | Text.H4("Received DevTools Messages")
                    | (Layout.Vertical().Gap(2) | messages.Value.Select(m => Text.Code(m, Languages.Json)))
                : null);
    }
}
