using System.Diagnostics;
using Ivy.Helpers;
using Ivy.Tendril.Services;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Commands;

public static class DoctorCommand
{
    private static readonly string[] RequiredSoftware = ["gh", "git"];
    private static readonly string[] OptionalSoftware = ["pandoc"];

    private static readonly Dictionary<string, string> VersionArgs = new()
    {
        ["gh"] = "--version",
        ["claude"] = "--version",
        ["codex"] = "--version",
        ["gemini"] = "--version",
        ["git"] = "--version",
        ["pwsh"] = "-Version",
        ["pandoc"] = "--version"
    };

    private static readonly Dictionary<string, string> HealthArgs = new()
    {
        ["gh"] = "auth status",
        ["claude"] = "-p \"ping\" --max-turns 1",
        ["codex"] = "login status"
    };

    public static int Handle(string[] args)
    {
        if (args.Length == 0 || args[0] != "doctor") return -1;

        if (args.Length > 1 && args[1] == "plans")
            return DoctorPlans(args.Skip(2).ToArray());

        return RunAsync().GetAwaiter().GetResult();
    }

    private static async Task<int> RunAsync()
    {
        var hasErrors = false;

        // 1. TENDRIL_HOME
        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME")?.Trim();
        if (!string.IsNullOrEmpty(tendrilHome) && tendrilHome.StartsWith("\"") && tendrilHome.EndsWith("\""))
            tendrilHome = tendrilHome[1..^1];

        PrintHeader("Environment");
        if (string.IsNullOrEmpty(tendrilHome))
        {
            PrintStatus("TENDRIL_HOME", "Not set", StatusKind.Error);
            Console.Error.WriteLine("\nTENDRIL_HOME is not set. Cannot continue.\n");
            return 1;
        }

        PrintStatus("TENDRIL_HOME", tendrilHome, StatusKind.Ok);

        var configPath = Path.Combine(tendrilHome, "config.yaml");
        if (File.Exists(configPath))
            PrintStatus("config.yaml", configPath, StatusKind.Ok);
        else
        {
            PrintStatus("config.yaml", "Not found", StatusKind.Error);
            hasErrors = true;
        }

        // 2. Load config
        ConfigService? configService = null;
        try
        {
            configService = new ConfigService();
            if (configService.NeedsOnboarding)
            {
                PrintStatus("Config", "Needs onboarding — config could not be loaded", StatusKind.Warn);
                hasErrors = true;
            }
        }
        catch (Exception ex)
        {
            PrintStatus("Config", $"Failed to load: {ex.Message}", StatusKind.Error);
            hasErrors = true;
        }

        // 3. Software checks
        PrintHeader("Software");

        var codingAgent = configService?.Settings.CodingAgent ?? "claude";
        var agentClis = GetAgentClis(configService);

        var allSoftware = RequiredSoftware
            .Concat(agentClis)
            .Concat(OptionalSoftware)
            .Distinct()
            .ToList();

        foreach (var sw in allSoftware)
        {
            var isRequired = RequiredSoftware.Contains(sw) || agentClis.Contains(sw);
            var versionArg = VersionArgs.GetValueOrDefault(sw, "--version");
            var installed = await CheckCommand(sw, versionArg);

            if (!installed)
            {
                var kind = isRequired ? StatusKind.Error : StatusKind.Warn;
                PrintStatus(sw, "Not found", kind);
                if (isRequired) hasErrors = true;
                continue;
            }

            if (HealthArgs.TryGetValue(sw, out var healthArg))
            {
                var health = await CheckHealth(sw, healthArg);
                switch (health)
                {
                    case HealthResult.Authenticated:
                        PrintStatus(sw, "Ready", StatusKind.Ok);
                        break;
                    case HealthResult.NotAuthenticated:
                        PrintStatus(sw, "Installed but not authenticated", isRequired ? StatusKind.Error : StatusKind.Warn);
                        if (isRequired) hasErrors = true;
                        break;
                    default:
                        PrintStatus(sw, "Installed (health check failed)", isRequired ? StatusKind.Error : StatusKind.Warn);
                        if (isRequired) hasErrors = true;
                        break;
                }
            }
            else
            {
                PrintStatus(sw, "OK", StatusKind.Ok);
            }
        }

        // PowerShell check with fallback (pwsh → powershell)
        var pwshInstalled = await CheckCommand("pwsh", "-Version");
        if (pwshInstalled)
            PrintStatus("powershell", "OK (pwsh)", StatusKind.Ok);
        else
        {
            var legacyInstalled = await CheckCommand("powershell", "-Version");
            if (legacyInstalled)
                PrintStatus("powershell", "OK (powershell)", StatusKind.Ok);
            else
            {
                PrintStatus("powershell", "Not found", StatusKind.Error);
                hasErrors = true;
            }
        }

        // 4. Database
        PrintHeader("Database");

        var dbPath = Path.Combine(tendrilHome, "tendril.db");
        if (!File.Exists(dbPath))
        {
            PrintStatus("tendril.db", "Not found", StatusKind.Error);
            hasErrors = true;
        }
        else
        {
            var fileInfo = new FileInfo(dbPath);
            PrintStatus("tendril.db", $"{fileInfo.Length / 1024.0:F0} KB", StatusKind.Ok);

            try
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();
                using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
                pragmaCmd.ExecuteNonQuery();

                using var integrityCmd = connection.CreateCommand();
                integrityCmd.CommandText = "PRAGMA integrity_check";
                var integrityResult = integrityCmd.ExecuteScalar()?.ToString();
                if (integrityResult == "ok")
                    PrintStatus("Integrity", "OK", StatusKind.Ok);
                else
                {
                    PrintStatus("Integrity", $"FAILED: {integrityResult}", StatusKind.Error);
                    hasErrors = true;
                }

                var migrator = new Database.DatabaseMigrator(connection);
                var currentVersion = migrator.GetCurrentVersion();
                var latestVersion = migrator.GetLatestVersion();
                if (currentVersion == latestVersion)
                    PrintStatus("Schema", $"v{currentVersion} (up to date)", StatusKind.Ok);
                else if (currentVersion < latestVersion)
                {
                    PrintStatus("Schema", $"v{currentVersion} → v{latestVersion} (needs migration)", StatusKind.Warn);
                    hasErrors = true;
                }
                else
                    PrintStatus("Schema", $"v{currentVersion} (newer than app v{latestVersion})", StatusKind.Warn);
            }
            catch (Exception ex)
            {
                PrintStatus("Connection", $"Failed: {ex.Message}", StatusKind.Error);
                hasErrors = true;
            }
        }

