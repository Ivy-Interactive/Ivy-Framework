namespace Ivy.Core.Server.ContentPipeline.Filters;

public class MetaGitHubUrlFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        if (!string.IsNullOrEmpty(context.ServerArgs.Metadata.GitHubUrl))
        {
            var metaTag = $"<meta name=\"github-url\" content=\"{context.ServerArgs.Metadata.GitHubUrl}\" />";
            html = html.Replace("</head>", $"  {metaTag}\n</head>");
        }

        return html;
    }
}
