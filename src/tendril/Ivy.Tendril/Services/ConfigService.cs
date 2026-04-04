using Ivy;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Services;

public record RepoConfig
{
    public string Owner { get; set; } = "";
    public string Name { get; set; } = "";
    public string FullName => $"{Owner}/{Name}";
    public string DisplayName => Name;
}

public record RepoRef
{
    public string Path { get; set; } = "";
    public string PrRule { get; set; } = "default";
}

public record ProjectConfig
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public Dictionary<string, object> Meta { get; set; } = new();
    public List<RepoRef> Repos { get; set; } = new();
    public List<ProjectVerificationRef> Verifications { get; set; } = new();
    public string Context { get; set; } = "";
    public List<ReviewActionConfig> ReviewActions { get; set; } = new();
    public List<PromptwareHookConfig> Hooks { get; set; } = new();
    public List<string> RepoPaths => Repos.Select(r => r.Path).ToList();
    public string? GetMeta(string key) => Meta.TryGetValue(key, out var v) ? v?.ToString() : null;
}

public record LevelConfig
{
    public string Name { get; set; } = "";
    public string Badge { get; set; } = "Outline";
}

public record VerificationConfig
{
    public string Name { get; set; } = "";
    public string Prompt { get; set; } = "";
}

public record ProjectVerificationRef
{
    public string Name { get; set; } = "";
    public bool Required { get; set; }
}

public record ReviewActionConfig
{
    public string Name { get; set; } = "";
    public string Condition { get; set; } = "";
    public string Action { get; set; } = "";
}

public record PromptwareHookConfig
{
    public string Name { get; set; } = "";
    public string When { get; set; } = "before";
    public List<string> Promptwares { get; set; } = new();
    public string Condition { get; set; } = "";
    public string Action { get; set; } = "";
}

public record EditorConfig
{
    public string Command { get; set; } = "code";
    public string Label { get; set; } = "VS Code";
}

public record PromptwareConfig
{
    public string Model { get; set; } = "";
    public List<string> AllowedTools { get; set; } = new();
}

public record LlmConfig
{
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gpt-4o-mini";
}

public class TendrilSettings
{
    public string AgentCommand { get; set; } = "claude";
    public int JobTimeout { get; set; } = 30;
    public int StaleOutputTimeout { get; set; } = 10;
    public List<ProjectConfig> Projects { get; set; } = new();
    public List<VerificationConfig> Verifications { get; set; } = new();
    public string PlanTemplate { get; set; } = "";
    public EditorConfig Editor { get; set; } = new();
    public LlmConfig? Llm { get; set; }
    public Dictionary<string, PromptwareConfig> Promptwares { get; set; } = new();
    public bool Telemetry { get; set; } = true;
    public List<LevelConfig> Levels { get; set; } = new()
    {
        new() { Name = "Critical", Badge = "Warning" },
        new() { Name = "Bug", Badge = "Destructive" },
        new() { Name = "NiceToHave", Badge = "Outline" },
        new() { Name = "Epic", Badge = "Info" }
    };
}

public class ConfigService
{
    private readonly TendrilSettings _settings;
    private readonly string _configPath;
    private readonly string _tendrilHome;
    private string? _pendingTendrilHome;

    internal ConfigService(TendrilSettings settings, string tendrilHome = "")
    {
        _settings = settings;
        _tendrilHome = !string.IsNullOrEmpty(tendrilHome)
            ? tendrilHome
            : Environment.GetEnvironmentVariable("TENDRIL_HOME") ?? "";
        _configPath = !string.IsNullOrEmpty(_tendrilHome)
            ? Path.Combine(_tendrilHome, "config.yaml")
            : Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
    }

    public ConfigService()
    {
        // TENDRIL_HOME is required
        var tendrilHomeEnv = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        // If TENDRIL_HOME is not set, trigger onboarding
        if (string.IsNullOrEmpty(tendrilHomeEnv))
        {
            NeedsOnboarding = true;
            _settings = new TendrilSettings();
            _configPath = Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
            _tendrilHome = "";
            return;
        }

        _tendrilHome = tendrilHomeEnv;

        // Determine config path: TENDRIL_HOME/config.yaml
        _configPath = Path.Combine(_tendrilHome, "config.yaml");

        // Load config if it exists
        if (File.Exists(_configPath))
        {
            var yaml = File.ReadAllText(_configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _settings = deserializer.Deserialize<TendrilSettings>(yaml) ?? new TendrilSettings();
        }
        else
        {
            // No config file exists - need onboarding
            NeedsOnboarding = true;
            _settings = new TendrilSettings();
            return;
        }

        NeedsOnboarding = false;

        if (_settings != null && !NeedsOnboarding)
        {
            // Initialize user secrets - check TENDRIL_HOME if it has a .csproj
            var secretsDirectory = Path.GetDirectoryName(_configPath) ?? System.AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(_tendrilHome) && Directory.Exists(_tendrilHome))
            {
                var tendrilCsproj = Directory.GetFiles(_tendrilHome, "*.csproj", SearchOption.TopDirectoryOnly);
                if (tendrilCsproj.Length > 0)
                {
                    secretsDirectory = _tendrilHome;
                }
            }
            VariableExpansion.InitializeUserSecrets(secretsDirectory);

            // Expand variables in settings
            ExpandSettingsVariables();

            // Expand repo paths
            if (_settings.Projects != null)
            {
                foreach (var proj in _settings.Projects)
                {
                    if (proj.Repos != null)
                    {
                        foreach (var repo in proj.Repos)
                        {
                            repo.Path = VariableExpansion.ExpandVariables(repo.Path, _tendrilHome);
                        }
                    }
                }
            }

            // Ensure all required directories exist
            Directory.CreateDirectory(_tendrilHome);
            Directory.CreateDirectory(Path.Combine(_tendrilHome, "Inbox"));
            Directory.CreateDirectory(Path.Combine(_tendrilHome, "Plans"));
            Directory.CreateDirectory(Path.Combine(_tendrilHome, "Trash"));
            Directory.CreateDirectory(Path.Combine(_tendrilHome, "Promptwares"));
            Directory.CreateDirectory(Path.Combine(_tendrilHome, "Hooks"));
        }
    }

