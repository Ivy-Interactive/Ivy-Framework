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
    public string SlackEmoji { get; set; } = "";
    public List<RepoRef> Repos { get; set; } = new();
    public List<ProjectVerificationRef> Verifications { get; set; } = new();
    public string Context { get; set; } = "";
    public List<string> RepoPaths => Repos.Select(r => r.Path).ToList();
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

public class TendrilSettings
{
    public string TendrilData { get; set; } = "";
    public string PlanFolder { get; set; } = "";
    public string AgentCommand { get; set; } = "claude";
    public int JobTimeout { get; set; } = 30;
    public int StaleOutputTimeout { get; set; } = 10;
    public List<ProjectConfig> Projects { get; set; } = new();
    public List<VerificationConfig> Verifications { get; set; } = new();
    public string PlanTemplate { get; set; } = "";
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

    internal ConfigService(TendrilSettings settings)
    {
        _settings = settings;
    }

    public ConfigService()
    {
        var configPath = Path.Combine(System.AppContext.BaseDirectory, "config.yaml");
        if (File.Exists(configPath))
        {
            var yaml = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _settings = deserializer.Deserialize<TendrilSettings>(yaml) ?? new TendrilSettings();
        }
        else
        {
            _settings = new TendrilSettings();
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

        // Ensure Trash directory exists for discarded/duplicate requests
        if (!string.IsNullOrEmpty(_settings.TendrilData))
        {
            Directory.CreateDirectory(Path.Combine(_settings.TendrilData, "Trash"));
        }
    }

    public TendrilSettings Settings => _settings;
    public string TendrilData => _settings.TendrilData;
    public string PlanFolder => _settings.PlanFolder;
    public List<ProjectConfig> Projects => _settings.Projects;
    public List<LevelConfig> Levels => _settings.Levels;
    public string[] LevelNames => _settings.Levels.Select(l => l.Name).ToArray();
    public ProjectConfig? GetProject(string name) => _settings.Projects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public BadgeVariant GetBadgeVariant(string level) =>
        Enum.TryParse<BadgeVariant>(_settings.Levels.FirstOrDefault(l => l.Name == level)?.Badge ?? "Outline", out var v) ? v : BadgeVariant.Outline;

    public Colors? GetProjectColor(string projectName)
    {
        var colorStr = GetProject(projectName)?.Color;
        return !string.IsNullOrEmpty(colorStr) && Enum.TryParse<Colors>(colorStr, out var c) ? c : null;
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
