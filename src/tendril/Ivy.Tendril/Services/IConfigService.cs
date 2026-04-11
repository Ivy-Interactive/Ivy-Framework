namespace Ivy.Tendril.Services;

public interface IConfigService
{
    TendrilSettings Settings { get; }
    string TendrilHome { get; }
    string ConfigPath { get; }
    string PlanFolder { get; }
    List<ProjectConfig> Projects { get; }
    List<LevelConfig> Levels { get; }
    string[] LevelNames { get; }
    EditorConfig Editor { get; }
    bool NeedsOnboarding { get; }

    ProjectConfig? GetProject(string name);
    BadgeVariant GetBadgeVariant(string level);
    Colors? GetProjectColor(string projectName);
    void SaveSettings();
    void ReloadSettings();
    event EventHandler? SettingsReloaded;
    void SetPendingTendrilHome(string path);
    string? GetPendingTendrilHome();
    void SetPendingProject(ProjectConfig project);
    ProjectConfig? GetPendingProject();
    void SetPendingCodingAgent(string name);
    string? GetPendingCodingAgent();
    void SetPendingVerificationDefinitions(List<VerificationConfig> definitions);
    List<VerificationConfig>? GetPendingVerificationDefinitions();
    void CompleteOnboarding(string tendrilHome);
    void OpenInEditor(string path);
}