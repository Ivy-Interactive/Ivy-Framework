using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

/// <summary>
/// Emits the markers the browser needs for WebMCP: a flag the frontend reads to decide whether to
/// touch <c>document.modelContext</c> at all, and the Chrome origin trial token when one is set.
/// No-ops unless the host called <c>server.UseWebMcp()</c>.
/// </summary>
public class WebMcpFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var options = context.Services.GetService<WebMcpOptions>();
        if (options == null) return;

        var head = document.Root?.Element("head");
        if (head == null) return;

        head.Add(new XElement("meta",
            new XAttribute("name", "ivy-webmcp"),
            new XAttribute("content", "true")));

        if (!string.IsNullOrWhiteSpace(options.OriginTrialToken))
        {
            head.Add(new XElement("meta",
                new XAttribute("http-equiv", "origin-trial"),
                new XAttribute("content", options.OriginTrialToken)));
        }
    }
}
