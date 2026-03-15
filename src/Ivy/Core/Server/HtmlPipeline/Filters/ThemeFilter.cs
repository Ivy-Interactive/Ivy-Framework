using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class ThemeFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var themeService = context.Services.GetService<IThemeService>();
        if (themeService != null)
        {
            var head = document.Root?.Element("head");
            if (head == null) return;

            var themeMetaTag = themeService.GenerateThemeMetaTag();
            head.Add(XElement.Parse(themeMetaTag));

            var themeCss = themeService.GenerateThemeCss();
            head.Add(XElement.Parse(themeCss));
        }
    }
}
