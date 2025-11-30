using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Docs.Tools;

public static partial class MarkdownConverter
{
    // Compiled regex patterns for better performance
    private static readonly Regex DetailsBlockRegex = DetailsRegex();
    private static readonly Regex SummaryStartRegex = SummaryRegex();
    private static readonly Regex BodyStartRegex = BodyRegex();

    public class AppMeta
    {
        public string? Icon { get; set; }
        public int Order { get; set; } = 0;
        public string? Title { get; set; }
        public string ViewBase { get; set; } = "ViewBase";
        public string? Prepare { get; set; } = "";
        public bool GroupExpanded { get; set; } = false;
        public List<string>? SearchHints { get; set; }
        public List<string>? Imports { get; set; }
    }

    static AppMeta ParseYamlAppMeta(string yaml)
    {
        string withoutDashes = RemoveFirstAndLastLine(yaml);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<AppMeta>(withoutDashes);
    }

    public static async Task ConvertAsync(string name, string relativePath, string absolutePath, string outputFile, string @namespace, bool skipIfNotChanged,
        int? order)
    {
        string className = name + "App";

        string markdownContent = await File.ReadAllTextAsync(absolutePath);

        string hash = Utils.GetShortHash(markdownContent);

        if (File.Exists(outputFile) && skipIfNotChanged)
        {
            var oldHash = FileHashMetadata.ReadHash(outputFile);
            if (oldHash != null && oldHash == hash)
            {
                Console.WriteLine("Skipping {0}", absolutePath);
                return;
            }
        }

        Console.WriteLine("Converting {0} to {1}", absolutePath, outputFile);

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePreciseSourceLocation()
            .UseYamlFrontMatter()
            .Build();

        var document = Markdig.Markdown.Parse(markdownContent, pipeline);

        var documentSource = Utils.GetGitFileUrl(absolutePath);

        AppMeta appMeta = new();

        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (yamlBlock != null)
        {
            string yamlContent = markdownContent.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);
            appMeta = ParseYamlAppMeta(yamlContent);
        }

        if (order != null)
        {
            appMeta.Order = order.Value;
        }

        StringBuilder codeBuilder = new();
        StringBuilder viewBuilder = new();
        HashSet<string> usedClassNames = [];
        HashSet<string> referencedApps = [];
        var linkConverter = new LinkConverter(relativePath);