    public TendrilSettings Settings => _settings;
    public string TendrilHome => _tendrilHome;
    public string PlanFolder => string.IsNullOrEmpty(_tendrilHome) ? "" : Path.Combine(_tendrilHome, "Plans");
    public List<ProjectConfig> Projects => _settings.Projects;
    public List<LevelConfig> Levels => _settings.Levels;
    public string[] LevelNames => _settings.Levels.Select(l => l.Name).ToArray();
    public EditorConfig Editor => _settings.Editor;
    public ProjectConfig? GetProject(string name) => _settings.Projects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public BadgeVariant GetBadgeVariant(string level) =>
        Enum.TryParse<BadgeVariant>(_settings.Levels.FirstOrDefault(l => l.Name == level)?.Badge ?? "Outline", out var v) ? v : BadgeVariant.Outline;

    public Colors? GetProjectColor(string projectName)
    {
        var colorStr = GetProject(projectName)?.Color;
        return !string.IsNullOrEmpty(colorStr) && Enum.TryParse<Colors>(colorStr, out var c) ? c : null;
    }

    public void SaveSettings()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .Build();
        var yaml = serializer.Serialize(_settings);
        File.WriteAllText(_configPath, yaml);
    }

    // Onboarding support
    public bool NeedsOnboarding { get; private set; }
    public void SetPendingTendrilHome(string path)
    {
        _pendingTendrilHome = path;
    }

    public string? GetPendingTendrilHome()
    {
        return _pendingTendrilHome;
    }

    public void CompleteOnboarding(string tendrilHome)
    {
        // Create tendril home directory structure
        Directory.CreateDirectory(tendrilHome);
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Inbox"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Plans"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Trash"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Promptwares"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Hooks"));

        // Save config to tendrilHome
        var newConfigPath = Path.Combine(tendrilHome, "config.yaml");
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .Build();
        var yaml = serializer.Serialize(_settings);
        File.WriteAllText(newConfigPath, yaml);

        NeedsOnboarding = false;
    }

    /// <summary>
    /// Expand variables in settings after loading config.
    /// </summary>
    private void ExpandSettingsVariables()
    {
        if (_settings == null) return;

        // Expand agent command
        _settings.AgentCommand = VariableExpansion.ExpandVariables(_settings.AgentCommand, _tendrilHome);

        // Expand plan template
        _settings.PlanTemplate = VariableExpansion.ExpandVariables(_settings.PlanTemplate, _tendrilHome);

        // Expand LLM config
        if (_settings.Llm != null)
        {
            _settings.Llm.Endpoint = VariableExpansion.ExpandVariables(_settings.Llm.Endpoint, _tendrilHome);
            _settings.Llm.ApiKey = VariableExpansion.ExpandVariables(_settings.Llm.ApiKey, _tendrilHome);
            _settings.Llm.Model = VariableExpansion.ExpandVariables(_settings.Llm.Model, _tendrilHome);
        }

        // Expand editor config
        if (_settings.Editor != null)
        {
            _settings.Editor.Command = VariableExpansion.ExpandVariables(_settings.Editor.Command, _tendrilHome);
            _settings.Editor.Label = VariableExpansion.ExpandVariables(_settings.Editor.Label, _tendrilHome);
        }

        // Expand promptware configs
        if (_settings.Promptwares != null)
        {
            foreach (var kvp in _settings.Promptwares.ToList())
            {
                var config = kvp.Value;
                config.Model = VariableExpansion.ExpandVariables(config.Model, _tendrilHome);

                if (config.AllowedTools != null)
                {
                    for (int i = 0; i < config.AllowedTools.Count; i++)
                    {
                        config.AllowedTools[i] = VariableExpansion.ExpandVariables(config.AllowedTools[i], _tendrilHome);
                    }
                }
            }
        }

        // Expand project configs
        if (_settings.Projects != null)
        {
            foreach (var project in _settings.Projects)
            {
                project.Context = VariableExpansion.ExpandVariables(project.Context, _tendrilHome);

                // Expand review actions
                if (project.ReviewActions != null)
                {
                    foreach (var action in project.ReviewActions)
                    {
                        action.Condition = VariableExpansion.ExpandVariables(action.Condition, _tendrilHome);
                        action.Action = VariableExpansion.ExpandVariables(action.Action, _tendrilHome);
                    }
                }

                // Expand hook actions
                if (project.Hooks != null)
                {
                    foreach (var hook in project.Hooks)
                    {
                        hook.Condition = VariableExpansion.ExpandVariables(hook.Condition, _tendrilHome);
                        hook.Action = VariableExpansion.ExpandVariables(hook.Action, _tendrilHome);
                    }
                }
            }
        }

        // Expand verification prompts
        if (_settings.Verifications != null)
        {
            foreach (var verification in _settings.Verifications)
            {
                verification.Prompt = VariableExpansion.ExpandVariables(verification.Prompt, _tendrilHome);
            }
        }
    }
}


public static class ProjectBadgeExtensions
{
    public static Badge WithProjectColor(this Badge badge, ConfigService config, string projectName)
    {
        var color = config.GetProjectColor(projectName);
        return color.HasValue ? badge.Color(color.Value) : badge;
    }
}
