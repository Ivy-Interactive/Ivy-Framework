using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console.Cli;

namespace Ivy.Docs.Tools;

public class GenerateApiDocsCommand : AsyncCommand<GenerateApiDocsCommand.Settings>
{
    private static readonly Regex WidgetDocsRegex = new(@"<WidgetDocs\s+[^>]*/>", RegexOptions.Compiled);
    private static readonly Regex TypeAttrRegex = new(@"Type\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ExtensionTypesAttrRegex = new(@"ExtensionTypes\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SourceUrlAttrRegex = new(@"SourceUrl\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<inputFolder>")]
        [Description("The input folder containing markdown files (supports glob patterns).")]
        public required string InputFolder { get; set; }

        [CommandArgument(1, "<outputFile>")]
        [Description("The output JSON file path for the API docs manifest.")]
        public required string OutputFile { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var inputFolder = settings.InputFolder;
        var pattern = "*.md";

        if (inputFolder.Contains('*') || inputFolder.Contains('?'))
        {
            pattern = Path.GetFileName(inputFolder);
            inputFolder = Path.GetDirectoryName(inputFolder)!;
        }

        inputFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, inputFolder));
        var outputFile = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, settings.OutputFile));

        var manifest = new Dictionary<string, string>();

        var files = Directory.GetFiles(inputFolder, pattern, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            var matches = WidgetDocsRegex.Matches(content);

            foreach (Match match in matches)
            {
                var tag = match.Value;
                var typeName = ExtractAttr(TypeAttrRegex, tag);
                if (string.IsNullOrEmpty(typeName))
                    continue;

                var extensionTypes = ExtractAttr(ExtensionTypesAttrRegex, tag);
                var sourceUrl = ExtractAttr(SourceUrlAttrRegex, tag);

                var key = BuildKey(typeName, extensionTypes, sourceUrl);

                if (manifest.ContainsKey(key))
                    continue;

                var apiDoc = ApiDocGenerator.GenerateApiDoc(typeName, extensionTypes, sourceUrl);
                manifest[key] = apiDoc;
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(outputFile, json, cancellationToken);

        Console.WriteLine($"Generated API docs manifest with {manifest.Count} entries at {outputFile}");

        return 0;
    }

    public static string BuildKey(string typeName, string? extensionTypes, string? sourceUrl)
    {
        return $"{typeName}|{extensionTypes ?? ""}|{sourceUrl ?? ""}";
    }

    private static string? ExtractAttr(Regex regex, string tag)
    {
        var match = regex.Match(tag);
        return match.Success ? match.Groups[1].Value : null;
    }
}
