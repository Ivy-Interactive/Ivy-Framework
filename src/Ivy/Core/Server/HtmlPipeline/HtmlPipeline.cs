using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Ivy.Core.Server.HtmlPipeline;

public class HtmlPipeline
{
    private readonly List<IHtmlFilter> _filters = new();

    public IReadOnlyList<IHtmlFilter> Filters => _filters;

    public HtmlPipeline Use(IHtmlFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    public HtmlPipeline Use<T>() where T : IHtmlFilter, new() => Use(new T());

    public HtmlPipeline Clear()
    {
        _filters.Clear();
        return this;
    }

    public string Process(HtmlPipelineContext context, string html)
    {
        // Strip DOCTYPE for XML parsing
        var doctype = "";
        var doctypeMatch = Regex.Match(html, @"<!doctype[^>]*>\s*", RegexOptions.IgnoreCase);
        if (doctypeMatch.Success)
        {
            doctype = doctypeMatch.Value;
            html = html.Substring(doctypeMatch.Length);
        }

        // Normalize bare boolean attributes for XML compatibility
        html = Regex.Replace(html, @"\bcrossorigin(?!=)", @"crossorigin=""anonymous""");

        // Self-close void HTML elements for XML compatibility
        html = Regex.Replace(html, @"<(link|meta|br|hr|img|input|source|track|wbr|col|area|base|embed)(\s[^>]*)?>", m =>
        {
            var tag = m.Value;
            if (tag.EndsWith("/>")) return tag;
            return tag.TrimEnd('>') + " />";
        });

        var doc = XDocument.Parse(html, LoadOptions.PreserveWhitespace);
        foreach (var filter in _filters)
            filter.Process(context, doc);

        var result = doc.ToString(SaveOptions.DisableFormatting);

        // Expand self-closing non-void elements back to open/close pairs
        // (XDocument serializes empty elements as self-closing, but HTML requires explicit close tags for these)
        result = Regex.Replace(
            result,
            @"<(script|div|title|style|span|a|textarea|iframe|canvas|video|audio|object|select|option|tbody|thead|tfoot|tr|td|th|ul|ol|li|form|button|label|p|section|article|nav|aside|header|footer|main|body)(\s[^>]*)?\s*/>",
            "<$1$2></$1>");

        return doctype + result;
    }
}
