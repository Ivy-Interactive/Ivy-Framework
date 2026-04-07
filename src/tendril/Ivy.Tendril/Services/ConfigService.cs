using System.Text.RegularExpressions;

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

    public string? GetMeta(string key)
    {
        return Meta.TryGetValue(key, out var v) ? v?.ToString() : null;
    }
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
    public string CodingAgent { get; set; } = "claude";
    public int JobTimeout { get; set; } = 30;
    public int StaleOutputTimeout { get; set; } = 10;
    public int MaxConcurrentJobs { get; set; } = 5;
    public List<ProjectConfig> Projects { get; set; } = new();
    public List<VerificationConfig> Verifications { get; set; } = new();
    public string PlanTemplate { get; set; } = "";
    public EditorConfig Editor { get; set; } = new();
    public LlmConfig? Llm { get; set; }
    public Dictionary<string, PromptwareConfig> Promptwares { get; set; } = new();
    public bool Telemetry { get; set; } = true;

    public List<LevelConfig> Levels { get; set; } = new()
    {
        new LevelConfig { Name = "Critical", Badge = "Warning" },
        new LevelConfig { Name = "Bug", Badge = "Destructive" },
        new LevelConfig { Name = "NiceToHave", Badge = "Outline" },
        new LevelConfig { Name = "Epic", Badge = "Info" }
    };
}

public class ConfigService : IConfigService
{
    private string[]? _levelNamesCache;
    private ProjectConfig? _pendingProject;
    private string? _pendingTendrilHome;
    private List<VerificationConfig>? _pendingVerificationDefinitions;

    internal ConfigService(TendrilSettings settings, string tendrilHome = "")
    {
        Settings = settings;
        TendrilHome = !string.IsNullOrEmpty(tendrilHome)
            ? tendrilHome
            : Environment.GetEnvironmentVariable("TENDRIL_HOME") ?? "";
        ConfigPath = !string.IsNullOrEmpty(TendrilHome)
            ? Path.Combine(TendrilHome, "config.yaml")
            : Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
    }

    public ConfigService()
    {
        var tendrilHomeEnv = Environment.GetEnvironmentVariable("TENDRIL_HOME")?.Trim();

        // Remove quotes if present
        if (!string.IsNullOrEmpty(tendrilHomeEnv) && tendrilHomeEnv.StartsWith("\"") && tendrilHomeEnv.EndsWith("\""))
            tendrilHomeEnv = tendrilHomeEnv.Substring(1, tendrilHomeEnv.Length - 2);

        if (string.IsNullOrEmpty(tendrilHomeEnv))
        {
            NeedsOnboarding = true;
            Settings = new TendrilSettings();
            ConfigPath = Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
            TendrilHome = "";
            return;
        }

        TendrilHome = tendrilHomeEnv;
        ConfigPath = Path.Combine(TendrilHome, "config.yaml");

        if (File.Exists(ConfigPath))
        {
            try
            {
                var yaml = FileHelper.ReadAllText(ConfigPath);
                // Quote unquoted %VAR% patterns that YAML rejects (% is a directive indicator)
                yaml = Regex.Replace(yaml, @"(?m)(?<=:\s+)(%\w+%.*)$", "'$1'");
                yaml = Regex.Replace(yaml, @"(?m)^(\s*-\s+)(%\w+%.*)$", "$1'$2'");
                Settings = YamlHelper.Deserializer.Deserialize<TendrilSettings>(yaml) ?? new TendrilSettings();
                MigrateProjectColors();
                NeedsOnboarding = false;
            }
            catch (Exception)
            {
                NeedsOnboarding = true;
                Settings = new TendrilSettings();
            }
        }
        else
        {
            NeedsOnboarding = true;
            Settings = new TendrilSettings();
            return;
        }

        if (Settings != null && !NeedsOnboarding)
        {
            // Initialize basic stuff
            VariableExpansion.InitializeUserSecrets(TendrilHome);
            ExpandSettingsVariables();

            // Expand repo paths
            if (Settings.Projects != null)
                foreach (var proj in Settings.Projects)
                    if (proj.Repos != null)
                        foreach (var repo in proj.Repos)
                            repo.Path = VariableExpansion.ExpandVariables(repo.Path, TendrilHome);

            // Ensure directories exist
            Directory.CreateDirectory(TendrilHome);
            Directory.CreateDirectory(Path.Combine(TendrilHome, "Inbox"));
            Directory.CreateDirectory(Path.Combine(TendrilHome, "Plans"));
            Directory.CreateDirectory(Path.Combine(TendrilHome, "Trash"));
            Directory.CreateDirectory(Path.Combine(TendrilHome, "Promptwares"));
            Directory.CreateDirectory(Path.Combine(TendrilHome, "Hooks"));
        }
    }

