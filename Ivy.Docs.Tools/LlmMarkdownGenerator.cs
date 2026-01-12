using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace Ivy.Docs.Tools;

public record WidgetDocsInfo(
    string TypeName,
    string? ExtensionTypes,
    string? SourceUrl,
    Match OriginalMatch
);

public static partial class LlmMarkdownGenerator
{
    private static readonly Regex DetailsBlockRegex = DetailsRegex();
    private static readonly Regex SummaryStartRegex = SummaryRegex();
    private static readonly Regex BodyStartRegex = BodyRegex();
    private static readonly Regex WidgetDocsRegex = WidgetDocsBlockRegex();

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UsePreciseSourceLocation()
        .UseYamlFrontMatter()
        .Build();

    public static Task<string> GenerateAsync(string sourceMarkdown, string filePath)
    {
        var pipeline = Pipeline;

        var document = Markdig.Markdown.Parse(sourceMarkdown, pipeline);

        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        int contentStartIndex = 0;

        if (yamlBlock != null)
        {
            contentStartIndex = yamlBlock.Span.End + 1;

            while (contentStartIndex < sourceMarkdown.Length &&
                   (sourceMarkdown[contentStartIndex] == '\n' || sourceMarkdown[contentStartIndex] == '\r'))
            {
                contentStartIndex++;
            }
        }

        string content = contentStartIndex < sourceMarkdown.Length
            ? sourceMarkdown[contentStartIndex..]
            : string.Empty;

        content = ExpandDetailsBlocks(content);
        content = ProcessWidgetDocsBlocks(content);
        content = CleanCodeBlockArguments(content);
        content = ProcessCustomBlocks(content);

        return Task.FromResult(content.Trim());
    }

    private static string ExpandDetailsBlocks(string markdown)
    {
        string result = markdown;
        int maxIterations = 100;
        int iteration = 0;

        while (DetailsBlockRegex.IsMatch(result) && iteration < maxIterations)
        {
            result = DetailsBlockRegex.Replace(result, match => ExpandSingleDetailsBlock(match.Value));
            iteration++;
        }

        return result;
    }

    private static string ExpandSingleDetailsBlock(string detailsHtml)
    {
        var sb = new StringBuilder();

        var summaryStartMatch = SummaryStartRegex.Match(detailsHtml);
        if (!summaryStartMatch.Success)
        {
            return detailsHtml
                .Replace("<Details>", "")
                .Replace("</Details>", "");
        }

        int summaryContentStart = summaryStartMatch.Index + summaryStartMatch.Length;
        int summaryEnd = detailsHtml.IndexOf("</Summary>", summaryContentStart, StringComparison.Ordinal);

        if (summaryEnd < 0)
        {
            return detailsHtml;
        }

        string summary = detailsHtml[summaryContentStart..summaryEnd].Trim();

        var bodyStartMatch = BodyStartRegex.Match(detailsHtml);
        if (!bodyStartMatch.Success)
        {
            sb.AppendLine();
            sb.AppendLine($"### {summary}");
            sb.AppendLine();
            return sb.ToString();
        }

        int bodyContentStart = bodyStartMatch.Index + bodyStartMatch.Length;
        int bodyEnd = detailsHtml.LastIndexOf("</Body>", StringComparison.Ordinal);

        if (bodyEnd < 0)
        {
            sb.AppendLine();
            sb.AppendLine($"### {summary}");
            sb.AppendLine();
            return sb.ToString();
        }

        string bodyContent = detailsHtml[bodyContentStart..bodyEnd].Trim();

        sb.AppendLine();
        sb.AppendLine($"### {summary}");
        sb.AppendLine();
        sb.AppendLine(bodyContent);
        sb.AppendLine();

        return sb.ToString();
    }

    private static List<WidgetDocsInfo> FindWidgetDocsBlocks(string markdown)
    {
        var results = new List<WidgetDocsInfo>();
        var matches = WidgetDocsRegex.Matches(markdown);

        foreach (Match match in matches)
        {
            string? typeName = ExtractAttribute(match.Value, "Type");
            if (string.IsNullOrEmpty(typeName))
            {
                Console.WriteLine($"Warning: WidgetDocs block missing Type attribute: {match.Value[..Math.Min(50, match.Value.Length)]}...");
                continue;
            }

            string? extensionTypes = ExtractAttribute(match.Value, "ExtensionTypes");
            string? sourceUrl = ExtractAttribute(match.Value, "SourceUrl");

            results.Add(new WidgetDocsInfo(typeName, extensionTypes, sourceUrl, match));
        }

        return results;
    }

