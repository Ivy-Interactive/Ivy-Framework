using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace Ivy.Docs.Tools;

/// <summary>
/// Represents a WidgetDocs block found in the markdown.
/// </summary>
/// <param name="TypeName">The full type name of the widget (e.g., "Ivy.BoolInput")</param>
/// <param name="ExtensionTypes">Semicolon-separated list of extension type names</param>
/// <param name="SourceUrl">URL to the source code on GitHub</param>
/// <param name="OriginalMatch">The original regex match for replacement</param>
public record WidgetDocsInfo(
    string TypeName,
    string? ExtensionTypes,
    string? SourceUrl,
    Match OriginalMatch
);

/// <summary>
/// Generates LLM-friendly markdown content from documentation files.
/// This includes expanding Details blocks, extracting tab contents,
/// and generating API documentation sections.
/// </summary>
public static partial class LlmMarkdownGenerator
{
    // Regex patterns for parsing Details blocks (same as MarkdownConverter)
    private static readonly Regex DetailsBlockRegex = DetailsRegex();
    private static readonly Regex SummaryStartRegex = SummaryRegex();
    private static readonly Regex BodyStartRegex = BodyRegex();

    // Regex pattern for WidgetDocs blocks
    private static readonly Regex WidgetDocsRegex = WidgetDocsBlockRegex();

    // Cached markdown pipeline (thread-safe, immutable after creation)
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UsePreciseSourceLocation()
        .UseYamlFrontMatter()
        .Build();

    /// <summary>
    /// Generates LLM-optimized markdown content from a source markdown file.
    /// </summary>
    /// <param name="sourceMarkdown">The original markdown content</param>
    /// <param name="filePath">Path to the source file (unused, kept for API compatibility)</param>
    /// <returns>Processed markdown suitable for LLM consumption</returns>
    public static Task<string> GenerateAsync(string sourceMarkdown, string filePath)
    {
        var pipeline = Pipeline;

        var document = Markdig.Markdown.Parse(sourceMarkdown, pipeline);

        // Find YAML front matter block to skip it
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        int contentStartIndex = 0;

        if (yamlBlock != null)
        {
            // Skip the YAML block - content starts after it
            contentStartIndex = yamlBlock.Span.End + 1;

            // Skip any leading newlines after YAML block
            while (contentStartIndex < sourceMarkdown.Length &&
                   (sourceMarkdown[contentStartIndex] == '\n' || sourceMarkdown[contentStartIndex] == '\r'))
            {
                contentStartIndex++;
            }
        }

        // Get content without YAML front matter
        string content = contentStartIndex < sourceMarkdown.Length
            ? sourceMarkdown[contentStartIndex..]
            : string.Empty;

        // Process Details blocks - expand them into regular markdown
        content = ExpandDetailsBlocks(content);

        // Process WidgetDocs blocks - generate API documentation
        content = ProcessWidgetDocsBlocks(content);

        // Clean up demo-* arguments from code blocks
        content = CleanCodeBlockArguments(content);

        // Process custom HTML blocks (Ingress, Callout, Embed)
        content = ProcessCustomBlocks(content);

        return Task.FromResult(content.Trim());
    }

    /// <summary>
    /// Expands all Details blocks into regular markdown sections.
    /// Recursively processes nested Details blocks.
    /// </summary>
    /// <param name="markdown">The markdown content to process</param>
    /// <returns>Markdown with Details blocks expanded</returns>
    private static string ExpandDetailsBlocks(string markdown)
    {
        // Process Details blocks from innermost to outermost (to handle nesting)
        // Keep processing until no more Details blocks are found
        string result = markdown;
        int maxIterations = 100; // Safety limit for deeply nested blocks
        int iteration = 0;

        while (DetailsBlockRegex.IsMatch(result) && iteration < maxIterations)
        {
            result = DetailsBlockRegex.Replace(result, match => ExpandSingleDetailsBlock(match.Value));
            iteration++;
        }

        return result;
    }

