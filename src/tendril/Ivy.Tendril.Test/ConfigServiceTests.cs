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

        try
        {
            var service = new ConfigService(new TendrilSettings());
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings);
            Assert.Equal("claude", service.Settings.CodingAgent);
        }
        finally
        {
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
    public void SetTendrilHome_LoadsSettingsConsistently()
    {
        var tempDir = CreateTempConfigFile(@"
codingAgent: consistent-agent
unknownField: ignored
maxConcurrentJobs: 10
projects: []
verifications: []
");

        try
        {
            var service = new ConfigService(new TendrilSettings());
            service.SetTendrilHome(tempDir);

            Assert.Equal("consistent-agent", service.Settings.CodingAgent);
            Assert.Equal(10, service.Settings.MaxConcurrentJobs);
        }
        finally
        {
            Directory.Delete(tempDir, true);
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
    public void Should_Parse_Default_Key_In_Promptwares()
    {
        var yaml = @"
promptwares:
  _default:
    profile: balanced
    allowedTools:
      - Read
      - Glob
  ExecutePlan:
    profile: deep
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
            Assert.Equal("balanced", defaultConfig.Profile);
            Assert.Equal(2, defaultConfig.AllowedTools.Count);
            Assert.Contains("Read", defaultConfig.AllowedTools);
            Assert.Contains("Glob", defaultConfig.AllowedTools);

            var execConfig = service.Settings.Promptwares["ExecutePlan"];
            Assert.Equal("deep", execConfig.Profile);
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
    profile: balanced
    allowedTools:
      - Read
      - Glob
  ExecutePlan:
    profile: deep
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            var defaultConfig = service.Settings.Promptwares["_default"];
            Assert.Equal("balanced", defaultConfig.Profile);
            Assert.Equal(2, defaultConfig.AllowedTools.Count);

            var execConfig = service.Settings.Promptwares["ExecutePlan"];
            Assert.Equal("deep", execConfig.Profile);
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

    [Fact]
    public void Should_Deserialize_Agents_Section_With_Profiles()
    {
        var yaml = @"
codingAgents:
  - name: ClaudeCode
    profiles:
      - name: deep
        model: claude-opus-4-6
        effort: max
      - name: balanced
        model: claude-sonnet-4-6
        effort: high
      - name: quick
        model: claude-haiku-4-5
        effort: low
  - name: Codex
    profiles:
      - name: deep
        model: gpt-5.4
        effort: high
      - name: balanced
        model: gpt-5.4-mini
        effort: medium
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings.CodingAgents);
            Assert.Equal(2, service.Settings.CodingAgents.Count);

            var claude = service.Settings.CodingAgents[0];
            Assert.Equal("ClaudeCode", claude.Name);
            Assert.Equal(3, claude.Profiles.Count);

            var claudeDeep = claude.Profiles[0];
            Assert.Equal("deep", claudeDeep.Name);
            Assert.Equal("claude-opus-4-6", claudeDeep.Model);
            Assert.Equal("max", claudeDeep.Effort);

            var claudeBalanced = claude.Profiles[1];
            Assert.Equal("balanced", claudeBalanced.Name);
            Assert.Equal("claude-sonnet-4-6", claudeBalanced.Model);
            Assert.Equal("high", claudeBalanced.Effort);

            var codex = service.Settings.CodingAgents[1];
            Assert.Equal("Codex", codex.Name);
            Assert.Equal(2, codex.Profiles.Count);
            Assert.Equal("gpt-5.4", codex.Profiles[0].Model);
            Assert.Equal("high", codex.Profiles[0].Effort);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Deserialize_AgentConfig_Arguments()
    {
        var yaml = @"
codingAgent: claude
codingAgents:
  - name: ClaudeCode
    arguments: --global-flag
    profiles:
      - name: deep
        model: claude-opus-4-6
        effort: max
        arguments: --temperature 0.2
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            Assert.NotNull(service.Settings.CodingAgents);
            Assert.Single(service.Settings.CodingAgents);

            var agent = service.Settings.CodingAgents[0];
            Assert.Equal("ClaudeCode", agent.Name);
            Assert.Equal("--global-flag", agent.Arguments);

            Assert.Single(agent.Profiles);
            var deep = agent.Profiles[0];
            Assert.Equal("deep", deep.Name);
            Assert.Equal("claude-opus-4-6", deep.Model);
            Assert.Equal("max", deep.Effort);
            Assert.Equal("--temperature 0.2", deep.Arguments);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Deserialize_PromptwareConfig_Profile_Field()
    {
        var yaml = @"
promptwares:
  ExecutePlan:
    profile: deep
    allowedTools:
      - Read
      - Bash
  MakePlan:
    profile: balanced
    allowedTools:
      - Read
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            var execute = service.Settings.Promptwares["ExecutePlan"];
            Assert.Equal("deep", execute.Profile);

            var make = service.Settings.Promptwares["MakePlan"];
            Assert.Equal("balanced", make.Profile);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Agents_DefaultsToEmptyList()
    {
        var settings = new TendrilSettings();
        Assert.NotNull(settings.CodingAgents);
        Assert.Empty(settings.CodingAgents);
    }

    [Fact]
    public void PromptwareConfig_Profile_DefaultsToEmpty()
    {
        var config = new PromptwareConfig();
        Assert.Equal("", config.Profile);
    }

    [Fact]
    public void AgentProfileConfig_Fields_DefaultsToEmpty()
    {
        var profile = new AgentProfileConfig();
        Assert.Equal("", profile.Name);
        Assert.Equal("", profile.Model);
        Assert.Equal("", profile.Effort);
        Assert.Equal("", profile.Arguments);
    }

    [Fact]
    public void SaveSettings_PersistsAdvancedSettings()
    {
        var yaml = @"
jobTimeout: 30
staleOutputTimeout: 10
maxConcurrentJobs: 5
editor:
  command: code
  label: VS Code
";

        var tempDir = CreateTempConfigFile(yaml);
        var service = new ConfigService(new TendrilSettings());

        try
        {
            service.SetTendrilHome(tempDir);

            service.Settings.JobTimeout = 45;
            service.Settings.StaleOutputTimeout = 15;
            service.Settings.MaxConcurrentJobs = 8;
            service.Settings.Editor.Command = "vim";
            service.Settings.Editor.Label = "Vim";
            service.SaveSettings();

            var reloaded = new ConfigService(new TendrilSettings());
            reloaded.SetTendrilHome(tempDir);

            Assert.Equal(45, reloaded.Settings.JobTimeout);
            Assert.Equal(15, reloaded.Settings.StaleOutputTimeout);
            Assert.Equal(8, reloaded.Settings.MaxConcurrentJobs);
            Assert.Equal("vim", reloaded.Settings.Editor.Command);
            Assert.Equal("Vim", reloaded.Settings.Editor.Label);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}