using System.Text.RegularExpressions;

namespace Ivy.Docs.Tools;

public partial class LinkConverter(string currentFilePath)
{
    public (HashSet<string> types, string markdown) Convert(string markdown)
    {
        var types = new HashSet<string>();
        var regex = MyRegex();
        var matches = regex.Matches(markdown);
        foreach (Match match in matches)
        {
            if (match.Value.StartsWith("!")) continue; //this is an image
            var (type, linkReplacement) = ConvertLink(match);
            if (type == null) continue;
            types.Add(type);
            markdown = markdown.Replace(match.Value, linkReplacement);
        }

        return (types, markdown);
    }

    private (string? type, string? linkReplacement) ConvertLink(Match match)
    {
        var text = match.Groups[1].Value;
        var link = match.Groups[2].Value;

        if (link.StartsWith("app://") || link.StartsWith("http://") || link.StartsWith("https://") ||
            link.StartsWith("mailto:") || link.StartsWith("tel:") || link.StartsWith("#"))
        {
            return (null, null); //do nothing
        }

        // Strip fragment identifier (#...) from link before processing
        var fragmentIndex = link.IndexOf('#');
        var linkWithoutFragment = fragmentIndex >= 0 ? link[..fragmentIndex] : link;
        var fragment = fragmentIndex >= 0 ? link[fragmentIndex..] : "";

        var path = Utils.GetPathForLink(currentFilePath, linkWithoutFragment);
        var type = Utils.GetTypeNameFromPath(path);
        var appId = Utils.GetAppIdFromTypeName(type);

        return (type, $"[{text}](app://{appId}{fragment})");
    }

    [GeneratedRegex(@"!?\[(.*?)\]\((.*?)\)")]
    private static partial Regex MyRegex();
}
