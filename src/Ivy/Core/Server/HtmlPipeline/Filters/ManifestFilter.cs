using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class ManifestFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var manifest = context.Services.GetService<ManifestOptions>();
        if (manifest != null)
        {
            var head = document.Root?.Element("head");
            head?.Add(new XElement("link",
                new XAttribute("rel", "manifest"),
                new XAttribute("href", "/manifest.json")));
        }
    }
}
