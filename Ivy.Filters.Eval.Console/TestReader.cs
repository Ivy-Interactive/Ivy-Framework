using Ivy.Filters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Filters.Eval.Console;

public class TestReader
{
    public static TestDocument Read(string filePath)
    {
        var yaml = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yamlDoc = deserializer.Deserialize<YamlTestDocument>(yaml);

        return new TestDocument(
            yamlDoc.Models ?? [],
            yamlDoc.Suites?.Select(s => new TestSuite(
                s.Name ?? "unnamed",
                s.Fields?.Select(f => new FieldMeta(
                    f.DisplayName ?? f.Id ?? "unknown",
                    f.Id ?? "unknown",
                    ParseFieldType(f.Type)
                )).ToArray() ?? [],
                s.Tests?.Select(t => new TestCase(
                    t.Filter ?? "",
                    t.Expected ?? []
                )).ToArray() ?? []
            )).ToArray() ?? []
        );
    }

    private static FieldType ParseFieldType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "text" => FieldType.Text,
            "number" => FieldType.Number,
            "date" => FieldType.Date,
            "datetime" => FieldType.DateTime,
            "boolean" => FieldType.Boolean,
            _ => FieldType.Text
        };
    }

    // Internal YAML deserialization classes
    private class YamlTestDocument
    {
        public string[]? Models { get; set; }
        public YamlTestSuite[]? Suites { get; set; }
    }

    private class YamlTestSuite
    {
        public string? Name { get; set; }
        public YamlField[]? Fields { get; set; }
        public YamlTestCase[]? Tests { get; set; }
    }

    private class YamlField
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Type { get; set; }
    }

    private class YamlTestCase
    {
        public string? Filter { get; set; }
        public string[]? Expected { get; set; }
    }
}

public record TestDocument(
    string[] Models,
    TestSuite[] Suites
);
