using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class DevToolsFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (context.ServerArgs.EnableDevTools)
        {
            var head = document.Root?.Element("head");
            head?.Add(new XElement("meta",
                new XAttribute("name", "ivy-enable-dev-tools"),
                new XAttribute("content", "true")));
        }
    }
}
