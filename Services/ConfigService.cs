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

public record ProjectConfig
{
    public string Name { get; set; } = "";
    public List<string> Repos { get; set; } = new();
    public string Context { get; set; } = "";
}

public class TendrilSettings
{
    public string PlanFolder { get; set; } = @".plans";
    public string AgentCommand { get; set; } = "claude";
    public List<ProjectConfig> Projects { get; set; } = new();
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

        // Resolve relative paths against the Tendril project root.
        // System.AppContext.BaseDirectory is typically bin/Debug/net10.0/, so go up 3 levels.
        if (!Path.IsPathRooted(_settings.PlanFolder))
        {
            var tendrilRoot = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));
            _settings.PlanFolder = Path.GetFullPath(Path.Combine(tendrilRoot, _settings.PlanFolder));
        }
    }

    public TendrilSettings Settings => _settings;
    public string PlanFolder => _settings.PlanFolder;
    public List<ProjectConfig> Projects => _settings.Projects;
    public ProjectConfig? GetProject(string name) => _settings.Projects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
