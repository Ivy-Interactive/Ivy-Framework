using System.Text.Json;
using Ivy.Docs.Tools;

namespace Ivy.Docs.Test;

public class GenerateApiDocsCommandTests
{
    [Fact]
    public void BuildKey_FormatsCorrectly()
    {
        var key = GenerateApiDocsCommand.BuildKey("Ivy.Button", "Ivy.ButtonExtensions", "https://example.com/Button.cs");
        Assert.Equal("Ivy.Button|Ivy.ButtonExtensions|https://example.com/Button.cs", key);
    }

    [Fact]
    public void BuildKey_HandlesNullExtensionTypesAndSourceUrl()
    {
        var key = GenerateApiDocsCommand.BuildKey("Ivy.Field", null, null);
        Assert.Equal("Ivy.Field||", key);
    }

    [Fact]
    public void GenerateApiDocsCommand_ProducesValidManifest()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"api_docs_test_{Guid.NewGuid():N}");
        var docsDir = Path.Combine(tempDir, "docs");
        var outputFile = Path.Combine(tempDir, "api-docs.json");

        try
        {
            Directory.CreateDirectory(docsDir);

            // Create a test markdown file with a WidgetDocs tag
            var testMd = Path.Combine(docsDir, "TestWidget.md");
            File.WriteAllText(testMd, """
                ---
                title: Test
                ---

                # Test Widget

                <WidgetDocs Type="Ivy.TextInput" ExtensionTypes="Ivy.TextInputExtensions" SourceUrl="https://example.com/TextInput.cs"/>
                """);

            var app = new Spectre.Console.Cli.CommandApp();
            app.Configure(config =>
            {
                config.AddCommand<GenerateApiDocsCommand>("generate-api-docs");
            });

            var exitCode = app.Run(["generate-api-docs", Path.Combine(docsDir, "*.md"), outputFile]);
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputFile));

            var json = File.ReadAllText(outputFile);
            var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.NotNull(manifest);

            var expectedKey = "Ivy.TextInput|Ivy.TextInputExtensions|https://example.com/TextInput.cs";
            Assert.True(manifest.ContainsKey(expectedKey), $"Manifest should contain key: {expectedKey}");

            var apiDoc = manifest[expectedKey];
            Assert.Contains("## API", apiDoc);
            Assert.Contains("### Constructors", apiDoc);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
