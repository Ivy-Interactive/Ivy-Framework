using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class LocalFilesFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (context.ServerArgs.DangerouslyAllowLocalFiles)
        {
            var head = document.Root?.Element("head");
            head?.Add(new XElement("meta",
                new XAttribute("name", "ivy-dangerously-allow-local-files"),
                new XAttribute("content", "true")));
        }
    }
}
