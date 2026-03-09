using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class LicenseFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var configuration = context.Services.GetRequiredService<IConfiguration>();
        var head = document.Root?.Element("head");
        if (head == null) return;

        var ivyLicense = configuration["Ivy:License"] ?? "";
        if (!string.IsNullOrEmpty(ivyLicense))
        {
            head.Add(new XElement("meta",
                new XAttribute("name", "ivy-license"),
                new XAttribute("content", ivyLicense)));
        }

#if DEBUG
        var ivyLicensePublicKey = configuration["Ivy:LicensePublicKey"] ?? "";
        if (!string.IsNullOrEmpty(ivyLicensePublicKey))
        {
            head.Add(new XElement("meta",
                new XAttribute("name", "ivy-license-public-key"),
                new XAttribute("content", ivyLicensePublicKey)));
        }
#endif
    }
}