        // 5. Agent model verification
        if (configService?.Settings.CodingAgents is { Count: > 0 } agents)
        {
            PrintHeader("Agent Models");

            var activeAgent = agents.FirstOrDefault(a =>
                string.Equals(a.Name, codingAgent, StringComparison.OrdinalIgnoreCase) ||
                (a.Name == "ClaudeCode" && codingAgent == "claude") ||
                (a.Name == "Codex" && codingAgent == "codex") ||
                (a.Name == "Gemini" && codingAgent == "gemini"));

            foreach (var agent in agents)
            {
                var isActive = agent == activeAgent;
                var label = isActive ? $"{agent.Name} (active)" : agent.Name;

                if (agent.Profiles.Count == 0)
                {
                    PrintStatus(label, "No profiles configured", StatusKind.Warn);
                    continue;
                }

                var cliName = ResolveCliName(agent.Name, codingAgent);
                var cliInstalled = cliName != null && await CheckCommand(cliName, VersionArgs.GetValueOrDefault(cliName, "--version"));

                if (!cliInstalled)
                {
                    PrintStatus(label, $"CLI '{cliName ?? agent.Name}' not found — skipping", StatusKind.Warn);
                    continue;
                }

                Console.WriteLine($"  {label}");

                foreach (var profile in agent.Profiles)
                {
                    if (string.IsNullOrEmpty(profile.Model))
                    {
                        PrintStatus($"    {profile.Name}", "No model specified", StatusKind.Warn);
                        continue;
                    }

                    var modelResult = await VerifyModel(cliName!, agent.Name, profile.Model);
                    var profileLabel = $"    {profile.Name}: {profile.Model}";
                    switch (modelResult)
                    {
                        case ModelResult.Ok:
                            PrintStatus(profileLabel, "OK", StatusKind.Ok);
                            break;
                        case ModelResult.InvalidModel:
                            PrintStatus(profileLabel, "Invalid model ID", StatusKind.Error);
                            hasErrors = true;
                            break;
                        case ModelResult.AuthError:
                            PrintStatus(profileLabel, "Auth error", StatusKind.Error);
                            hasErrors = true;
                            break;
                        case ModelResult.Timeout:
                            PrintStatus(profileLabel, "Timeout (30s)", StatusKind.Warn);
                            break;
                        case ModelResult.Unknown:
                            PrintStatus(profileLabel, "Check failed", StatusKind.Warn);
                            break;
                    }
                }
            }
        }

