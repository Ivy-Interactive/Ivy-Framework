using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class MetaGitHubUrlFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.Metadata.GitHubUrl))
        {
            var head = document.Root?.Element("head");
            head?.Add(new XElement("meta",
                new XAttribute("name", "github-url"),
                new XAttribute("content", context.ServerArgs.Metadata.GitHubUrl)));
        }
    }
}
