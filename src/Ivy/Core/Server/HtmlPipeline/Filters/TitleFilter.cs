using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class TitleFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.MetaTitle))
        {
            var head = document.Root?.Element("head");
            var titleElement = head?.Element("title");
            if (titleElement != null)
            {
                titleElement.Value = context.ServerArgs.MetaTitle;
            }
        }
    }
}
