using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.ContentPipeline.Filters;

public class ManifestFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        var manifest = context.Services.GetService<ManifestOptions>();
        if (manifest != null)
        {
            var manifestLink = "<link rel=\"manifest\" href=\"/manifest.json\" />";
            html = html.Replace("</head>", $"  {manifestLink}\n</head>");
        }

        return html;
    }
}