    private static string? ExtractAttribute(string tag, string attributeName)
    {
        var regex = attributeName switch
        {
            "Type" => TypeAttributeRegex(),
            "ExtensionTypes" => ExtensionTypesAttributeRegex(),
            "SourceUrl" => SourceUrlAttributeRegex(),
            "Url" => UrlAttributeRegex(),
            _ => new Regex($@"{attributeName}\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase)
        };
        var match = regex.Match(tag);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string ProcessWidgetDocsBlocks(string markdown)
    {
        var widgetDocs = FindWidgetDocsBlocks(markdown);

        if (widgetDocs.Count == 0)
            return markdown;

        var result = markdown;

        foreach (var widgetDoc in widgetDocs.OrderByDescending(w => w.OriginalMatch.Index))
        {
            var apiDoc = ApiDocGenerator.GenerateApiDoc(
                widgetDoc.TypeName,
                widgetDoc.ExtensionTypes,
                widgetDoc.SourceUrl
            );

            result = result.Remove(widgetDoc.OriginalMatch.Index, widgetDoc.OriginalMatch.Length)
                          .Insert(widgetDoc.OriginalMatch.Index, apiDoc);
        }

        return result;
    }

    private static string CleanCodeBlockArguments(string markdown)
    {
        return CodeBlockWithDemoRegex().Replace(markdown, match =>
        {
            var lang = match.Groups[1].Value;
            return $"```{lang}";
        });
    }

    private static string ProcessCustomBlocks(string markdown)
    {
        var result = markdown;

        result = IngressBlockRegex().Replace(result, match =>
        {
            var content = match.Groups[1].Value.Trim();
            return $"*{content}*";
        });

        result = CalloutBlockRegex().Replace(result, match =>
        {
            var type = ExtractAttribute(match.Value, "Type") ?? "Note";
            var content = match.Groups[1].Value.Trim();
            return $"> **{type}:** {content}";
        });

        result = EmbedBlockRegex().Replace(result, match =>
        {
            var url = ExtractAttribute(match.Value, "Url");
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            return $"[View: {url}]({url})";
        });

        return result;
    }

    [GeneratedRegex(@"<Details>[\s\S]*?</Details>", RegexOptions.Compiled)]
    private static partial Regex DetailsRegex();

    [GeneratedRegex(@"<Summary[^>]*>", RegexOptions.Compiled)]
    private static partial Regex SummaryRegex();

    [GeneratedRegex(@"<Body[^>]*>", RegexOptions.Compiled)]
    private static partial Regex BodyRegex();

    [GeneratedRegex(@"<WidgetDocs\s+[^>]*/>", RegexOptions.Compiled)]
    private static partial Regex WidgetDocsBlockRegex();

    [GeneratedRegex(@"```(\w+)\s+demo-\w+(?:\s+demo-\w+)*", RegexOptions.Compiled)]
    private static partial Regex CodeBlockWithDemoRegex();

    [GeneratedRegex(@"<Ingress>\s*([\s\S]*?)\s*</Ingress>", RegexOptions.Compiled)]
    private static partial Regex IngressBlockRegex();

    [GeneratedRegex(@"<Callout[^>]*>\s*([\s\S]*?)\s*</Callout>", RegexOptions.Compiled)]
    private static partial Regex CalloutBlockRegex();

    [GeneratedRegex(@"<Embed\s+[^>]*/>", RegexOptions.Compiled)]
    private static partial Regex EmbedBlockRegex();

    [GeneratedRegex(@"Type\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TypeAttributeRegex();

    [GeneratedRegex(@"ExtensionTypes\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ExtensionTypesAttributeRegex();

    [GeneratedRegex(@"SourceUrl\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex SourceUrlAttributeRegex();

    [GeneratedRegex(@"Url\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex UrlAttributeRegex();
}
