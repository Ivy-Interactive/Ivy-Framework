namespace Ivy.Core.Server.ContentPipeline.Filters;

public class MetaDescriptionFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.Metadata.Description))
        {
            var metaDescriptionTag = $"<meta name=\"description\" content=\"{context.ServerArgs.Metadata.Description}\" />";
            html = html.Replace("</head>", $"  {metaDescriptionTag}\n</head>");
        }

        return html;
    }
}
