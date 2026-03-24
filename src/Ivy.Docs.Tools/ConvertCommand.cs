using System.ComponentModel;
using System.Xml.Linq;
using Spectre.Console.Cli;

namespace Ivy.Docs.Tools;

public class ConvertCommand : AsyncCommand<ConvertCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<inputFolder>")]
        [Description("The input folder containing markdown files.")]
        public required string InputFolder { get; set; }

        [CommandArgument(1, "<outputFolder>")]
        [Description("The output folder for generated C# files.")]
        public required string OutputFolder { get; set; }

        [CommandOption("--skip-if-not-changed")]
        public required bool SkipIfNotChanged { get; init; } = false;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var pattern = Path.GetFileName(settings.InputFolder);

        var inputFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(settings.InputFolder)!));
        var outputFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, settings.OutputFolder));

        Directory.CreateDirectory(outputFolder);

        var projectFile = GetProjectFile(inputFolder);
        var rootNamespace = GetRootNamespace(projectFile);

        var tasks = Directory.GetFiles(inputFolder, pattern, SearchOption.AllDirectories).Select(async absoluteInputPath =>
        {
            var (order, name) = Utils.GetOrderFromFileName(absoluteInputPath);

            if (name == "_Index")
            {
                (order, _) = Utils.GetOrderFromFileName(Path.GetFileName(Path.GetDirectoryName(absoluteInputPath))!);
            }
            string relativeInputPath = Path.GetRelativePath(inputFolder, absoluteInputPath);
            string relativeOutputPath = Utils.GetRelativeFolderWithoutOrder(inputFolder, absoluteInputPath);

            string folder = Path.GetFullPath(Path.Combine(outputFolder, relativeOutputPath));

            Directory.CreateDirectory(folder);

            string ivyOutput = Path.Combine(folder, $"{name}.g.cs");
            string llmMarkdownOutput = Path.Combine(folder, $"{name}.md");

            var namespaceSuffix = relativeOutputPath
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.').Trim('.');

            if (namespaceSuffix.StartsWith("Generated."))
                namespaceSuffix = namespaceSuffix.Substring("Generated.".Length);

            string @namespace = string.IsNullOrEmpty(namespaceSuffix)
                ? $"{rootNamespace}.Apps"
                : $"{rootNamespace}.Apps.{namespaceSuffix}";

            await MarkdownConverter.ConvertAsync(name, relativeInputPath, absoluteInputPath, ivyOutput, @namespace, settings.SkipIfNotChanged, order);
            await GenerateLlmMarkdownAsync(absoluteInputPath, llmMarkdownOutput, settings.SkipIfNotChanged);
        });

        await Task.WhenAll(tasks);

        return 0;
    }

    private string GetRootNamespace(string projectFile)
    {
        var doc = XDocument.Load(projectFile);
        var rootNamespace = doc.Descendants("RootNamespace").FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(rootNamespace))
            throw new Exception("No <RootNamespace> element found in the project file.");
        return rootNamespace;
    }

    private string GetProjectFile(string startFolder)
    {
        var currentFolder = startFolder;
        while (!string.IsNullOrEmpty(currentFolder))
        {
            var csprojFiles = Directory.GetFiles(currentFolder, "*.csproj");
            if (csprojFiles.Length > 0)
                return csprojFiles[0];
            currentFolder = Directory.GetParent(currentFolder)?.FullName;
        }
        throw new FileNotFoundException("No .csproj file found in the directory hierarchy.");
    }

    private static async Task GenerateLlmMarkdownAsync(string inputFile, string outputFile, bool skipIfNotChanged)
    {
        string markdownContent = await File.ReadAllTextAsync(inputFile);
        string hash = Utils.GetShortHash(markdownContent);

        if (File.Exists(outputFile) && skipIfNotChanged)
        {
            var oldHash = FileHashMetadata.ReadHash(outputFile);
            if (oldHash != null && oldHash == hash)
            {
                return;
            }
        }

        // Console.WriteLine("Generating {0}", outputFile);

        string outputContent = await LlmMarkdownGenerator.GenerateAsync(markdownContent, inputFile);

        await using (var stream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
        await using (var writer = new StreamWriter(stream))
        {
            await writer.WriteAsync(outputContent);
        }

        FileHashMetadata.WriteHash(outputFile, hash);
    }
}