    /// <summary>
    /// Expands a single Details block into a markdown section.
    /// </summary>
    /// <param name="detailsHtml">The complete Details block HTML</param>
    /// <returns>Expanded markdown content</returns>
    private static string ExpandSingleDetailsBlock(string detailsHtml)
    {
        var sb = new StringBuilder();

        // Extract Summary content
        var summaryStartMatch = SummaryStartRegex.Match(detailsHtml);
        if (!summaryStartMatch.Success)
        {
            // No summary found, return original content without Details tags
            return detailsHtml
                .Replace("<Details>", "")
                .Replace("</Details>", "");
        }

        int summaryContentStart = summaryStartMatch.Index + summaryStartMatch.Length;
        int summaryEnd = detailsHtml.IndexOf("</Summary>", summaryContentStart, StringComparison.Ordinal);

        if (summaryEnd < 0)
        {
            // Malformed block, return as-is
            return detailsHtml;
        }

        string summary = detailsHtml[summaryContentStart..summaryEnd].Trim();

        // Extract Body content
        var bodyStartMatch = BodyStartRegex.Match(detailsHtml);
        if (!bodyStartMatch.Success)
        {
            // No body found, just return summary as heading
            sb.AppendLine();
            sb.AppendLine($"### {summary}");
            sb.AppendLine();
            return sb.ToString();
        }

        int bodyContentStart = bodyStartMatch.Index + bodyStartMatch.Length;
        int bodyEnd = detailsHtml.LastIndexOf("</Body>", StringComparison.Ordinal);

        if (bodyEnd < 0)
        {
            // Malformed block
            sb.AppendLine();
            sb.AppendLine($"### {summary}");
            sb.AppendLine();
            return sb.ToString();
        }

        string bodyContent = detailsHtml[bodyContentStart..bodyEnd].Trim();

        // Build expanded markdown section
        sb.AppendLine();
        sb.AppendLine($"### {summary}");
        sb.AppendLine();
        sb.AppendLine(bodyContent);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Finds all WidgetDocs blocks in the markdown content.
    /// </summary>
    /// <param name="markdown">The markdown content to search</param>
    /// <returns>List of WidgetDocsInfo with extracted attributes</returns>
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

    /// <summary>
    /// Extracts an attribute value from an XML-like tag.
    /// </summary>
    /// <param name="tag">The tag content (e.g., &lt;WidgetDocs Type="..." /&gt;)</param>
    /// <param name="attributeName">The attribute name to extract</param>
    /// <returns>The attribute value or null if not found</returns>
    private static string? ExtractAttribute(string tag, string attributeName)
    {
        // Use pre-compiled regex for common attributes
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

    /// <summary>
    /// Processes WidgetDocs blocks - generates API documentation sections.
    /// </summary>
    /// <param name="markdown">The markdown content</param>
    /// <returns>Markdown with WidgetDocs blocks replaced by API documentation</returns>
    private static string ProcessWidgetDocsBlocks(string markdown)
    {
        var widgetDocs = FindWidgetDocsBlocks(markdown);

        if (widgetDocs.Count == 0)
            return markdown;

        var result = markdown;

        // Process in reverse order to preserve string positions
        foreach (var widgetDoc in widgetDocs.OrderByDescending(w => w.OriginalMatch.Index))
        {
            // Generate API documentation using reflection
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

    /// <summary>
    /// Removes demo-* arguments from code block declarations.
    /// Converts "```csharp demo-below" to "```csharp"
    /// </summary>
    private static string CleanCodeBlockArguments(string markdown)
    {
        // Match code block opening with demo-* arguments
        return CodeBlockWithDemoRegex().Replace(markdown, match =>
        {
            var lang = match.Groups[1].Value;
            return $"```{lang}";
        });
    }

    /// <summary>
    /// Processes custom HTML blocks (Ingress, Callout, Embed) into plain markdown.
    /// </summary>
    private static string ProcessCustomBlocks(string markdown)
    {
        var result = markdown;

        // Process <Ingress>content</Ingress> -> just the content (as emphasis)
        result = IngressBlockRegex().Replace(result, match =>
        {
            var content = match.Groups[1].Value.Trim();
            return $"*{content}*";
        });

        // Process <Callout Type="...">content</Callout> -> blockquote
        result = CalloutBlockRegex().Replace(result, match =>
        {
            var type = ExtractAttribute(match.Value, "Type") ?? "Note";
            var content = match.Groups[1].Value.Trim();
            return $"> **{type}:** {content}";
        });

        // Process <Embed Url="..."/> -> markdown link
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

    // Attribute extraction regexes (pre-compiled for performance)
    [GeneratedRegex(@"Type\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TypeAttributeRegex();

    [GeneratedRegex(@"ExtensionTypes\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ExtensionTypesAttributeRegex();

    [GeneratedRegex(@"SourceUrl\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex SourceUrlAttributeRegex();

    [GeneratedRegex(@"Url\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex UrlAttributeRegex();
}
