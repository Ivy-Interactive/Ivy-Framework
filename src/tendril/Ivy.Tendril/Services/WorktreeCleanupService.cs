using System.Diagnostics;
using System.Globalization;
using Ivy.Tendril.Apps.Plans;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Services;

public class WorktreeCleanupService : IStartable, IDisposable
{
    private static readonly HashSet<string> TerminalStates = new(StringComparer.OrdinalIgnoreCase)
        { "Completed", "Failed", "Skipped", "Icebox" };

    private static readonly TimeSpan GracePeriod = TimeSpan.FromHours(1);
    private static readonly TimeSpan TimerInterval = TimeSpan.FromMinutes(30);

    private readonly string _plansDirectory;
    private readonly ILogger<WorktreeCleanupService> _logger;
    private Timer? _timer;

    public WorktreeCleanupService(string plansDirectory, ILogger<WorktreeCleanupService> logger)
    {
        _plansDirectory = plansDirectory;
        _logger = logger;
    }

    public void Start()
    {
        _timer = new Timer(_ => RunCleanup(), null, TimeSpan.Zero, TimerInterval);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    internal void RunCleanup()
    {
        try
        {
            if (!Directory.Exists(_plansDirectory)) return;

            foreach (var dir in Directory.GetDirectories(_plansDirectory))
            {
                try
                {
                    CleanupPlanWorktrees(dir);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup worktrees for {PlanFolder}", Path.GetFileName(dir));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Worktree cleanup scan failed");
        }
    }

    internal static void CleanupPlanWorktrees(string planFolderPath, ILogger? logger = null)
    {
        var worktreesDir = Path.Combine(planFolderPath, "worktrees");
        if (!Directory.Exists(worktreesDir)) return;

        var planYamlPath = Path.Combine(planFolderPath, "plan.yaml");
        if (!File.Exists(planYamlPath)) return;

        PlanYaml? planYaml;
        try
        {
            var yaml = FileHelper.ReadAllText(planYamlPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            planYaml = deserializer.Deserialize<PlanYaml>(yaml);
        }
        catch
        {
            return;
        }

        if (planYaml == null) return;

        if (!TerminalStates.Contains(planYaml.State)) return;

        if (DateTime.UtcNow - planYaml.Updated < GracePeriod) return;

        logger?.LogInformation("Cleaning up worktrees for plan {PlanFolder} (state: {State}, updated: {Updated})",
            Path.GetFileName(planFolderPath), planYaml.State, planYaml.Updated.ToString("o", CultureInfo.InvariantCulture));

        PlanReaderService.RemoveWorktrees(planFolderPath, logger);

        // Force-delete any remaining worktree directories that git couldn't remove
        // (orphaned .git files, missing entries, Windows file locks, etc.)
        foreach (var wtDir in Directory.GetDirectories(worktreesDir))
        {
            try
            {
                ForceDeleteDirectory(wtDir, logger);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to force-delete worktree directory {Dir}", Path.GetFileName(wtDir));
            }
        }

        // Remove the worktrees directory itself
        try
        {
            if (Directory.Exists(worktreesDir))
                ForceDeleteDirectory(worktreesDir, logger);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to force-delete worktrees directory {Dir}", worktreesDir);
        }
    }

    /// <summary>
    ///     Recursively deletes a directory, falling back to <c>cmd /c rmdir /s /q</c> on
    ///     Windows when <see cref="Directory.Delete(string, bool)"/> fails with
    ///     <see cref="UnauthorizedAccessException"/> or <see cref="IOException"/>.
    /// </summary>
    /// <remarks>
    ///     Windows <c>Directory.Delete</c> can fail on deeply nested paths (such as
    ///     <c>node_modules</c>) due to long-path limits, transient file locks, or
    ///     NTFS permission quirks. <c>rmdir /s /q</c> handles these cases more robustly.
    /// </remarks>
    internal static void ForceDeleteDirectory(string path, ILogger? logger = null)
    {
        PlanReaderService.ClearReadOnlyAttributes(path);
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            if (!OperatingSystem.IsWindows()) throw;

            logger?.LogInformation("Directory.Delete failed for {Dir}, falling back to rmdir /s /q",
                Path.GetFileName(path));

            var psi = new ProcessStartInfo("cmd.exe", $"/c rmdir /s /q \"{path}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit(30000);

            if (Directory.Exists(path))
                throw new IOException($"rmdir /s /q also failed to delete '{Path.GetFileName(path)}'", ex);
        }
    }

    private void CleanupPlanWorktrees(string planFolderPath)
    {
        CleanupPlanWorktrees(planFolderPath, _logger);
    }
}
