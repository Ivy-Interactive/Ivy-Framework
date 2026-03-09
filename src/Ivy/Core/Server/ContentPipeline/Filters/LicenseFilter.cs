using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Server.ContentPipeline.Filters;

public class LicenseFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        var configuration = context.Services.GetRequiredService<IConfiguration>();

        var ivyLicense = configuration["Ivy:License"] ?? "";
        if (!string.IsNullOrEmpty(ivyLicense))
        {
            var ivyLicenseTag = $"<meta name=\"ivy-license\" content=\"{ivyLicense}\" />";
            html = html.Replace("</head>", $"  {ivyLicenseTag}\n</head>");
        }

#if DEBUG
        var ivyLicensePublicKey = configuration["Ivy:LicensePublicKey"] ?? "";
        if (!string.IsNullOrEmpty(ivyLicensePublicKey))
        {
            var ivyLicensePublicKeyTag =
                $"<meta name=\"ivy-license-public-key\" content=\"{ivyLicensePublicKey}\" />";
            html = html.Replace("</head>", $"  {ivyLicensePublicKeyTag}\n</head>");
        }
#endif

        return html;
    }
}
