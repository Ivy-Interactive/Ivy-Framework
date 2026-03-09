namespace Ivy.Core.Server.ContentPipeline.Filters;

public class DevToolsFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        if (context.ServerArgs.EnableDevTools)
        {
            var ivyEnableDevToolsTag = $"<meta name=\"ivy-enable-dev-tools\" content=\"true\" />";
            html = html.Replace("</head>", $"  {ivyEnableDevToolsTag}\n</head>");
        }

        return html;
    }
}
