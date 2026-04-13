using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

/// <summary>
/// Injects a &lt;base href&gt; and &lt;meta name="ivy-path-base"&gt; tag so assets and API/SignalR
/// URLs resolve correctly regardless of the page URL.
///
/// Without a path base (local): PathToAppIdMiddleware rewrites app paths to /?appId=... internally,
/// but the browser URL stays at /foo/bar. A &lt;base href="/"&gt; tag ensures relative asset paths
/// (Vite base: './') resolve from root rather than the current page URL.
///
/// With a path base (reverse proxy): the browser URL includes the prefix (e.g. /studio/foo/bar),
/// and relative assets must resolve under that prefix, so &lt;base href="/studio/"&gt; is injected
/// along with the ivy-path-base meta tag for JS use.
/// </summary>
public class BasePathFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var basePath = context.ServerArgs.BasePath;
        var head = document.Root?.Element("head");
        if (head == null) return;

        if (string.IsNullOrEmpty(basePath))
        {
            // No base path: inject <base href="/"> so relative asset paths (./assets/...)
            // resolve from root even when the browser URL has path segments.
            head.AddFirst(new XElement("base", new XAttribute("href", "/")));
        }
        else
        {
            var trimmed = "/" + basePath.Trim('/');
            head.AddFirst(new XElement("meta",
                new XAttribute("name", "ivy-path-base"),
                new XAttribute("content", trimmed)));
            head.AddFirst(new XElement("base",
                new XAttribute("href", trimmed + "/")));
        }
    }
}
