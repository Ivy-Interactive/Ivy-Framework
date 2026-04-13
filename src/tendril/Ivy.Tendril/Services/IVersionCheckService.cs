namespace Ivy.Tendril.Services;

public interface IVersionCheckService
{
    Task<VersionInfo> CheckForUpdatesAsync();
}

public record VersionInfo(
    string CurrentVersion,
    string? LatestVersion,
    bool HasUpdate,
    DateTime? LastChecked);
