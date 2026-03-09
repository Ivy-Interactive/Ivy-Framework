using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.ContentPipeline.Filters;

public class ThemeFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        var themeService = context.Services.GetService<IThemeService>();
        if (themeService != null)
        {
            var themeCss = themeService.GenerateThemeCss();
            var themeMetaTag = themeService.GenerateThemeMetaTag();
            html = html.Replace("</head>", $"  {themeMetaTag}\n  {themeCss}\n</head>");
        }

        return html;
    }
}