        // Summary
        Console.WriteLine();
        if (hasErrors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Issues found. Fix the errors above and re-run `tendril doctor`.");
            Console.ResetColor();
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("All checks passed.");
        Console.ResetColor();
        return 0;
    }

    private static string[] GetAgentClis(ConfigService? configService)
    {
        var codingAgent = configService?.Settings.CodingAgent ?? "claude";
        var clis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (configService?.Settings.CodingAgents is { Count: > 0 } agents)
        {
            foreach (var agent in agents)
            {
                var cli = ResolveCliName(agent.Name, codingAgent);
                if (cli != null) clis.Add(cli);
            }
        }
        else
        {
            clis.Add(codingAgent);
        }

        return clis.ToArray();
    }

    private static string? ResolveCliName(string agentName, string codingAgent)
    {
        return agentName.ToLower() switch
        {
            "claude" or "claudecode" => "claude",
            "codex" => "codex",
            "gemini" => "gemini",
            _ => null
        };
    }

    private static async Task<ModelResult> VerifyModel(string cli, string agentName, string model)
    {
        var (args, timeout) = cli switch
        {
            "claude" => ($"-p \"ping\" --model {model} --max-turns 1", 30000),
            "gemini" => ($"-p \"Reply OK\" --model {model}", 30000),
            "codex" => ($"exec --model {model} \"Reply OK\"", 60000),
            _ => ("", 0)
        };

        if (string.IsNullOrEmpty(args)) return ModelResult.Unknown;

        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(MakeStartInfo(cli, args));
                if (proc is null) return ModelResult.Unknown;

                var stderr = "";
                proc.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) stderr += e.Data + "\n";
                };
                proc.BeginErrorReadLine();
                proc.StandardOutput.ReadToEnd();

                var exited = proc.WaitForExitOrKill(timeout);
                if (!exited) return ModelResult.Timeout;
                if (proc.ExitCode == 0) return ModelResult.Ok;

                if (stderr.Contains("model identifier is invalid", StringComparison.OrdinalIgnoreCase) ||
                    stderr.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                    stderr.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
                    return ModelResult.InvalidModel;

                if (stderr.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
                    stderr.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                    stderr.Contains("401", StringComparison.OrdinalIgnoreCase) ||
                    stderr.Contains("403", StringComparison.OrdinalIgnoreCase))
                    return ModelResult.AuthError;

                return ModelResult.InvalidModel;
            });
        }
        catch
        {
            return ModelResult.Unknown;
        }
    }

    private enum HealthResult { Authenticated, NotAuthenticated, CheckFailed }
    private enum ModelResult { Ok, InvalidModel, AuthError, Timeout, Unknown }
    private enum StatusKind { Ok, Warn, Error }

    private static ProcessStartInfo MakeStartInfo(string fileName, string arguments) => new()
    {
        FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
        Arguments = OperatingSystem.IsWindows() ? $"/S /c \"{fileName} {arguments}\"" : arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    private static async Task<bool> CheckCommand(string fileName, string arguments)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(MakeStartInfo(fileName, arguments));
                if (proc is null) return false;
                proc.StandardOutput.ReadToEnd();
                proc.StandardError.ReadToEnd();
                proc.WaitForExitOrKill(10000);
                return proc.ExitCode == 0;
            });
        }
        catch
        {
            return false;
        }
    }

    private static async Task<HealthResult> CheckHealth(string fileName, string arguments)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(MakeStartInfo(fileName, arguments));
                if (proc is null) return HealthResult.CheckFailed;
                proc.StandardOutput.ReadToEnd();
                proc.StandardError.ReadToEnd();
                var exited = proc.WaitForExitOrKill(30000);
                if (!exited) return HealthResult.CheckFailed;
                return proc.ExitCode == 0 ? HealthResult.Authenticated : HealthResult.NotAuthenticated;
            });
        }
        catch
        {
            return HealthResult.CheckFailed;
        }
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"── {title} ──");
        Console.ResetColor();
    }

    private static void PrintStatus(string label, string value, StatusKind kind)
    {
        var (symbol, color) = kind switch
        {
            StatusKind.Ok => ("✓", ConsoleColor.Green),
            StatusKind.Warn => ("!", ConsoleColor.Yellow),
            StatusKind.Error => ("✗", ConsoleColor.Red),
            _ => (" ", ConsoleColor.Gray)
        };

        Console.ForegroundColor = color;
        Console.Write($"  {symbol} ");
        Console.ResetColor();
        Console.Write(label.PadRight(40));
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ResetColor();
    }

    // --- doctor plans subcommand ---

    internal record PlanHealthResult(
        string Id,
        string Title,
        string State,
        int Worktrees,
        string Health,
        bool IsHealthy
    );

    private static string? GetArgValue(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    internal static int DoctorPlans(string[] args)
    {
        var showAll = args.Contains("--all");
        var fix = args.Contains("--fix");
        var stateFilter = GetArgValue(args, "--state");
        var worktreesOnly = args.Contains("--worktrees");

        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME")?.Trim();
        if (string.IsNullOrEmpty(tendrilHome))
        {
            Console.Error.WriteLine("TENDRIL_HOME is not set.");
            return 1;
        }

        var plansDir = Path.Combine(tendrilHome, "Plans");
        if (!Directory.Exists(plansDir))
        {
            Console.Error.WriteLine($"Plans directory not found: {plansDir}");
            return 1;
        }

        Console.WriteLine($"Scanning plans in: {plansDir}");
        Console.WriteLine();

        var allResults = ScanPlans(plansDir);

        if (fix)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Repairing unhealthy plans...");
            Console.ResetColor();
            Console.WriteLine();

            var repairedCount = 0;
            var failedCount = 0;

            foreach (var result in allResults.Where(r => !r.IsHealthy))
            {
                var matchingDir = Directory.GetDirectories(plansDir, $"{result.Id}-*").FirstOrDefault();
                if (matchingDir == null) continue;

                var repairResult = RepairPlan(matchingDir, result);
                if (repairResult.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ {result.Id}: {repairResult.Message}");
                    Console.ResetColor();
                    repairedCount++;
                }
                else if (repairResult.Message != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ {result.Id}: {repairResult.Message}");
                    Console.ResetColor();
                    failedCount++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Repair summary: {repairedCount} repaired, {failedCount} failed");
            Console.WriteLine();

            allResults = ScanPlans(plansDir);
        }

        IEnumerable<PlanHealthResult> results;
        if (showAll)
            results = allResults;
        else if (stateFilter != null)
            results = allResults.Where(r => r.State.Equals(stateFilter, StringComparison.OrdinalIgnoreCase));
        else if (worktreesOnly)
            results = allResults.Where(r => r.Worktrees > 0);
        else
            results = allResults.Where(r => !r.IsHealthy || r.State.Equals("Failed", StringComparison.OrdinalIgnoreCase));

        PrintPlansTable(results);
        PrintPlansSummary(allResults);

        return allResults.Any(r => !r.IsHealthy) ? 1 : 0;
    }

    internal static List<PlanHealthResult> ScanPlans(string plansDir)
    {
        var results = new List<PlanHealthResult>();

        var planDirs = Directory.GetDirectories(plansDir)
            .Where(d => System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(d), @"^\d{5}-"))
            .OrderBy(d => Path.GetFileName(d))
            .ToList();

        foreach (var dir in planDirs)
        {
            var folderName = Path.GetFileName(dir);
            var match = System.Text.RegularExpressions.Regex.Match(folderName, @"^(\d{5})-(.+)$");
            var id = match.Success ? match.Groups[1].Value : folderName;
            var title = match.Success ? match.Groups[2].Value : "";

            var yamlPath = Path.Combine(dir, "plan.yaml");
            var (yamlHealthy, yamlError, state) = CheckYamlHealth(yamlPath);
            var worktreeCount = CountWorktrees(dir);
            var hasNestedWorktrees = HasNestedWorktrees(dir);
            var hasStaleWorktrees = HasStaleWorktrees(dir);

            var recsError = CheckRecommendationsHealth(dir);

            var healthIssues = new List<string>();
            if (!yamlHealthy)
                healthIssues.Add($"YAML:{yamlError}");
            if (recsError != null)
                healthIssues.Add($"Recs:{recsError}");
            if (hasNestedWorktrees)
                healthIssues.Add("NestedWorktree");
            if (hasStaleWorktrees)
                healthIssues.Add("StaleWorktree");

            var health = healthIssues.Count == 0 ? "OK" : string.Join(",", healthIssues);

            results.Add(new PlanHealthResult(id, title, state, worktreeCount, health, healthIssues.Count == 0));
        }

        return results;
    }

    internal static (bool Healthy, string? Error, string State) CheckYamlHealth(string yamlPath)
    {
        if (!File.Exists(yamlPath))
            return (false, "Missing", "Unknown");

        try
        {
            var content = File.ReadAllText(yamlPath);
            if (string.IsNullOrWhiteSpace(content))
                return (false, "Empty", "Unknown");

            // Try strict deserialization first
            try
            {
                var plan = Services.YamlHelper.Deserializer.Deserialize<Apps.Plans.PlanYaml>(content);
                if (plan == null)
                    return (false, "Null after parse", "Unknown");

                if (string.IsNullOrWhiteSpace(plan.State))
                    return (false, "Missing state", "Unknown");

                if (string.IsNullOrWhiteSpace(plan.Project))
                    return (false, "Missing project", plan.State);

                if (string.IsNullOrWhiteSpace(plan.Title))
                    return (false, "Missing title", plan.State);

                if (plan.Repos == null || plan.Repos.Count == 0)
                    return (false, "No repos", plan.State);

                return (true, null, plan.State);
            }
            catch
            {
                // Fall back to regex extraction for plans with minor schema issues
                // (e.g. priority nested under dependsOn)
                var state = ExtractYamlField(content, "state") ?? "Unknown";
                var project = ExtractYamlField(content, "project");
                var title = ExtractYamlField(content, "title");

                if (string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(title))
                    return (false, "Parse error (missing required fields)", state);

                return (false, "Malformed YAML (readable)", state);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Read error: {ex.Message}", "Unknown");
        }
    }

    private static string? ExtractYamlField(string content, string field)
    {
        var match = System.Text.RegularExpressions.Regex.Match(content, $@"^{field}:\s*(.+)$",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim().Trim('\'', '"') : null;
    }

    internal static int CountWorktrees(string planPath)
    {
        var worktreesPath = Path.Combine(planPath, "worktrees");
        if (!Directory.Exists(worktreesPath))
            return 0;

        return Directory.GetDirectories(worktreesPath).Length;
    }

    internal static bool HasNestedWorktrees(string planPath)
    {
        var worktreesPath = Path.Combine(planPath, "worktrees");
        if (!Directory.Exists(worktreesPath))
            return false;

        try
        {
            foreach (var wtDir in Directory.GetDirectories(worktreesPath))
            {
                var nestedGit = Directory.EnumerateFileSystemEntries(wtDir, ".git", SearchOption.AllDirectories).ToList();
                if (nestedGit.Count > 1 || (nestedGit.Count == 1 && nestedGit[0] != Path.Combine(wtDir, ".git")))
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    internal static bool HasStaleWorktrees(string planPath)
    {
        var worktreesPath = Path.Combine(planPath, "worktrees");
        if (!Directory.Exists(worktreesPath))
            return false;

        try
        {
            foreach (var wtDir in Directory.GetDirectories(worktreesPath))
            {
                var gitFile = Path.Combine(wtDir, ".git");
                if (!File.Exists(gitFile))
                    return true;
            }
        }
        catch
        {
            // Ignore access errors
        }

        return false;
    }

    internal static string? CheckRecommendationsHealth(string planPath)
    {
        var recsPath = Path.Combine(planPath, "artifacts", "recommendations.yaml");
        if (!File.Exists(recsPath))
            return null;

        try
        {
            var content = File.ReadAllText(recsPath);
            if (string.IsNullOrWhiteSpace(content))
                return "Empty";

            var recs = Services.YamlHelper.Deserializer.Deserialize<List<Apps.Plans.RecommendationYaml>>(content);
            if (recs == null || recs.Count == 0)
                return "Empty list";

            return null;
        }
        catch (Exception ex)
        {
            return $"Parse error: {ex.Message}";
        }
    }

    internal static void PrintPlansTable(IEnumerable<PlanHealthResult> results)
    {
        const int idWidth = 5;
        const int planWidth = 33;
        const int stateWidth = 10;
        const int wtWidth = 10;

        Console.WriteLine(
            $"{"Id".PadRight(idWidth)}  {"Plan".PadRight(planWidth)}  {"State".PadRight(stateWidth)}  {"Worktrees".PadRight(wtWidth)}  Health");
        Console.WriteLine(
            $"{new string('-', idWidth)}  {new string('-', planWidth)}  {new string('-', stateWidth)}  {new string('-', wtWidth)}  ------");

        foreach (var r in results)
        {
            var truncatedTitle = r.Title.Length > planWidth
                ? r.Title[..(planWidth - 3)] + "..."
                : r.Title;

            Console.Write($"{r.Id.PadRight(idWidth)}  {truncatedTitle.PadRight(planWidth)}  {r.State.PadRight(stateWidth)}  {r.Worktrees.ToString().PadRight(wtWidth)}  ");

            if (r.IsHealthy)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(r.Health);
            }

            Console.ResetColor();
        }
    }

    internal static void PrintPlansSummary(List<PlanHealthResult> allResults)
    {
        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($"  Total plans: {allResults.Count}");
        Console.WriteLine($"  Healthy: {allResults.Count(r => r.IsHealthy)}");
        Console.WriteLine($"  Unhealthy: {allResults.Count(r => !r.IsHealthy)}");
        Console.WriteLine($"  With worktrees: {allResults.Count(r => r.Worktrees > 0)}");
    }

    internal record RepairResult(bool Success, string? Message);

    internal static RepairResult RepairPlan(string planPath, PlanHealthResult healthResult)
    {
        var repairs = new List<string>();

        try
        {
            if (healthResult.Health.Contains("YAML:"))
            {
                var yamlPath = Path.Combine(planPath, "plan.yaml");
                if (File.Exists(yamlPath))
                {
                    var content = File.ReadAllText(yamlPath);
                    var repaired = PlanReaderService.RepairPlanYaml(content);
                    if (repaired != content)
                    {
                        File.WriteAllText(yamlPath, repaired);
                        repairs.Add("repaired plan.yaml");
                    }
                }
            }

            if (healthResult.Health.Contains("Recs:"))
            {
                var recsPath = Path.Combine(planPath, "artifacts", "recommendations.yaml");
                if (File.Exists(recsPath))
                {
                    var content = File.ReadAllText(recsPath);
                    var repaired = PlanReaderService.RepairPlanYaml(content);
                    if (repaired != content)
                    {
                        File.WriteAllText(recsPath, repaired);
                        repairs.Add("repaired recommendations.yaml");
                    }
                }
            }

            if (repairs.Count == 0)
                return new RepairResult(false, null);

            return new RepairResult(true, string.Join(", ", repairs));
        }
        catch (Exception ex)
        {
            return new RepairResult(false, $"repair failed: {ex.Message}");
        }
    }

}
