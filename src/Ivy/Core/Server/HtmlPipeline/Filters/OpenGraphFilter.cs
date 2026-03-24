using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline.Filters;

public class OpenGraphFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var head = document.Root?.Element("head");
        if (head == null) return;

        var meta = context.ServerArgs.Metadata;

        var title = meta.Title;
        if (string.IsNullOrEmpty(title))
        {
            title = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Ivy App";
        }

        var siteName = meta.OgSiteName;
        if (string.IsNullOrEmpty(siteName))
        {
            siteName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        }

        var ogImage = meta.OgImage;
        if (string.IsNullOrEmpty(ogImage) && !string.IsNullOrEmpty(meta.GitHubUrl))
        {
            var encodedTitle = System.Net.WebUtility.UrlEncode(title);
            var repo = ExtractGitHubRepo(meta.GitHubUrl);
            if (repo != null)
            {
                ogImage = $"https://banner.ivy.app/ogImage?text={encodedTitle}&repo={repo}";
            }
        }

        // Open Graph
        AddMeta(head, "property", "og:title", title);

        if (!string.IsNullOrEmpty(meta.Description))
            AddMeta(head, "property", "og:description", meta.Description);

        AddMeta(head, "property", "og:type", meta.OgType ?? "website");

        if (!string.IsNullOrEmpty(ogImage))
        {
            AddMeta(head, "property", "og:image", ogImage);
            AddMeta(head, "property", "og:image:width", "1200");
            AddMeta(head, "property", "og:image:height", "630");
            AddMeta(head, "property", "og:image:alt", title);
        }

        if (!string.IsNullOrEmpty(siteName))
            AddMeta(head, "property", "og:site_name", siteName);

        if (!string.IsNullOrEmpty(meta.OgLocale))
            AddMeta(head, "property", "og:locale", meta.OgLocale);

        // Twitter/X Cards
        AddMeta(head, "name", "twitter:card", meta.TwitterCard ?? "summary_large_image");
        AddMeta(head, "name", "twitter:title", title);

        if (!string.IsNullOrEmpty(meta.Description))
            AddMeta(head, "name", "twitter:description", meta.Description);

        if (!string.IsNullOrEmpty(ogImage))
        {
            AddMeta(head, "name", "twitter:image", ogImage);
            AddMeta(head, "name", "twitter:image:alt", title);
        }
    }

    private static string? ExtractGitHubRepo(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        var segments = uri.AbsolutePath.Trim('/').Split('/');
        if (segments.Length >= 2)
            return $"{segments[0]}/{segments[1]}";

        return null;
    }

    private static void AddMeta(XElement head, string attributeName, string key, string content)
    {
        head.Add(new XElement("meta",
            new XAttribute(attributeName, key),
            new XAttribute("content", content)));
    }
}
