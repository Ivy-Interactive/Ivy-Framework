namespace Ivy.Core.Server.ContentPipeline.Filters;

public class MetaGitHubUrlFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.MetaGitHubUrl))
        {
            var metaTag = $"<meta name=\"github-url\" content=\"{context.ServerArgs.MetaGitHubUrl}\" />";
            html = html.Replace("</head>", $"  {metaTag}\n</head>");
        }

        return html;
    }
}
