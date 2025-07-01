﻿using System.Text;
using System.Xml.Linq;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Docs.Tools;

public static class MarkdownConverter
{
    public class AppMeta
    {
        public string? Icon { get; set; }
        public int Order { get; set; } = 0;
        public string? Title { get; set; }
        public string ViewBase { get; set; } = "ViewBase";
        public string? Prepare { get; set; } = "";
        public bool GroupExpanded { get; set; } = false;
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

        var document = Markdown.Parse(markdownContent, pipeline);

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
        HashSet<string> usedClassNames = new();
        HashSet<string> referencedApps = new();
        var linkConverter = new LinkConverter(relativePath);

        codeBuilder.AppendLine("using System;");
        codeBuilder.AppendLine("using Ivy;");
        codeBuilder.AppendLine("using Ivy.Apps;");
        codeBuilder.AppendLine("using Ivy.Shared;");
        codeBuilder.AppendLine("using Ivy.Core;");
        codeBuilder.AppendLine("using static Ivy.Helpers.Layout;");
        codeBuilder.AppendLine("using static Ivy.Helpers.Text;");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"namespace {@namespace};");
        codeBuilder.AppendLine();
        codeBuilder.Append($"[App(order:{appMeta.Order}");
        codeBuilder.Append(appMeta.Icon != null ? $", icon:Icons.{appMeta.Icon}" : "");
        codeBuilder.Append(appMeta.Title != null ? $", title:\"{appMeta.Title}\"" : "");
        codeBuilder.Append(appMeta.GroupExpanded ? ", groupExpanded:true" : "");
        codeBuilder.Append(documentSource != null ? $", documentSource:\"{documentSource}\"" : "");
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
        StringBuilder viewBuilder, HashSet<string> usedClassNames, HashSet<string> referencedApps, LinkConverter linkConverter)
    {
        var sectionBuilder = new StringBuilder();

        void WriteSection()
        {
            if (sectionBuilder.Length > 0)
            {
                var (types, convertedMarkdown) = linkConverter.Convert(sectionBuilder.ToString().Trim());
                referencedApps.UnionWith(types);
                AppendAsMultiLineStringIfNecessary(3, convertedMarkdown, codeBuilder, "| new Markdown(", ").HandleLinkClick(onLinkClick)");
                sectionBuilder.Clear();
            }
        }

        foreach (var child in document)
        {
            if (child is HtmlBlock htmlBlock)
            {
                WriteSection();
                HandleHtmlBlock(markdownContent, htmlBlock, codeBuilder);
            }

            else if (child is FencedCodeBlock codeBlock)
            {
                WriteSection();
                HandleCodeBlock(codeBlock, markdownContent, codeBuilder, viewBuilder, usedClassNames);
            }

            else if (child is HeadingBlock hBlock)
            {
                if (hBlock.Inline != null)
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
                string rawMarkdown = markdownContent.Substring(mBlock.Span.Start, mBlock.Span.Length).Trim();
                sectionBuilder.AppendLine().AppendLine(rawMarkdown);
            }
        }

        WriteSection();
    }

    private static void HandleHtmlBlock(string markdownContent, HtmlBlock htmlBlock, StringBuilder codeBuilder)
    {
        string htmlContent = markdownContent.Substring(htmlBlock.Span.Start, htmlBlock.Span.Length).Trim();
        var xml = XElement.Parse(htmlContent);

        if (xml.Name.LocalName == "Callout")
        {
            HandleCalloutBlock(codeBuilder, xml);
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
            HandleDetailsBlock(codeBuilder, xml);
        }
        else
        {
            throw new Exception($"Unknown HTML block: {xml.Name.LocalName}");
        }
    }

    private static void HandleDetailsBlock(StringBuilder codeBuilder, XElement xml)
    {
        string summary = xml.Element("Summary")?.Value ?? throw new Exception("Details block must have a Summary element.");
        string content = xml.Element("Body")?.Value.Trim() ?? throw new Exception("Details block must have a Body element.");
        AppendAsMultiLineStringIfNecessary(3, content, codeBuilder, $"""| new Expandable("{summary}", new Markdown(""", ").HandleLinkClick(onLinkClick))");
    }

    private static void HandleWidgetDocsBlock(StringBuilder codeBuilder, XElement xml)
    {
        string typeName = xml.Attribute("Type")?.Value ?? throw new Exception("WidgetDocs block must have a Type attribute.");
        string? extensionTypes = xml.Attribute("ExtensionTypes")?.Value;
        string? sourceUrl = xml.Attribute("SourceUrl")?.Value;
        codeBuilder.AppendTab(3).AppendLine($"""| new WidgetDocsView("{typeName}", {(!string.IsNullOrEmpty(extensionTypes) ? $"\"{extensionTypes}\"" : "null")}, {(!string.IsNullOrEmpty(sourceUrl) ? $"\"{sourceUrl}\"" : "null")})""");
    }

    private static void HandleCalloutBlock(StringBuilder codeBuilder, XElement xml)
    {
        string icon = xml.Attribute("Icon")?.Value ?? "Info";
        string content = xml.Value.Trim();
        AppendAsMultiLineStringIfNecessary(3, content, codeBuilder, "| new Callout(", $", icon:Icons.{icon})");
    }

    private static void HandleEmbedBlock(StringBuilder codeBuilder, XElement xml)
    {
        string url = xml.Attribute("Url")?.Value ?? throw new Exception("Embed block must have an Url attribute.");
        codeBuilder.AppendTab(3).AppendLine($"""| new Embed("{url}")""");
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
            _ => "Languages.Csharp"
        };
    }

    private static void HandleCodeBlock(FencedCodeBlock codeBlock, string markdownContent, StringBuilder codeBuilder,
        StringBuilder viewBuilder, HashSet<string> usedClassNames)
    {
        string language = codeBlock.Info ?? "csharp";
        string codeContent = markdownContent.Substring(codeBlock.Span.Start, codeBlock.Span.Length).Trim();
        codeContent = RemoveFirstAndLastLine(codeContent);
        if (language == "csharp" && (codeBlock.Arguments?.Trim().StartsWith("demo", StringComparison.InvariantCultureIgnoreCase) ?? false))
        {
            HandleDemoCodeBlock(codeBuilder, viewBuilder, codeContent, language, codeBlock.Arguments.Trim().ToLower(), usedClassNames);
        }
        else if (language == "terminal")
        {
            var lines = codeContent.Split('\n');
            codeBuilder.AppendTab(3).AppendLine("| new Terminal() ");
            foreach (var line in lines)
            {
                if (line.StartsWith('>'))
                {
                    codeBuilder.AppendTab(4).AppendLine($".AddCommand(\"{line.TrimStart('>').Trim()}\")");
                }
                else
                {
                    codeBuilder.AppendTab(4).AppendLine($".AddOutput(\"{line.Trim()}\")");
                }
            }
        }
        else
        {
            AppendAsMultiLineStringIfNecessary(3, codeContent, codeBuilder, "| Code(", $",{MapLanguageToEnum(language)})");
        }
    }

    private static void HandleDemoCodeBlock(StringBuilder codeBuilder, StringBuilder viewBuilder, string codeContent,
        string language, string arguments, HashSet<string> usedClassNames)
    {
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

        if (arguments is "demo") // just demo no code
        {
            codeBuilder.AppendTab(3).AppendLine($"| ({insertCode})");
        }
        else if (arguments is "demo-tabs")
        {
            codeBuilder.AppendTab(3).AppendLine("| Tabs( ");
            codeBuilder.AppendTab(4).AppendLine($"new Tab(\"Demo\", {insertCode}),");
            AppendAsMultiLineStringIfNecessary(4, codeContent, codeBuilder, "new Tab(\"Code\", new Code(", $",{MapLanguageToEnum(language)}))");
            codeBuilder.AppendTab(3).AppendLine(").Height(Size.Fit()).Padding(0, 8, 0, 0)");
        }
        else if (arguments is "demo-below")
        {
            codeBuilder.AppendTab(3).AppendLine("| (Vertical() ");
            AppendAsMultiLineStringIfNecessary(4, codeContent, codeBuilder, "| Code(", $",{MapLanguageToEnum(language)})");
            codeBuilder.AppendTab(4).AppendLine($"| ({insertCode})");
            codeBuilder.AppendTab(3).AppendLine(")");
        }
        else if (arguments is "demo-above")
        {
            codeBuilder.AppendTab(3).AppendLine("| (Vertical() ");
            codeBuilder.AppendTab(4).AppendLine($"| ({insertCode})");
            AppendAsMultiLineStringIfNecessary(4, codeContent, codeBuilder, "| Code(", $",{MapLanguageToEnum(language)})");
            codeBuilder.AppendTab(3).AppendLine(")");
        }
        else if (arguments is "demo-right")
        {
            codeBuilder.AppendTab(3).AppendLine("| (Grid().Columns(2) ");
            AppendAsMultiLineStringIfNecessary(4, codeContent, codeBuilder, "| Code(", $",{MapLanguageToEnum(language)})");
            codeBuilder.AppendTab(4).AppendLine($"| ({insertCode})");
            codeBuilder.AppendTab(3).AppendLine(")");
        }
        else if (arguments is "demo-left")
        {
            codeBuilder.AppendTab(3).AppendLine("| (Grid().Columns(2) ");
            codeBuilder.AppendTab(4).AppendLine($"| ({insertCode})");
            AppendAsMultiLineStringIfNecessary(4, codeContent, codeBuilder, "| Code(", $",{MapLanguageToEnum(language)})");
            codeBuilder.AppendTab(5).AppendLine(")");
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
            sb.AppendTab(tabs).AppendLine($"{prepend}\"{rawMarkdown}\"{append}");
        }
    }

    private static string RemoveFirstAndLastLine(string input) => string.Join(Environment.NewLine,
        input.Split('\n').Skip(1).SkipLast(1).Select(e => e.TrimEnd('\r')));
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