    public TendrilSettings Settings { get; private set; }

    public string TendrilHome { get; private set; }

    public string ConfigPath { get; private set; }

    public string PlanFolder => string.IsNullOrEmpty(TendrilHome) ? "" : Path.Combine(TendrilHome, "Plans");

    public List<ProjectConfig> Projects => Settings.Projects;

    // Levels are returned in the order defined in config.yaml (not sorted).
    // Users can reorder levels in the Settings UI, and the order is preserved.
    public List<LevelConfig> Levels => Settings.Levels;

    public string[] LevelNames
    {
        get
        {
            if (_levelNamesCache == null) _levelNamesCache = Settings.Levels.Select(l => l.Name).ToArray();
            return _levelNamesCache;
        }
    }

    public EditorConfig Editor => Settings.Editor;

    public ProjectConfig? GetProject(string name)
    {
        return Settings.Projects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public BadgeVariant GetBadgeVariant(string level)
    {
        return Enum.TryParse<BadgeVariant>(Settings.Levels.FirstOrDefault(l => l.Name == level)?.Badge ?? "Outline",
            out var v)
            ? v
            : BadgeVariant.Outline;
    }

    public Colors? GetProjectColor(string projectName)
    {
        var colorStr = GetProject(projectName)?.Color;
        return !string.IsNullOrEmpty(colorStr) && Enum.TryParse<Colors>(colorStr, out var c) ? c : null;
    }

    public void SaveSettings()
    {
        _levelNamesCache = null;
        var yaml = YamlHelper.SerializerCompact.Serialize(Settings);
        FileHelper.WriteAllText(ConfigPath, yaml);
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

    public void SetPendingProject(ProjectConfig project)
    {
        _pendingProject = project;
    }

    public ProjectConfig? GetPendingProject()
    {
        return _pendingProject;
    }

    public void SetPendingVerificationDefinitions(List<VerificationConfig> definitions)
    {
        _pendingVerificationDefinitions = definitions;
    }

    public List<VerificationConfig>? GetPendingVerificationDefinitions()
    {
        return _pendingVerificationDefinitions;
    }

    public void CompleteOnboarding(string tendrilHome)
    {
        // Update paths
        SetTendrilHome(tendrilHome);

        // Ensure directories exist
        Directory.CreateDirectory(TendrilHome);
        Directory.CreateDirectory(Path.Combine(TendrilHome, "Inbox"));
        Directory.CreateDirectory(Path.Combine(TendrilHome, "Plans"));
        Directory.CreateDirectory(Path.Combine(TendrilHome, "Trash"));
        Directory.CreateDirectory(Path.Combine(TendrilHome, "Promptwares"));
        Directory.CreateDirectory(Path.Combine(TendrilHome, "Hooks"));

        // Use current settings (already initialized or updated during onboarding)
        // If they are empty, serialize defaults
        SaveSettings();

        NeedsOnboarding = false;
    }

    internal static string? MigrateProjectColor(string? colorValue)
    {
        if (string.IsNullOrEmpty(colorValue))
            return null;

        if (Enum.TryParse<Colors>(colorValue, out _))
            return colorValue;

        return Colors.Slate.ToString();
    }

    private void MigrateProjectColors()
    {
        if (Settings?.Projects == null) return;

        var needsSave = false;
        foreach (var project in Settings.Projects)
        {
            var migrated = MigrateProjectColor(project.Color);
            var migratedStr = migrated ?? "";
            if (migratedStr != project.Color)
            {
                project.Color = migratedStr;
                needsSave = true;
            }
        }

        if (needsSave && File.Exists(ConfigPath))
        {
            var yaml = YamlHelper.SerializerCompact.Serialize(Settings);
            FileHelper.WriteAllText(ConfigPath, yaml);
        }
    }

    internal void SetTendrilHome(string tendrilHome)
    {
        TendrilHome = tendrilHome;
        ConfigPath = Path.Combine(TendrilHome, "config.yaml");

        // Load config if it exists at the new path
        if (File.Exists(ConfigPath))
        {
            var yaml = FileHelper.ReadAllText(ConfigPath);
            var loadedSettings = YamlHelper.Deserializer.Deserialize<TendrilSettings>(yaml);
            if (loadedSettings != null) Settings = loadedSettings;
        }

        MigrateProjectColors();
        _levelNamesCache = null;
        VariableExpansion.InitializeUserSecrets(TendrilHome);
        ExpandSettingsVariables();
    }

    /// <summary>
    ///     Expand variables in settings after loading config.
    /// </summary>
    private void ExpandSettingsVariables()
    {
        if (Settings == null) return;

        // Expand coding agent
        Settings.CodingAgent = VariableExpansion.ExpandVariables(Settings.CodingAgent, TendrilHome);

        // Expand plan template
        Settings.PlanTemplate = VariableExpansion.ExpandVariables(Settings.PlanTemplate, TendrilHome);

        // Expand LLM config
        if (Settings.Llm != null)
        {
            Settings.Llm.Endpoint = VariableExpansion.ExpandVariables(Settings.Llm.Endpoint, TendrilHome);
            Settings.Llm.ApiKey = VariableExpansion.ExpandVariables(Settings.Llm.ApiKey, TendrilHome);
            Settings.Llm.Model = VariableExpansion.ExpandVariables(Settings.Llm.Model, TendrilHome);
        }

        // Expand editor config
        if (Settings.Editor != null)
        {
            Settings.Editor.Command = VariableExpansion.ExpandVariables(Settings.Editor.Command, TendrilHome);
            Settings.Editor.Label = VariableExpansion.ExpandVariables(Settings.Editor.Label, TendrilHome);
        }

        // Expand promptware configs
        if (Settings.Promptwares != null)
            foreach (var kvp in Settings.Promptwares.ToList())
            {
                var config = kvp.Value;
                config.Model = VariableExpansion.ExpandVariables(config.Model, TendrilHome);

                if (config.AllowedTools != null)
                    for (var i = 0; i < config.AllowedTools.Count; i++)
                        config.AllowedTools[i] = VariableExpansion.ExpandVariables(config.AllowedTools[i], TendrilHome);
            }

        // Expand project configs
        if (Settings.Projects != null)
            foreach (var project in Settings.Projects)
            {
                project.Context = VariableExpansion.ExpandVariables(project.Context, TendrilHome);

                // Expand review actions
                if (project.ReviewActions != null)
                    foreach (var action in project.ReviewActions)
                    {
                        action.Condition = VariableExpansion.ExpandVariables(action.Condition, TendrilHome);
                        action.Action = VariableExpansion.ExpandVariables(action.Action, TendrilHome);
                    }

                // Expand hook actions
                if (project.Hooks != null)
                    foreach (var hook in project.Hooks)
                    {
                        hook.Condition = VariableExpansion.ExpandVariables(hook.Condition, TendrilHome);
                        hook.Action = VariableExpansion.ExpandVariables(hook.Action, TendrilHome);
                    }
            }

        // Expand verification prompts
        if (Settings.Verifications != null)
            foreach (var verification in Settings.Verifications)
                verification.Prompt = VariableExpansion.ExpandVariables(verification.Prompt, TendrilHome);
    }
}

public static class ProjectBadgeExtensions
{
    public static Badge WithProjectColor(this Badge badge, IConfigService config, string projectName)
    {
        var color = config.GetProjectColor(projectName);
        return color.HasValue ? badge.Color(color.Value) : badge;
    }
}