using System.Text.RegularExpressions;

namespace Ivy.Core.Server.ContentPipeline.Filters;

public class TitleFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.MetaTitle))
        {
            var metaTitleTag = $"<title>{context.ServerArgs.MetaTitle}</title>";
            html = Regex.Replace(html, "<title>.*?</title>", metaTitleTag, RegexOptions.Singleline);
        }

        return html;
    }
}
