using Ivy.Tendril.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Test;

public class ConfigServiceTests
{
    private static string CreateTempConfigFile(string yamlContent)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-config-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "config.yaml");
        File.WriteAllText(configPath, yamlContent);
        return tempDir;
    }

    [Fact]
    public void Should_Parse_Projects_From_Config()
    {
        var yaml = @"
agentCommand: claude

projects:
  - name: TestProject
    repos:
      - path: D:\Repos\Test
    context: |
      Test context for the project.
      Multiple lines supported.
  - name: AnotherProject
    repos:
      - path: D:\Repos\Another
      - path: D:\Repos\Another2
    context: |
      Another test context.
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.NotNull(settings);
        Assert.Equal(2, settings.Projects.Count);

        var project1 = settings.Projects[0];
        Assert.Equal("TestProject", project1.Name);
        Assert.Single(project1.Repos);
        Assert.Equal(@"D:\Repos\Test", project1.Repos[0].Path);
        Assert.Contains("Test context for the project", project1.Context);

        var project2 = settings.Projects[1];
        Assert.Equal("AnotherProject", project2.Name);
        Assert.Equal(2, project2.Repos.Count);
        Assert.Contains("Another test context", project2.Context);
    }

    [Fact]
    public void Should_Return_Empty_Projects_When_No_Section()
    {
        var yaml = @"
agentCommand: claude
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.NotNull(settings);
        Assert.NotNull(settings.Projects);
        Assert.Empty(settings.Projects);
    }

    [Fact]
    public void Should_Find_Project_By_Name()
    {
        var yaml = @"
agentCommand: claude

projects:
  - name: IvyFramework
    repos:
      - path: D:\Repos\Ivy-Framework
    context: Framework context
  - name: IvyAgent
    repos:
      - path: D:\Repos\Ivy-Agent
    context: Agent context
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        // Test exact match
        var project = settings.Projects.FirstOrDefault(p => p.Name.Equals("IvyFramework", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(project);
        Assert.Equal("IvyFramework", project.Name);
        Assert.Contains("Framework context", project.Context);

        // Test case-insensitive match
        var project2 = settings.Projects.FirstOrDefault(p => p.Name.Equals("ivyagent", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(project2);
        Assert.Equal("IvyAgent", project2.Name);

        // Test non-existent project
        var project3 = settings.Projects.FirstOrDefault(p => p.Name.Equals("NonExistent", StringComparison.OrdinalIgnoreCase));
        Assert.Null(project3);
    }

    [Fact]
    public void Should_Deserialize_ReviewActions()
    {
        var yaml = @"
agentCommand: claude

projects:
  - name: TestProject
    repos:
      - path: D:\Repos\Test
    reviewActions:
      - name: Sample
        condition: 'Test-Path ""artifacts\sample\*.csproj""'
        action: 'dotnet run --browse'
      - name: Open Docs
        condition: ''
        action: 'start docs/index.html'
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.NotNull(settings);
        var project = settings.Projects[0];
        Assert.Equal(2, project.ReviewActions.Count);

        Assert.Equal("Sample", project.ReviewActions[0].Name);
        Assert.Contains("Test-Path", project.ReviewActions[0].Condition);
        Assert.Equal("dotnet run --browse", project.ReviewActions[0].Action);

        Assert.Equal("Open Docs", project.ReviewActions[1].Name);
        Assert.Empty(project.ReviewActions[1].Condition);
        Assert.Equal("start docs/index.html", project.ReviewActions[1].Action);
    }

    [Fact]
    public void Should_Default_ReviewActions_To_Empty_List()
    {
        var yaml = @"
agentCommand: claude

projects:
  - name: TestProject
    repos:
      - path: D:\Repos\Test
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.NotNull(settings);
        var project = settings.Projects[0];
        Assert.NotNull(project.ReviewActions);
        Assert.Empty(project.ReviewActions);
    }
}
