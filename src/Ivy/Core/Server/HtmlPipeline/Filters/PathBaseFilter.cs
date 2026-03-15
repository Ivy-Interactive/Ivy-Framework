using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

/// <summary>
/// Injects a &lt;meta name="ivy-path-base"&gt; tag so the frontend can build
/// correct API/SignalR URLs when served behind a reverse proxy that preserves
/// the full path (e.g. /{customerId}/studio).
/// Asset paths in index.html are relative (Vite base: './') so they resolve
/// correctly against the page URL without any rewriting here.
/// </summary>
public class PathBaseFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var pathBase = context.ServerArgs.PathBase;
        if (string.IsNullOrEmpty(pathBase))
            return;

        var head = document.Root?.Element("head");
        head?.AddFirst(new XElement("meta",
            new XAttribute("name", "ivy-path-base"),
            new XAttribute("content", pathBase.TrimEnd('/'))));
    }
}
