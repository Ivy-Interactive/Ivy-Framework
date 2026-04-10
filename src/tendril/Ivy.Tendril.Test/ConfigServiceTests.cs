using Ivy.Tendril.Services;

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

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings);
            Assert.Equal(2, service.Settings.Projects.Count);

            var project1 = service.Settings.Projects[0];
            Assert.Equal("TestProject", project1.Name);
            Assert.Single(project1.Repos);
            Assert.Equal(@"D:\Repos\Test", project1.Repos[0].Path);
            Assert.Contains("Test context for the project", project1.Context);

            var project2 = service.Settings.Projects[1];
            Assert.Equal("AnotherProject", project2.Name);
            Assert.Equal(2, project2.Repos.Count);
            Assert.Contains("Another test context", project2.Context);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Return_Empty_Projects_When_No_Section()
    {
        var yaml = @"
codingAgent: claude
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings);
            Assert.NotNull(service.Settings.Projects);
            Assert.Empty(service.Settings.Projects);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Find_Project_By_Name()
    {
        var yaml = @"
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

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            // Test exact match
            var project = service.Settings.Projects.FirstOrDefault(p =>
                p.Name.Equals("IvyFramework", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(project);
            Assert.Equal("IvyFramework", project.Name);
            Assert.Contains("Framework context", project.Context);

            // Test case-insensitive match
            var project2 =
                service.Settings.Projects.FirstOrDefault(p =>
                    p.Name.Equals("ivyagent", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(project2);
            Assert.Equal("IvyAgent", project2.Name);

            // Test non-existent project
            var project3 = service.Settings.Projects.FirstOrDefault(p =>
                p.Name.Equals("NonExistent", StringComparison.OrdinalIgnoreCase));
            Assert.Null(project3);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Deserialize_ReviewActions()
    {
        var yaml = @"
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

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings);
            var project = service.Settings.Projects[0];
            Assert.Equal(2, project.ReviewActions.Count);

            Assert.Equal("Sample", project.ReviewActions[0].Name);
            Assert.Contains("Test-Path", project.ReviewActions[0].Condition);
            Assert.Equal("dotnet run --browse", project.ReviewActions[0].Action);

            Assert.Equal("Open Docs", project.ReviewActions[1].Name);
            Assert.Empty(project.ReviewActions[1].Condition);
            Assert.Contains("start docs", project.ReviewActions[1].Action);
            Assert.Contains("index.html", project.ReviewActions[1].Action);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Default_ReviewActions_To_Empty_List()
    {
        var yaml = @"
projects:
  - name: TestProject
    repos:
      - path: D:\Repos\Test
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings);
            var project = service.Settings.Projects[0];
            Assert.NotNull(project.ReviewActions);
            Assert.Empty(project.ReviewActions);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_IgnoresUnknownYamlKeys()
    {
        var tempDir = CreateTempConfigFile(@"
codingAgent: claude
unknownKey: someValue
projects:
  - name: Test
    color: Blue
    repos: []
    verifications: []
");

        var previousHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        Environment.SetEnvironmentVariable("TENDRIL_HOME", tempDir);

        try
        {
            var service = new ConfigService();

            Assert.NotNull(service.Settings);
            Assert.Equal("claude", service.Settings.CodingAgent);
            Assert.False(service.NeedsOnboarding);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TENDRIL_HOME", previousHome);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SetTendrilHome_IgnoresUnknownYamlKeys()
    {
        var tempDir = CreateTempConfigFile(@"
codingAgent: test-agent
anotherUnknownKey: anotherValue
projects:
  - name: TestProject
    color: Red
    repos: []
    verifications: []
");

        var service = new ConfigService(new TendrilSettings { CodingAgent = "initial" });

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings);
            Assert.Equal("test-agent", service.Settings.CodingAgent);
            Assert.Equal(tempDir, service.TendrilHome);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_And_SetTendrilHome_BehaviorIsConsistent()
    {
        var tempDir = CreateTempConfigFile(@"
codingAgent: consistent-agent
unknownField: ignored
maxConcurrentJobs: 10
projects: []
verifications: []
");

        var previousHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        Environment.SetEnvironmentVariable("TENDRIL_HOME", tempDir);

        try
        {
            // Load via constructor
            var serviceViaConstructor = new ConfigService();

            // Load via SetTendrilHome
            var serviceViaSetHome = new ConfigService(new TendrilSettings());
            serviceViaSetHome.SetTendrilHome(tempDir);

            // Both should succeed and load the same settings
            Assert.Equal("consistent-agent", serviceViaConstructor.Settings.CodingAgent);
            Assert.Equal("consistent-agent", serviceViaSetHome.Settings.CodingAgent);

            Assert.Equal(10, serviceViaConstructor.Settings.MaxConcurrentJobs);
            Assert.Equal(10, serviceViaSetHome.Settings.MaxConcurrentJobs);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TENDRIL_HOME", previousHome);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void NeedsOnboarding_IsTrueWhenNoTendrilHomeSet()
    {
        // When TENDRIL_HOME is not set, ConfigService should indicate onboarding is needed.
        // TendrilServer uses this flag to defer database and watcher service initialization
        // until onboarding completes and a valid TendrilHome is established.
        var previousHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        Environment.SetEnvironmentVariable("TENDRIL_HOME", null);

        try
        {
            var service = new ConfigService();
            Assert.True(service.NeedsOnboarding);
            Assert.Equal("", service.TendrilHome);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TENDRIL_HOME", previousHome);
        }
    }

    [Theory]
    [InlineData("#9a3c3c", "Slate")]
    [InlineData("#2563eb", "Slate")]
    [InlineData("invalid", "Slate")]
    [InlineData("Blue", "Blue")]
    [InlineData("Emerald", "Emerald")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void MigrateProjectColor_HandlesVariousInputs(string? input, string? expected)
    {
        var result = ConfigService.MigrateProjectColor(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void LoadSettings_MigratesHexColorsToSlate()
    {
        var tempDir = CreateTempConfigFile(@"
projects:
  - name: TestProject
    color: '#9a3c3c'
    repos: []
    verifications: []
");

        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.Equal("Slate", service.Settings.Projects[0].Color);

            var savedYaml = File.ReadAllText(Path.Combine(tempDir, "config.yaml"));
            Assert.Contains("Slate", savedYaml);
            Assert.DoesNotContain("#9a3c3c", savedYaml);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadSettings_PreservesValidEnumColors()
    {
        var tempDir = CreateTempConfigFile(@"
projects:
  - name: TestProject
    color: Blue
    repos: []
    verifications: []
");

        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.Equal("Blue", service.Settings.Projects[0].Color);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetRepoRef_ReturnsMatchingRepo()
    {
        var project = new ProjectConfig
        {
            Name = "Test",
            Repos =
            [
                new RepoRef { Path = @"D:\Repos\Foo", PrRule = "yolo" },
                new RepoRef { Path = @"D:\Repos\Bar", PrRule = "default" }
            ]
        };

        var result = project.GetRepoRef(@"D:\Repos\Foo");
        Assert.NotNull(result);
        Assert.Equal("yolo", result.PrRule);
    }

    [Fact]
    public void GetRepoRef_ReturnsNullWhenNotFound()
    {
        var project = new ProjectConfig
        {
            Name = "Test",
            Repos = [new RepoRef { Path = @"D:\Repos\Foo" }]
        };

        Assert.Null(project.GetRepoRef(@"D:\Repos\NonExistent"));
    }

    [Fact]
    public void GetRepoRef_IsCaseInsensitive()
    {
        var project = new ProjectConfig
        {
            Name = "Test",
            Repos = [new RepoRef { Path = @"D:\Repos\Foo", PrRule = "yolo" }]
        };

        var result = project.GetRepoRef(@"d:\repos\foo");
        Assert.NotNull(result);
        Assert.Equal("yolo", result.PrRule);
    }

    [Fact]
    public void Should_Deserialize_PromptwareConfig_Effort()
    {
        var yaml = @"
promptwares:
  MakePlan:
    model: sonnet
    effort: high
    allowedTools:
      - Read
      - Write
  ExecutePlan:
    model: opus
    effort: max
    allowedTools:
      - Read
      - Bash
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings.Promptwares);
            Assert.Equal(2, service.Settings.Promptwares.Count);

            var makePlan = service.Settings.Promptwares["MakePlan"];
            Assert.Equal("sonnet", makePlan.Model);
            Assert.Equal("high", makePlan.Effort);
            Assert.Equal(new List<string> { "Read", "Write" }, makePlan.AllowedTools);

            var executePlan = service.Settings.Promptwares["ExecutePlan"];
            Assert.Equal("opus", executePlan.Model);
            Assert.Equal("max", executePlan.Effort);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Deserialize_DefaultEffort()
    {
        var yaml = @"
defaultEffort: medium
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.Equal("medium", service.Settings.DefaultEffort);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DefaultEffort_DefaultsToHigh()
    {
        var settings = new TendrilSettings();
        Assert.Equal("high", settings.DefaultEffort);
    }

    [Fact]
    public void PromptwareConfig_Effort_DefaultsToEmpty()
    {
        var config = new PromptwareConfig();
        Assert.Equal("", config.Effort);
    }

    [Fact]
    public void Should_Expand_Effort_Variables()
    {
        var yaml = @"
defaultEffort: high
promptwares:
  TestPw:
    model: sonnet
    effort: high
    allowedTools:
      - Read
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            // Verify effort values survive expansion (non-variable values pass through unchanged)
            Assert.Equal("high", service.Settings.DefaultEffort);
            Assert.Equal("high", service.Settings.Promptwares["TestPw"].Effort);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Parse_Default_Key_In_Promptwares()
    {
        var yaml = @"
promptwares:
  _default:
    model: sonnet
    effort: high
    allowedTools:
      - Read
      - Glob
  ExecutePlan:
    model: opus
    effort: max
    allowedTools:
      - Read
      - Write
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings.Promptwares);
            Assert.True(service.Settings.Promptwares.ContainsKey("_default"));
            Assert.True(service.Settings.Promptwares.ContainsKey("ExecutePlan"));

            var defaultConfig = service.Settings.Promptwares["_default"];
            Assert.Equal("sonnet", defaultConfig.Model);
            Assert.Equal("high", defaultConfig.Effort);
            Assert.Equal(2, defaultConfig.AllowedTools.Count);
            Assert.Contains("Read", defaultConfig.AllowedTools);
            Assert.Contains("Glob", defaultConfig.AllowedTools);

            var execConfig = service.Settings.Promptwares["ExecutePlan"];
            Assert.Equal("opus", execConfig.Model);
            Assert.Equal("max", execConfig.Effort);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Parse_Partial_Promptware_Entry_Alongside_Default()
    {
        var yaml = @"
promptwares:
  _default:
    model: sonnet
    effort: high
    allowedTools:
      - Read
      - Glob
  ExecutePlan:
    model: opus
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            var defaultConfig = service.Settings.Promptwares["_default"];
            Assert.Equal("sonnet", defaultConfig.Model);
            Assert.Equal("high", defaultConfig.Effort);
            Assert.Equal(2, defaultConfig.AllowedTools.Count);

            var execConfig = service.Settings.Promptwares["ExecutePlan"];
            Assert.Equal("opus", execConfig.Model);
            Assert.Equal("", execConfig.Effort); // Not specified — PowerShell merge will inherit from _default
            Assert.Empty(execConfig.AllowedTools); // Not specified — PowerShell merge will inherit from _default
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void EditorConfig_IsAvailable_WhenCommandExists()
    {
        // "dotnet" should be available on any machine running these tests
        var result = ConfigService.IsCommandAvailable("dotnet");
        Assert.True(result);
    }

    [Fact]
    public void EditorConfig_IsAvailable_WhenCommandMissing()
    {
        var result = ConfigService.IsCommandAvailable("nonexistent-command-xyz-12345");
        Assert.False(result);
    }

    [Fact]
    public void PlatformHelper_OpenInEditor_ReturnsFalse_WhenCommandInvalid()
    {
        var result = PlatformHelper.OpenInEditor("nonexistent-editor-xyz-12345", "somefile.txt");
        Assert.False(result);
    }
}