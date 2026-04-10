using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

/// <summary>
/// Injects the Ivy Desktop Bridge script into the HTML when running in desktop mode.
/// This bridge provides native filesystem access (e.g. showDirectoryPicker) by
/// communicating with the Photino host.
/// </summary>
public class DesktopBridgeFilter : IHtmlFilter
{
    public static readonly string BridgeScript = @"
        (function() {
            if (window.__ivy_desktop) return;
            
            // macOS Shim: Redirect window.external.sendMessage to webkit message handler
            if (!window.external || !window.external.sendMessage) {
                if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.photino) {
                    window.external = window.external || {};
                    window.external.sendMessage = function(msg) {
                        try {
                            window.webkit.messageHandlers.photino.postMessage(msg);
                        } catch (e) {
                            console.error('[Ivy Desktop] Failed to post message to photino handler:', e);
                        }
                    };
                }
            }

            if (!window.external || !window.external.sendMessage) {
                console.warn('[Ivy Desktop] Photino native bridge not found. Full path picking will not work.');
                return;
            }

            const pendingRequests = new Map();
            window.__ivy_desktop = {
                showDirectoryPicker: function() {
                    console.log('[Ivy Desktop] showDirectoryPicker called');
                    const id = Math.random().toString(36).substr(2, 9);
                    return new Promise((resolve) => {
                        pendingRequests.set(id, resolve);
                        console.log('[Ivy Desktop] Requesting folder picker from C#, id:', id);
                        window.external.sendMessage(JSON.stringify({
                            type: 'showDirectoryPicker',
                            id: id
                        }));
                    });
                }
            };

            const handleResponse = (msg) => {
                console.log('[Ivy Desktop] Message received from C#:', msg);
                try {
                    const data = (typeof msg === 'string') ? JSON.parse(msg) : msg;
                    if (data && data.type === 'showDirectoryPickerRes') {
                        const resolve = pendingRequests.get(data.id);
                        if (resolve) {
                            pendingRequests.delete(data.id);
                            console.log('[Ivy Desktop] Resolving request', data.id, 'with', data.result);
                            resolve(data.result);
                        } else {
                            console.warn('[Ivy Desktop] No pending request found for id:', data.id);
                        }
                    }
                } catch (e) {
                    console.error('[Ivy Desktop] Error parsing response from C#:', e);
                }
            };

            // Handle both flavors of Photino messaging
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.addEventListener('message', arg => handleResponse(arg.data));
            } else {
                window.addEventListener('message', arg => handleResponse(arg.data));
            }
            
            console.log('[Ivy Desktop] Bridge initialized.');
        })();
    ";

    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (!context.ServerArgs.IsDesktop) return;

        var head = document.Root?.Element("head");
        if (head == null) return;

        head.Add(new XElement("script",
            new XAttribute("src", "/ivy/desktop-bridge.js")));
    }
}