        codeBuilder.AppendLine("using System;");
        codeBuilder.AppendLine("using Ivy;");
        codeBuilder.AppendLine("using Ivy.Apps;");
        codeBuilder.AppendLine("using Ivy.Shared;");
        codeBuilder.AppendLine("using Ivy.Core;");
        codeBuilder.AppendLine("using Ivy.Views.Tables;");
        codeBuilder.AppendLine("using Ivy.Views.Kanban;");
        codeBuilder.AppendLine("using static Ivy.Views.Layout;");
        codeBuilder.AppendLine("using static Ivy.Views.Text;");
        codeBuilder.AppendLine("using Ivy.Docs.Tools;");
        if (appMeta.Imports != null)
        {
            foreach (var import in appMeta.Imports)
            {
                codeBuilder.AppendLine($"using {import};");
            }
        }
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"namespace {@namespace};");
        codeBuilder.AppendLine();
        codeBuilder.Append($"[App(order:{appMeta.Order}");
        codeBuilder.Append(appMeta.Icon != null ? $", icon:Icons.{appMeta.Icon}" : "");
        codeBuilder.Append(appMeta.Title != null ? $", title:{FormatLiteral(appMeta.Title)}" : "");
        codeBuilder.Append(appMeta.GroupExpanded ? ", groupExpanded:true" : "");
        codeBuilder.Append(documentSource != null ? $", documentSource:{FormatLiteral(documentSource)}" : "");
        if (appMeta.SearchHints != null && appMeta.SearchHints.Count > 0)
        {
            var hints = string.Join(", ", appMeta.SearchHints.Select(h => FormatLiteral(h)));
            codeBuilder.Append($", searchHints: [{hints}]");
        }
        codeBuilder.AppendLine(")]");
        codeBuilder.AppendLine($"public class {className}(bool onlyBody = false) : {appMeta.ViewBase}");
        codeBuilder.AppendLine("{");
        codeBuilder.AppendTab(1).AppendLine($"public {className}() : this(false)");
        codeBuilder.AppendTab(1).AppendLine("{");
        codeBuilder.AppendTab(1).AppendLine("}");
        codeBuilder.AppendTab(1).AppendLine("public override object? Build()");
        codeBuilder.AppendTab(1).AppendLine("{");
        if (!string.IsNullOrEmpty(appMeta.Prepare))
        {
            var prepareLines = appMeta.Prepare.Split('\n');
            foreach (var line in prepareLines)
            {
                codeBuilder.AppendTab(2).AppendLine(line.Trim());
            }
        }

        if (document.Any(e => e is not YamlFrontMatterBlock))
        {
            codeBuilder.AppendTab(2).AppendLine("var appDescriptor = this.UseService<AppDescriptor>();");
            codeBuilder.AppendTab(2).AppendLine("var onLinkClick = this.UseLinks();");
            codeBuilder.AppendTab(2).AppendLine("var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).HandleLinkClick(onLinkClick)");

            HandleBlocks(document, codeBuilder, markdownContent, viewBuilder, usedClassNames, referencedApps, linkConverter);

            codeBuilder.AppendTab(3).AppendLine(";");

            if (referencedApps.Count > 0)
            {
                codeBuilder.AppendTab(2).AppendLine("// Build errors here indicates that one or more referenced apps don't exist. Check markdown links.");
                codeBuilder.AppendTab(2).Append("Type[] _ = [").Append(string.Join(", ", referencedApps.Select(e => "typeof(" + e + ")").ToArray())).AppendLine("]; ");
            }

            codeBuilder.AppendTab(2).AppendLine("return article;");
        }
        else
        {
            codeBuilder.AppendTab(2).AppendLine("return null;");
        }

        codeBuilder.AppendTab().AppendLine("}");
        codeBuilder.AppendTab(0).AppendLine("}");

        codeBuilder.AppendLine(viewBuilder.ToString());

        await using (var stream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
        await using (var writer = new StreamWriter(stream))
        {
            await writer.WriteAsync(codeBuilder.ToString());
        }

        FileHashMetadata.WriteHash(outputFile, hash);
    }

    private static void HandleBlocks(MarkdownDocument document, StringBuilder codeBuilder, string markdownContent,
        StringBuilder viewBuilder, HashSet<string> usedClassNames, HashSet<string> referencedApps, LinkConverter linkConverter, bool isNestedContent = false, int baseIndentLevel = 3)
    {
        var sectionBuilder = new StringBuilder();

        void WriteSection()
        {
            if (sectionBuilder.Length > 0)
            {
                var (types, convertedMarkdown) = linkConverter.Convert(sectionBuilder.ToString().Trim());
                referencedApps.UnionWith(types);
                AppendAsMultiLineStringIfNecessary(baseIndentLevel, convertedMarkdown, codeBuilder,
                    isNestedContent ? ", new Markdown(" : "| new Markdown(",
                    ").HandleLinkClick(onLinkClick)");
                sectionBuilder.Clear();
            }
        }

        // Pre-process to find and handle Details blocks manually since Markdig doesn't parse them correctly
        var detailsMatches = DetailsBlockRegex.Matches(markdownContent);
        var processedDetailsRanges = new List<(int start, int end)>();

        foreach (Match match in detailsMatches)
        {
            processedDetailsRanges.Add((match.Index, match.Index + match.Length));
        }

        foreach (var child in document)
        {
            if (child is HtmlBlock htmlBlock)
            {
                WriteSection();

                string htmlContent = markdownContent.Substring(htmlBlock.Span.Start, htmlBlock.Span.Length).Trim();

                // Check if this is a Details block that was incompletely parsed
                if (htmlContent.StartsWith("<Details>") && !htmlContent.EndsWith("</Details>"))
                {
                    // Find the complete Details block
                    var detailsMatch = detailsMatches.Cast<Match>()
                        .FirstOrDefault(m => m.Index == htmlBlock.Span.Start);

                    if (detailsMatch != null)
                    {
                        // Handle the complete Details block directly
                        HandleDetailsBlockDirect(codeBuilder, detailsMatch.Value, viewBuilder, usedClassNames);
                        continue;
                    }
                }

                // Skip if this HTML block is inside a Details block (but not the Details tag itself)
                bool isInsideDetailsBlock = processedDetailsRanges.Any(range =>
                    htmlBlock.Span.Start > range.start && htmlBlock.Span.Start < range.end);

                if (isInsideDetailsBlock)
                {
                    continue;
                }

                HandleHtmlBlock(markdownContent, htmlBlock, codeBuilder, viewBuilder, usedClassNames, linkConverter, referencedApps);
            }

            else if (child is FencedCodeBlock codeBlock)
            {
                // Skip if this code block is inside a Details block
                bool isInsideDetailsBlock = processedDetailsRanges.Any(range =>
                    codeBlock.Span.Start >= range.start && codeBlock.Span.Start < range.end);

                if (!isInsideDetailsBlock)
                {
                    WriteSection();
                    HandleCodeBlock(codeBlock, markdownContent, codeBuilder, viewBuilder, usedClassNames, isNestedContent, baseIndentLevel);
                }
            }

            else if (child is HeadingBlock hBlock)
            {
                // Skip if this heading is inside a Details block
                bool isInsideDetailsBlock = processedDetailsRanges.Any(range =>
                    hBlock.Span.Start >= range.start && hBlock.Span.Start < range.end);

                if (!isInsideDetailsBlock && hBlock.Inline != null)
                {
                    var headingText = new StringBuilder();
                    foreach (var inline in hBlock.Inline.Descendants())
                    {
                        if (inline is LiteralInline literal)
                        {
                            headingText.Append(literal.Content.ToString());
                        }
                    }

                    sectionBuilder.AppendLine();
                    sectionBuilder.AppendLine($"{new string('#', hBlock.Level)} {headingText.ToString().Trim()}");
                }
            }

            else if (child is not YamlFrontMatterBlock && child is MarkdownObject mBlock)
            {
                // Skip if this block is inside a Details block
                bool isInsideDetailsBlock = processedDetailsRanges.Any(range =>
                    mBlock.Span.Start >= range.start && mBlock.Span.Start < range.end);

                if (!isInsideDetailsBlock)
                {
                    string rawMarkdown = markdownContent.Substring(mBlock.Span.Start, mBlock.Span.Length).Trim();
                    sectionBuilder.AppendLine().AppendLine(rawMarkdown);
                }
            }
        }

        // Only write final section if there's actual content
        if (sectionBuilder.Length > 0 && !string.IsNullOrWhiteSpace(sectionBuilder.ToString()))
        {
            WriteSection();
        }
    }

    private static void HandleHtmlBlock(string markdownContent, HtmlBlock htmlBlock, StringBuilder codeBuilder, StringBuilder viewBuilder, HashSet<string> usedClassNames, LinkConverter linkConverter, HashSet<string> referencedApps)
    {
        string htmlContent = markdownContent.Substring(htmlBlock.Span.Start, htmlBlock.Span.Length).Trim();

        // Check if it's a Details block first (before XML parsing since it may contain markdown)
        // Note: Must be case-sensitive to distinguish from HTML <details> element
        if (htmlContent.StartsWith("<Details>"))
        {
            HandleDetailsBlock(codeBuilder, null, markdownContent, htmlBlock, viewBuilder, usedClassNames);
            return;
        }

        // Skip standard HTML elements that aren't our custom blocks
        if (htmlContent.StartsWith("<details>", StringComparison.OrdinalIgnoreCase) ||
            htmlContent.StartsWith("</", StringComparison.OrdinalIgnoreCase))
        {
            // This is a standard HTML element or closing tag, skip it
            return;
        }

        XElement xml;
        try
        {
            xml = XElement.Parse(htmlContent);
        }
        catch (System.Xml.XmlException)
        {
            // If it's not valid XML, skip it (probably a standard HTML element)
            Console.WriteLine($"Skipping non-XML HTML block: {htmlContent[..Math.Min(50, htmlContent.Length)]}...");
            return;
        }

        if (xml.Name.LocalName == "Callout")
        {
            HandleCalloutBlock(codeBuilder, xml, linkConverter, referencedApps);
        }
        else if (xml.Name.LocalName == "Embed")
        {
            HandleEmbedBlock(codeBuilder, xml);
        }
        else if (xml.Name.LocalName == "WidgetDocs")
        {
            HandleWidgetDocsBlock(codeBuilder, xml);
        }
        else if (xml.Name.LocalName == "Details")
        {
            HandleDetailsBlock(codeBuilder, xml, markdownContent, htmlBlock, viewBuilder, usedClassNames);
        }
        else if (xml.Name.LocalName == "Ingress")
        {
            HandleIngressBlock(codeBuilder, xml);
        }
        else
        {
            throw new Exception($"Unknown HTML block: {xml.Name.LocalName}");
        }
    }

    private static void HandleDetailsBlock(StringBuilder codeBuilder, XElement? xml, string markdownContent, HtmlBlock htmlBlock, StringBuilder viewBuilder, HashSet<string> usedClassNames)
    {
        // Get the raw HTML content
        string htmlContent = markdownContent.Substring(htmlBlock.Span.Start, htmlBlock.Span.Length);
        HandleDetailsBlockDirect(codeBuilder, htmlContent, viewBuilder, usedClassNames);
    }

    private static void HandleDetailsBlockDirect(StringBuilder codeBuilder, string htmlContent, StringBuilder viewBuilder, HashSet<string> usedClassNames)
    {
        // Extract Summary content
        var summaryStartMatch = SummaryStartRegex.Match(htmlContent);
        if (!summaryStartMatch.Success)
            throw new Exception($"Details block missing <Summary> tag. Block starts with: {htmlContent.Substring(0, Math.Min(50, htmlContent.Length))}...");

        int summaryContentStart = summaryStartMatch.Index + summaryStartMatch.Length;
        int summaryEnd = htmlContent.IndexOf("</Summary>", summaryContentStart);
        if (summaryEnd < 0)
            throw new Exception($"Details block missing closing </Summary> tag at position {summaryContentStart}. Content after <Summary>: {htmlContent.Substring(summaryContentStart, Math.Min(50, htmlContent.Length - summaryContentStart))}...");

        string summary = htmlContent[summaryContentStart..summaryEnd].Trim();

        // Find Body opening tag (could be <Body> or <Body attribute="value"> etc.)
        var bodyStartMatch = BodyStartRegex.Match(htmlContent);
        if (!bodyStartMatch.Success)
            throw new Exception($"Details block missing <Body> tag. Block content: {htmlContent.Substring(0, Math.Min(100, htmlContent.Length))}...");

        int bodyContentStart = bodyStartMatch.Index + bodyStartMatch.Length;

        // Find closing </Body> tag
        int bodyEnd = htmlContent.LastIndexOf("</Body>");
        if (bodyEnd < 0)
            throw new Exception($"Details block missing closing </Body> tag. Body content starts at position {bodyContentStart}. Content: {htmlContent.Substring(bodyContentStart, Math.Min(50, htmlContent.Length - bodyContentStart))}...");

        string bodyContent = htmlContent[bodyContentStart..bodyEnd].Trim();

        // Process the body content through the full markdown pipeline
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePreciseSourceLocation()
            .Build();

        var bodyDocument = Markdig.Markdown.Parse(bodyContent, pipeline);

        // Create a temporary builder for the body content
        var bodyCodeBuilder = new StringBuilder();
        var bodyReferencedApps = new HashSet<string>();
        var bodyLinkConverter = new LinkConverter("");

        // Process the body through HandleBlocks
        HandleBlocks(bodyDocument, bodyCodeBuilder, bodyContent, viewBuilder, usedClassNames, bodyReferencedApps, bodyLinkConverter, false, 4);

        // Get the generated body content
        var bodyOutput = bodyCodeBuilder.ToString().TrimEnd();

        if (!string.IsNullOrWhiteSpace(bodyOutput))
        {
            // Check if we have content that needs to be wrapped in Vertical()
            var lines = bodyOutput.Split('\n');
            var hasMultipleItems = lines.Count(l => l.TrimStart().StartsWith("| ")) > 1;

            if (hasMultipleItems)
            {
                // Multiple items - wrap in Vertical()
                codeBuilder.AppendTab(3).AppendLine($"""| new Expandable("{summary}",""");
                codeBuilder.AppendTab(4).AppendLine("Vertical()");
                codeBuilder.Append(bodyOutput);
                codeBuilder.AppendLine();
                codeBuilder.AppendTab(3).AppendLine(")");
            }
            else
            {
                // Single item - use directly without Vertical()
                // Remove the leading pipe from the single item
                var singleItemContent = bodyOutput.TrimStart();
                if (singleItemContent.StartsWith("| "))
                {
                    singleItemContent = singleItemContent[2..];
                }
                codeBuilder.AppendTab(3).AppendLine($"""| new Expandable("{summary}",""");
                codeBuilder.AppendTab(4).AppendLine(singleItemContent);
                codeBuilder.AppendTab(3).AppendLine(")");
            }
        }
        else
        {
            codeBuilder.AppendTab(3).AppendLine($"""| new Expandable("{summary}", new Markdown("No content"))""");
        }
    }

    private static void HandleWidgetDocsBlock(StringBuilder codeBuilder, XElement xml)
    {
        string typeName = xml.Attribute("Type")?.Value ?? throw new Exception("WidgetDocs block must have a Type attribute.");
        string? extensionTypes = xml.Attribute("ExtensionTypes")?.Value;
        string? sourceUrl = xml.Attribute("SourceUrl")?.Value;
        codeBuilder.AppendTab(3).AppendLine($"""| new WidgetDocsView("{typeName}", {(!string.IsNullOrEmpty(extensionTypes) ? FormatLiteral(extensionTypes) : "null")}, {(!string.IsNullOrEmpty(sourceUrl) ? FormatLiteral(sourceUrl) : "null")})""");
    }

    private static void HandleCalloutBlock(StringBuilder codeBuilder, XElement xml, LinkConverter linkConverter, HashSet<string> referencedApps)
    {
        // Handle Type attribute (tip, info, warning, error) - map to icon
        string? type = xml.Attribute("Type")?.Value;
        string icon = xml.Attribute("Icon")?.Value ??
            (type?.ToLowerInvariant() switch
            {
                "tip" or "info" => "Info",
                "warning" => "CircleAlert",
                "error" => "CircleAlert",
                "success" => "CircleCheck",
                _ => "Info"
            });
        string content = xml.Value.Trim();

        // Convert links in the Callout content to app:// format
        var (types, convertedContent) = linkConverter.Convert(content);
        referencedApps.UnionWith(types);

        AppendAsMultiLineStringIfNecessary(3, convertedContent, codeBuilder, "| new Callout(", $", icon:Icons.{icon}).HandleLinkClick(onLinkClick)");
    }

    private static void HandleEmbedBlock(StringBuilder codeBuilder, XElement xml)
    {
        string url = xml.Attribute("Url")?.Value ?? throw new Exception("Embed block must have an Url attribute.");
        codeBuilder.AppendTab(3).AppendLine($"""| new Embed("{url}")""");
    }

    private static void HandleIngressBlock(StringBuilder codeBuilder, XElement xml)
    {
        string content = xml.Value.Trim();
        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("Ingress block must have content.");
        }
        AppendAsMultiLineStringIfNecessary(3, content, codeBuilder, "| Lead(", ")");
    }

    private static string MapLanguageToEnum(string lang)
    {
        return lang.ToLowerInvariant() switch
        {
            "csharp" or "cs" => "Languages.Csharp",
            "javascript" or "js" => "Languages.Javascript",
            "typescript" or "ts" => "Languages.Typescript",
            "python" => "Languages.Python",
            "sql" => "Languages.Sql",
            "html" => "Languages.Html",
            "css" => "Languages.Css",
            "json" => "Languages.Json",
            "dbml" => "Languages.Dbml",
            "text" => "Languages.Text",
            _ => "Languages.Text"
        };
    }

    private static void HandleCodeBlock(FencedCodeBlock codeBlock, string markdownContent, StringBuilder codeBuilder,
StringBuilder viewBuilder, HashSet<string> usedClassNames, bool isNestedContent = false, int baseIndentLevel = 3)
    {
        string language = codeBlock.Info ?? "csharp";
        string codeContent = markdownContent.Substring(codeBlock.Span.Start, codeBlock.Span.Length).Trim();
        codeContent = RemoveFirstAndLastLine(codeContent);
        if (language == "csharp" && (codeBlock.Arguments?.Trim().StartsWith("demo", StringComparison.InvariantCultureIgnoreCase) ?? false))
        {
            HandleDemoCodeBlock(codeBuilder, viewBuilder, codeContent, language, codeBlock.Arguments.Trim().ToLower(), usedClassNames, isNestedContent, baseIndentLevel);
        }
        else if (language == "terminal")
        {
            var lines = codeContent.Split('\n');
            codeBuilder.AppendTab(baseIndentLevel).AppendLine((isNestedContent ? ", " : "| ") + "new Terminal() ");
            foreach (var line in lines)
            {
                if (line.StartsWith('>'))
                {
                    codeBuilder.AppendTab(baseIndentLevel + 1).AppendLine($".AddCommand({FormatLiteral(line.TrimStart('>').Trim())})");
                }
                else
                {
                    codeBuilder.AppendTab(baseIndentLevel + 1).AppendLine($".AddOutput({FormatLiteral(line.Trim())})");
                }
            }
        }
        else if (language == "mermaid")
        {
            // Handle Mermaid diagrams by wrapping them in Markdown widget with proper syntax
            string mermaidBlock = $"```mermaid\n{codeContent}\n```";
            AppendAsMultiLineStringIfNecessary(baseIndentLevel, mermaidBlock, codeBuilder,
                isNestedContent ? ", new Markdown(" : "| new Markdown(",
                ").HandleLinkClick(onLinkClick)");
        }
        else
        {
            AppendAsMultiLineStringIfNecessary(baseIndentLevel, codeContent, codeBuilder,
                isNestedContent ? ", Code(" : "| Code(",
                $",{MapLanguageToEnum(language)})");
        }
    }

    private static void HandleDemoCodeBlock(StringBuilder codeBuilder, StringBuilder viewBuilder, string codeContent,
        string language, string arguments, HashSet<string> usedClassNames, bool isNestedContent = false, int baseIndentLevel = 3)
    {
        // Local helpers to reduce duplication and improve readability
        static string ParseDemoArgs(string args)
        {
            var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var layout = parts.Length > 0 ? parts[0] : "demo";
            return layout;
        }

        void AppendDemoContent(StringBuilder cb, int tabs, string insert)
        {
            cb.AppendTab(tabs).AppendLine($"{(isNestedContent ? ", " : "| ")}new Box().Plain().Content({insert})");
        }

        void AppendTabbedDemo(StringBuilder cb, string code, string insert, string lang)
        {
            cb.AppendTab(baseIndentLevel).AppendLine((isNestedContent ? ", " : "| ") + "Tabs( ");
            cb.AppendTab(baseIndentLevel + 1).AppendLine($"new Tab(\"Demo\", new Box().Plain().Content({insert})),");
            AppendAsMultiLineStringIfNecessary(baseIndentLevel + 1, code, cb, "new Tab(\"Code\", new Code(", $",{MapLanguageToEnum(lang)}))")
                ;
            cb.AppendTab(baseIndentLevel).AppendLine(").Height(Size.Fit()).Padding(0, 8, 0, 0).Variant(TabsVariant.Content)");
        }

        void AppendVerticalDemo(StringBuilder cb, string code, string insert, string lang, bool demoBelow)
        {
            cb.AppendTab(baseIndentLevel).AppendLine((isNestedContent ? ", " : "| ") + "(Vertical() ");
            if (!demoBelow) AppendDemoContent(cb, baseIndentLevel + 1, insert);
            AppendAsMultiLineStringIfNecessary(baseIndentLevel + 1, code, cb, "| Code(", $",{MapLanguageToEnum(lang)})");
            if (demoBelow) AppendDemoContent(cb, baseIndentLevel + 1, insert);
            cb.AppendTab(baseIndentLevel).AppendLine(")");
        }

        void AppendGridDemo(StringBuilder cb, string code, string insert, string lang, bool demoRight)
        {
            cb.AppendTab(baseIndentLevel).AppendLine((isNestedContent ? ", " : "| ") + "(Grid().Columns(2) ");
            if (!demoRight) AppendDemoContent(cb, baseIndentLevel + 1, insert);
            AppendAsMultiLineStringIfNecessary(baseIndentLevel + 1, code, cb, "| Code(", $",{MapLanguageToEnum(lang)})");
            if (demoRight) AppendDemoContent(cb, baseIndentLevel + 1, insert);
            cb.AppendTab(baseIndentLevel).AppendLine(")");
        }

        // Build insert code and include view class when needed
        string insertCode;
        if (Utils.IsView(codeContent, out string? className))
        {
            var unusedClassName = GetUnusedClassName(className!, usedClassNames);
            if (unusedClassName != className)
            {
                codeContent = Utils.RenameClass(codeContent, unusedClassName);
                className = unusedClassName;
                usedClassNames.Add(unusedClassName);
            }
            else
            {
                usedClassNames.Add(className);
            }
            viewBuilder.AppendLine().AppendLine().Append(codeContent);
            insertCode = $"new {className}()";
        }
        else
        {
            insertCode = codeContent;
        }

        var layout = ParseDemoArgs(arguments);
        switch (layout)
        {
            case "demo":
                AppendDemoContent(codeBuilder, baseIndentLevel, insertCode);
                break;
            case "demo-tabs":
                AppendTabbedDemo(codeBuilder, codeContent, insertCode, language);
                break;
            case "demo-below":
                AppendVerticalDemo(codeBuilder, codeContent, insertCode, language, demoBelow: true);
                break;
            case "demo-above":
                AppendVerticalDemo(codeBuilder, codeContent, insertCode, language, demoBelow: false);
                break;
            case "demo-right":
                AppendGridDemo(codeBuilder, codeContent, insertCode, language, demoRight: true);
                break;
            case "demo-left":
                AppendGridDemo(codeBuilder, codeContent, insertCode, language, demoRight: false);
                break;
            default:
                // Fallback to simple demo
                AppendDemoContent(codeBuilder, baseIndentLevel, insertCode);
                break;
        }
    }

    private static string GetUnusedClassName(string className, HashSet<string> usedClassNames)
    {
        if (usedClassNames.Contains(className))
        {
            int i = 1;
            while (usedClassNames.Contains(className + i))
            {
                i++;
            }
            return className + i;
        }
        return className;
    }

    private static void AppendAsMultiLineStringIfNecessary(int tabs, string rawMarkdown, StringBuilder sb, string prepend, string append)
    {
        if (rawMarkdown.Contains('\n') || rawMarkdown.Contains('"'))
        {
            var lines = rawMarkdown.Split('\n');
            sb.AppendTab(tabs).AppendLine(prepend);
            sb.AppendTab(tabs + 1).AppendLine("\"\"\"\"");
            foreach (var line in lines)
            {
                sb.AppendTab(tabs + 1).AppendLine(line.TrimEnd());
            }
            sb.AppendTab(tabs + 1).AppendLine($"\"\"\"\"{append}");
        }
        else
        {
            sb.AppendTab(tabs).AppendLine($"{prepend}{FormatLiteral(rawMarkdown)}{append}");
        }
    }

    private static string FormatLiteral(string literal) => SymbolDisplay.FormatLiteral(literal, true);

    private static string RemoveFirstAndLastLine(string input) => string.Join(Environment.NewLine,
        input.Split('\n').Skip(1).SkipLast(1).Select(e => e.TrimEnd('\r')));
    [GeneratedRegex(@"<Details>[\s\S]*?</Details>", RegexOptions.Compiled)]
    private static partial Regex DetailsRegex();
    [GeneratedRegex(@"<Summary[^>]*>", RegexOptions.Compiled)]
    private static partial Regex SummaryRegex();
    [GeneratedRegex(@"<Body[^>]*>", RegexOptions.Compiled)]
    private static partial Regex BodyRegex();
}

public static class StringBuilderExtensions
{
    public static StringBuilder AppendTab(this StringBuilder sb, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            sb.Append("    ");
        }
        return sb;
    }
}