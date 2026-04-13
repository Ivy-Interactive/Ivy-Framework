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

    private static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TimerInterval = TimeSpan.FromMinutes(30);

    private readonly string _plansDirectory;
    private readonly ILogger<WorktreeCleanupService> _logger;
    private readonly IWorktreeLifecycleLogger? _lifecycleLogger;
    private Timer? _timer;

    public WorktreeCleanupService(string plansDirectory, ILogger<WorktreeCleanupService> logger, IWorktreeLifecycleLogger? lifecycleLogger = null)
    {
        _plansDirectory = plansDirectory;
        _logger = logger;
        _lifecycleLogger = lifecycleLogger;
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

            // First pass: remove recursive Plans artifacts within worktrees
            CleanupRecursiveArtifacts();

            // Clean up legacy .promptwares directories (pre-migration artifacts)
            CleanupLegacyPromptwaresDirs();

            // Second pass: regular plan-level worktree cleanup
            foreach (var dir in Directory.GetDirectories(_plansDirectory))
            {
                try
                {
                    CleanupPlanWorktrees(dir, _logger, _lifecycleLogger);
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

    internal void CleanupRecursiveArtifacts()
    {
        try
        {
            if (!Directory.Exists(_plansDirectory)) return;

            foreach (var planDir in Directory.GetDirectories(_plansDirectory))
            {
                try
                {
                    var worktreesDir = Path.Combine(planDir, "worktrees");
                    if (!Directory.Exists(worktreesDir)) continue;

                    var nestedPlans = Directory.GetDirectories(worktreesDir, "Plans", SearchOption.AllDirectories);

                    foreach (var nestedPlanDir in nestedPlans)
                    {
                        try
                        {
                            _logger.LogInformation(
                                "Removing recursive Plans artifact at {Path} (parent: {Parent})",
                                nestedPlanDir.Replace(_plansDirectory + Path.DirectorySeparatorChar, ""),
                                Path.GetFileName(planDir));

                            ForceDeleteDirectory(nestedPlanDir, _logger);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Failed to delete nested Plans directory {Path}",
                                nestedPlanDir.Replace(_plansDirectory + Path.DirectorySeparatorChar, ""));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to scan for recursive artifacts in {PlanFolder}",
                        Path.GetFileName(planDir));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Recursive artifact cleanup scan failed");
        }
    }

    internal static void CleanupPlanWorktrees(string planFolderPath, ILogger? logger = null, IWorktreeLifecycleLogger? lifecycleLogger = null)
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

        var planId = WorktreeLifecycleLogger.ExtractPlanId(planFolderPath);

        logger?.LogInformation("Cleaning up worktrees for plan {PlanFolder} (state: {State}, updated: {Updated})",
            Path.GetFileName(planFolderPath), planYaml.State, planYaml.Updated.ToString("o", CultureInfo.InvariantCulture));

        PlanReaderService.RemoveWorktrees(planFolderPath, logger, lifecycleLogger);

        // Safety net: RemoveWorktrees should have removed all directories
        foreach (var wtDir in Directory.GetDirectories(worktreesDir))
        {
            logger?.LogWarning(
                "Worktree directory still exists after RemoveWorktrees (this should not happen): {Path}",
                Path.GetFileName(wtDir));

            lifecycleLogger?.LogCleanupAttempt(planId, wtDir, "CleanupPlanWorktrees(fallback)", gitFileExists: false);

            try
            {
                ForceDeleteDirectory(wtDir, logger);
                lifecycleLogger?.LogCleanupSuccess(planId, wtDir);
            }
            catch (Exception ex)
            {
                lifecycleLogger?.LogCleanupFailed(planId, wtDir, ex.Message);
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
        const int maxRetries = 3;
        int[] delaysMs = [500, 1000, 1500];

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            if (attempt > 0)
            {
                logger?.LogDebug("ForceDeleteDirectory retry {Attempt}/{Max} for {Dir}",
                    attempt, maxRetries, Path.GetFileName(path));
                Thread.Sleep(delaysMs[attempt - 1]);
            }

            PlanReaderService.ClearReadOnlyAttributes(path);
            try
            {
                Directory.Delete(path, true);
                return;
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

                if (!Directory.Exists(path))
                    return;

                if (attempt < maxRetries)
                    continue;

                TryLogHandleHolders(path, logger);
                throw new IOException($"rmdir /s /q also failed to delete '{Path.GetFileName(path)}' after {maxRetries} retries", ex);
            }
        }
    }

    private static void TryLogHandleHolders(string path, ILogger? logger)
    {
        if (logger == null || !OperatingSystem.IsWindows()) return;
        try
        {
            var psi = new ProcessStartInfo("handle.exe", $"-accepteula -nobanner \"{path}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc == null) return;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(5000);
            if (!string.IsNullOrWhiteSpace(output))
                logger.LogWarning("Processes holding handles on {Dir}:\n{Output}", Path.GetFileName(path), output);
        }
        catch
        {
            // handle.exe not installed or failed — silently skip
        }
    }

    internal void CleanupLegacyPromptwaresDirs()
    {
        try
        {
            if (!Directory.Exists(_plansDirectory)) return;

            foreach (var planDir in Directory.GetDirectories(_plansDirectory))
            {
                try
                {
                    var worktreesDir = Path.Combine(planDir, "worktrees");
                    if (!Directory.Exists(worktreesDir)) continue;

                    var legacyDirs = Directory.GetDirectories(worktreesDir, ".promptwares", SearchOption.AllDirectories);

                    foreach (var legacyDir in legacyDirs)
                    {
                        try
                        {
                            _logger.LogInformation(
                                "Removing legacy .promptwares directory at {Path} (parent: {Parent})",
                                legacyDir.Replace(_plansDirectory + Path.DirectorySeparatorChar, ""),
                                Path.GetFileName(planDir));

                            ForceDeleteDirectory(legacyDir, _logger);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Failed to delete legacy .promptwares directory {Path}",
                                legacyDir.Replace(_plansDirectory + Path.DirectorySeparatorChar, ""));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to scan for legacy .promptwares in {PlanFolder}",
                        Path.GetFileName(planDir));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Legacy .promptwares cleanup scan failed");
        }
    }

    private void CleanupPlanWorktrees(string planFolderPath)
    {
        CleanupPlanWorktrees(planFolderPath, _logger, _lifecycleLogger);
    }
}
