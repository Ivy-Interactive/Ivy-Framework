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
    public string TendrilData { get; set; } = "";
    public string ReposHome { get; set; } = "";
    public string PlanFolder { get; set; } = "";
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
    private string? _pendingTendrilHome;

    internal ConfigService(TendrilSettings settings)
    {
        _settings = settings;
        _configPath = Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
    }

    public ConfigService()
    {
        // Check for TENDRIL_HOME first - might have config there
        var tendrilHomeEnv = Environment.GetEnvironmentVariable("TENDRIL_HOME");

        // Determine config path: prefer TENDRIL_HOME/config.yaml if it exists
        if (!string.IsNullOrEmpty(tendrilHomeEnv) && Directory.Exists(tendrilHomeEnv))
        {
            var tendrilConfig = Path.Combine(tendrilHomeEnv, "config.yaml");
            if (File.Exists(tendrilConfig))
            {
                _configPath = tendrilConfig;
            }
            else
            {
                _configPath = Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
            }
        }
        else
        {
            _configPath = Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
        }

        var configPath = _configPath;
        var reposHomeEnv = Environment.GetEnvironmentVariable("REPOS_HOME");
        var needsOnboarding = false;

        if (File.Exists(configPath))
        {
            var yaml = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _settings = deserializer.Deserialize<TendrilSettings>(yaml) ?? new TendrilSettings();

            // Override with environment variables if set
            if (!string.IsNullOrEmpty(tendrilHomeEnv))
            {
                _settings.TendrilData = tendrilHomeEnv;
            }
            if (!string.IsNullOrEmpty(reposHomeEnv))
            {
                _settings.ReposHome = reposHomeEnv;
            }

            // Check if we have valid tendril data folder
            if (string.IsNullOrEmpty(_settings.TendrilData) || !Directory.Exists(_settings.TendrilData))
            {
                needsOnboarding = true;
            }
        }
        else
        {
            // No config file exists - need onboarding
            needsOnboarding = true;
            _settings = new TendrilSettings();
            if (!string.IsNullOrEmpty(tendrilHomeEnv))
            {
                _settings.TendrilData = tendrilHomeEnv;
            }
            if (!string.IsNullOrEmpty(reposHomeEnv))
            {
                _settings.ReposHome = reposHomeEnv;
            }
        }

        NeedsOnboarding = needsOnboarding;

        string ExpandTilde(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.StartsWith("~/") || path.StartsWith("~\\"))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Substring(2));
            return path;
        }

        if (_settings != null)
        {
            // First expand tilde
            _settings.TendrilData = ExpandTilde(_settings.TendrilData);
            _settings.ReposHome = ExpandTilde(_settings.ReposHome);
            _settings.PlanFolder = ExpandTilde(_settings.PlanFolder);

            // Initialize user secrets - check TENDRIL_HOME first if it has a .csproj
            var secretsDirectory = Path.GetDirectoryName(_configPath) ?? System.AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(_settings.TendrilData) && Directory.Exists(_settings.TendrilData))
            {
                var tendrilCsproj = Directory.GetFiles(_settings.TendrilData, "*.csproj", SearchOption.TopDirectoryOnly);
                if (tendrilCsproj.Length > 0)
                {
                    secretsDirectory = _settings.TendrilData;
                }
            }
            VariableExpansion.InitializeUserSecrets(secretsDirectory);

            // Then expand variables (now that we know tendrilData and reposHome)
            ExpandSettingsVariables();

            if (_settings.Projects != null)
            {
                foreach (var proj in _settings.Projects)
                {
                    if (proj.Repos != null)
                    {
                        foreach (var repo in proj.Repos)
                        {
                            repo.Path = ExpandTilde(repo.Path);
                            repo.Path = VariableExpansion.ExpandVariables(repo.Path, _settings.TendrilData, _settings.ReposHome);
                        }
                    }
                }
            }
        }

        var tendrilRoot = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

        // Resolve tendrilData path
        if (!string.IsNullOrEmpty(_settings.TendrilData) && !Path.IsPathRooted(_settings.TendrilData))
        {
            _settings.TendrilData = Path.GetFullPath(Path.Combine(tendrilRoot, _settings.TendrilData));
        }

        // Derive planFolder from tendrilData if not explicitly set
        if (string.IsNullOrEmpty(_settings.PlanFolder))
        {
            _settings.PlanFolder = !string.IsNullOrEmpty(_settings.TendrilData)
                ? Path.Combine(_settings.TendrilData, "Plans")
                : Path.Combine(tendrilRoot, ".plans");
        }
        else if (!Path.IsPathRooted(_settings.PlanFolder))
        {
            _settings.PlanFolder = Path.GetFullPath(Path.Combine(tendrilRoot, _settings.PlanFolder));
        }

        // Ensure all required directories exist
        if (!string.IsNullOrEmpty(_settings.TendrilData))
        {
            Directory.CreateDirectory(_settings.TendrilData);
            Directory.CreateDirectory(Path.Combine(_settings.TendrilData, "Inbox"));
            Directory.CreateDirectory(Path.Combine(_settings.TendrilData, "Plans"));
            Directory.CreateDirectory(Path.Combine(_settings.TendrilData, "Trash"));
            Directory.CreateDirectory(Path.Combine(_settings.TendrilData, "Promptwares"));
            Directory.CreateDirectory(Path.Combine(_settings.TendrilData, "Hooks"));
        }
    }

    public TendrilSettings Settings => _settings;
    public string TendrilData => _settings.TendrilData;
    public string ReposHome => _settings.ReposHome;
    public string PlanFolder => _settings.PlanFolder;
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
    private string? _pendingReposHome;

    public void SetPendingTendrilHome(string path)
    {
        _pendingTendrilHome = path;
    }

    public string? GetPendingTendrilHome()
    {
        return _pendingTendrilHome;
    }

    public void SetPendingReposHome(string path)
    {
        _pendingReposHome = path;
    }

    public string? GetPendingReposHome()
    {
        return _pendingReposHome;
    }

    public void CompleteOnboarding(string tendrilHome, string reposHome)
    {
        _settings.TendrilData = tendrilHome;
        _settings.ReposHome = reposHome;
        _settings.PlanFolder = Path.Combine(tendrilHome, "Plans");
        NeedsOnboarding = false;
        SaveSettings();
    }

    /// <summary>
    /// Expand variables in settings after loading config.
    /// </summary>
    private void ExpandSettingsVariables()
    {
        if (_settings == null) return;

        var tendrilHome = _settings.TendrilData;
        var reposHome = _settings.ReposHome;

        // Expand agent command
        _settings.AgentCommand = VariableExpansion.ExpandVariables(_settings.AgentCommand, tendrilHome, reposHome);

        // Expand plan template
        _settings.PlanTemplate = VariableExpansion.ExpandVariables(_settings.PlanTemplate, tendrilHome, reposHome);

        // Expand LLM config
        if (_settings.Llm != null)
        {
            _settings.Llm.Endpoint = VariableExpansion.ExpandVariables(_settings.Llm.Endpoint, tendrilHome, reposHome);
            _settings.Llm.ApiKey = VariableExpansion.ExpandVariables(_settings.Llm.ApiKey, tendrilHome, reposHome);
            _settings.Llm.Model = VariableExpansion.ExpandVariables(_settings.Llm.Model, tendrilHome, reposHome);
        }

        // Expand editor config
        if (_settings.Editor != null)
        {
            _settings.Editor.Command = VariableExpansion.ExpandVariables(_settings.Editor.Command, tendrilHome, reposHome);
            _settings.Editor.Label = VariableExpansion.ExpandVariables(_settings.Editor.Label, tendrilHome, reposHome);
        }

        // Expand promptware configs
        if (_settings.Promptwares != null)
        {
            foreach (var kvp in _settings.Promptwares.ToList())
            {
                var config = kvp.Value;
                config.Model = VariableExpansion.ExpandVariables(config.Model, tendrilHome, reposHome);

                if (config.AllowedTools != null)
                {
                    for (int i = 0; i < config.AllowedTools.Count; i++)
                    {
                        config.AllowedTools[i] = VariableExpansion.ExpandVariables(config.AllowedTools[i], tendrilHome, reposHome);
                    }
                }
            }
        }

        // Expand project configs
        if (_settings.Projects != null)
        {
            foreach (var project in _settings.Projects)
            {
                project.Context = VariableExpansion.ExpandVariables(project.Context, tendrilHome, reposHome);

                // Expand review actions
                if (project.ReviewActions != null)
                {
                    foreach (var action in project.ReviewActions)
                    {
                        action.Condition = VariableExpansion.ExpandVariables(action.Condition, tendrilHome, reposHome);
                        action.Action = VariableExpansion.ExpandVariables(action.Action, tendrilHome, reposHome);
                    }
                }

                // Expand hook actions
                if (project.Hooks != null)
                {
                    foreach (var hook in project.Hooks)
                    {
                        hook.Condition = VariableExpansion.ExpandVariables(hook.Condition, tendrilHome, reposHome);
                        hook.Action = VariableExpansion.ExpandVariables(hook.Action, tendrilHome, reposHome);
                    }
                }
            }
        }

        // Expand verification prompts
        if (_settings.Verifications != null)
        {
            foreach (var verification in _settings.Verifications)
            {
                verification.Prompt = VariableExpansion.ExpandVariables(verification.Prompt, tendrilHome, reposHome);
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
