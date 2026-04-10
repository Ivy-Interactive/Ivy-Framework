using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

/// <summary>
/// Injects the Ivy Desktop Bridge script into the HTML when running in desktop mode.
/// This bridge provides native filesystem access (e.g. showDirectoryPicker) by
/// communicating with the Photino host.
/// </summary>
public class DesktopBridgeFilter : IHtmlFilter
{
    private const string BridgeScript = @"
        (function() {
            if (window.__ivy_desktop) return;
            const pendingRequests = new Map();
            window.__ivy_desktop = {
                showDirectoryPicker: function() {
                    const id = Math.random().toString(36).substr(2, 9);
                    return new Promise((resolve) => {
                        pendingRequests.set(id, resolve);
                        window.external.sendMessage(JSON.stringify({
                            type: 'showDirectoryPicker',
                            id: id
                        }));
                    });
                }
            };

            const handleResponse = (msg) => {
                try {
                    const data = typeof msg === 'string' ? JSON.parse(msg) : msg;
                    if (data && data.type === 'showDirectoryPickerRes') {
                        const resolve = pendingRequests.get(data.id);
                        if (resolve) {
                            pendingRequests.delete(data.id);
                            resolve(data.result);
                        }
                    }
                } catch (e) {
                    // Ignore non-bridge messages
                }
            };

            // Handle both flavors of Photino messaging
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.addEventListener('message', arg => handleResponse(arg.data));
            } else {
                window.addEventListener('message', arg => handleResponse(arg.data));
            }
        })();
    ";

    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (!context.ServerArgs.IsDesktop) return;

        var head = document.Root?.Element("head");
        if (head == null) return;

        head.Add(new XElement("script",
            new XAttribute("type", "text/javascript"),
            new XText(BridgeScript)));
    }
}
