using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class MetaDescriptionFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.MetaDescription))
        {
            var head = document.Root?.Element("head");
            head?.Add(new XElement("meta",
                new XAttribute("name", "description"),
                new XAttribute("content", context.ServerArgs.MetaDescription)));
        }
    }
}
