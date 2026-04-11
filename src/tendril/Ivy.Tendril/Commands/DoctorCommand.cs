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
}
