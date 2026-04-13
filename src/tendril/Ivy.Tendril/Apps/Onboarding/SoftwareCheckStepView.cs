using System.Diagnostics;
using Ivy.Helpers;

namespace Ivy.Tendril.Apps.Onboarding;

public class SoftwareCheckStepView(
    IState<int> stepperIndex,
    IState<Dictionary<string, bool>?> checkResults,
    IState<Dictionary<string, HealthCheckStatus?>?> healthResults) : ViewBase
{
    private static readonly Dictionary<string, string> HealthCheckHints = new()
    {
        ["gh"] = "Run `gh auth login` to authenticate",
        ["claude"] = "Run `claude` to log in, or check your plan/credits",
        ["codex"] = "Run `codex login` to authenticate",
        ["gemini"] = "Run `gemini` to authenticate (opens browser). Verify auth before selecting Gemini as your coding agent."
    };

    public override object Build()
    {
        var isChecking = UseState(false);

        var hasAnyCodingAgent = checkResults.Value != null
                                && (checkResults.Value["claude"] || checkResults.Value["codex"] ||
                                    checkResults.Value["gemini"]);

        var ghHealthy = healthResults.Value?.GetValueOrDefault("gh") == HealthCheckStatus.Authenticated;
        var anyAgentHealthy = healthResults.Value != null
                              && (healthResults.Value.GetValueOrDefault("claude") == HealthCheckStatus.Authenticated
                                  || healthResults.Value.GetValueOrDefault("codex") == HealthCheckStatus.Authenticated
                                  || healthResults.Value.GetValueOrDefault("gemini") == HealthCheckStatus.Authenticated);

        var allRequiredPassed = checkResults.Value != null
                                && checkResults.Value["gh"] && ghHealthy
                                && hasAnyCodingAgent && anyAgentHealthy
                                && checkResults.Value["git"]
                                && checkResults.Value["powershell"];

        return Layout.Vertical().Margin(0, 0, 0, 20)
               | Text.H2("Required Software")
               | Text.Markdown(
                   """
                   Tendril requires the following software to be installed:

                   **Required:**
                   - **Coding Agent** - At least one of: Claude Code CLI, Codex CLI, or Gemini CLI
                   - **GitHub CLI** - For PR creation and GitHub integration
                   - **Git** - For version control
                   - **PowerShell** - For running promptware and hooks

                   **Optional:**
                   - **Pandoc** - For PDF export functionality
                   """)
               | (checkResults.Value != null
                   ? Layout.Vertical()
                     | new Separator()
                     | (Layout.Horizontal()
                        | Text.H3("Results")
                        | new Spacer()
                        | new Button(!isChecking.Value ? "Recheck" : "Checking...")
                            .Outline()
                            .Small()
                            .Icon(Icons.RefreshCw, Align.Right)
                            .Loading(isChecking.Value)
                            .Disabled(isChecking.Value)
                            .OnClick(async () => await CheckSoftware()))
                     | new Table(
                         new TableRow(
                             new TableCell("Software").IsHeader(),
                             new TableCell("Status").IsHeader(),
                             new TableCell("Instructions").IsHeader()
                         ).IsHeader(),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "GitHub CLI", "gh", "https://cli.github.com/", true),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "Claude CLI", "claude", "https://docs.anthropic.com/en/docs/claude-code", false),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "Codex CLI", "codex", "https://openai.com/index/codex/", false),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "Gemini CLI", "gemini", "https://github.com/google-gemini/gemini-cli", false),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "Git", "git", "https://git-scm.com/downloads", true),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "PowerShell", "powershell", "https://github.com/PowerShell/PowerShell", true),
                         MakeSoftwareRow(checkResults.Value, healthResults.Value, "Pandoc (Optional)", "pandoc", "https://pandoc.org/installing.html", false)
                     ).Width(Size.Full())
                   : null!)
               | (checkResults.Value == null
                   ? new Button("Check Software")
                       .Primary()
                       .Large()
                       .Icon(Icons.ArrowRight, Align.Right)
                       .Loading(isChecking.Value)
                       .Disabled(isChecking.Value)
                       .OnClick(async () => await CheckSoftware())
                   : allRequiredPassed
                       ? new Button("Continue")
                           .Primary()
                           .Large()
                           .Icon(Icons.ArrowRight, Align.Right)
                           .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                       : Text.Muted("Please Wait...")
               );

        async Task CheckSoftware()
        {
            isChecking.Set(true);

            var ghTask = CheckCommand("gh", "--version");
            var claudeTask = CheckCommand("claude", "--version");
            var codexTask = CheckCommand("codex", "--version");
            var geminiTask = CheckCommand("gemini", "--version");
            var gitTask = CheckCommand("git", "--version");
            var powershellTask = CheckPowerShell();
            var pandocTask = CheckCommand("pandoc", "--version");

            await Task.WhenAll(ghTask, claudeTask, codexTask, geminiTask, gitTask, powershellTask, pandocTask);

            var results = new Dictionary<string, bool>
            {
                ["gh"] = ghTask.Result,
                ["claude"] = claudeTask.Result,
                ["codex"] = codexTask.Result,
                ["gemini"] = geminiTask.Result,
                ["git"] = gitTask.Result,
                ["powershell"] = powershellTask.Result,
                ["pandoc"] = pandocTask.Result
            };

            checkResults.Set(results);

            var ghHealthTask = results["gh"] ? CheckHealth("gh", "auth status") : null;
            // --max-turns 1 caps Claude at a single agentic turn so the process exits cleanly.
            // Without it, `claude -p` still starts an interactive session on some versions and
            // blocks until the 30 s timeout fires, falsely reporting "not authenticated".
            // NOTE: --max-turns is undocumented in `claude --help` (Claude Code 2.1.100) and
            // could be silently removed in a future release — exactly what happened to Gemini CLI
            // in 0.37.1+ (see plan 00030). If this check starts failing without auth changes,
            // verify --max-turns is still a recognized flag (`claude --help | grep max-turns`).
            var claudeHealthTask = results["claude"] ? CheckHealth("claude", "-p \"ping\" --max-turns 1") : null;
            var codexHealthTask = results["codex"] ? CheckHealth("codex", "login status") : null;
            // Gemini CLI has no non-interactive auth check command. Running `gemini -p` opens a browser
            // when unauthenticated (see plan 03037). Instead, check for ~/.gemini/oauth_creds.json.
            var geminiHealthTask = results["gemini"] ? CheckGeminiAuth() : null;

            var pendingTasks = new List<(string key, Task<HealthCheckStatus> task)>();
            if (ghHealthTask != null) pendingTasks.Add(("gh", ghHealthTask));
            if (claudeHealthTask != null) pendingTasks.Add(("claude", claudeHealthTask));
            if (codexHealthTask != null) pendingTasks.Add(("codex", codexHealthTask));
            if (geminiHealthTask != null) pendingTasks.Add(("gemini", geminiHealthTask));

            var health = new Dictionary<string, HealthCheckStatus?>();

            while (pendingTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(pendingTasks.Select(t => t.task));
                var (key, _) = pendingTasks.First(t => t.task == completedTask);
                pendingTasks.RemoveAll(t => t.task == completedTask);

                health[key] = await completedTask;
                healthResults.Set(new Dictionary<string, HealthCheckStatus?>(health));
            }

            isChecking.Set(false);
        }
    }

    private static TableRow MakeSoftwareRow(
        Dictionary<string, bool> results,
        Dictionary<string, HealthCheckStatus?>? health,
        string displayName,
        string key,
        string installUrl,
        bool isRequired)
    {
        var installed = results.GetValueOrDefault(key);
        var healthStatus = health?.GetValueOrDefault(key);

        string statusText;
        if (!installed)
            statusText = isRequired ? "❌ Not Found" : "❌ Not Installed";
        else if (healthStatus == HealthCheckStatus.Authenticated)
            statusText = "✅ Ready";
        else if (healthStatus == HealthCheckStatus.NotAuthenticated)
            statusText = "⚠️ Installed but not authenticated";
        else if (healthStatus == HealthCheckStatus.CheckFailed)
            statusText = "⚠️ Health check failed";
        else
            statusText = "✅ Installed";

        TableCell notesCell;
        if (!installed)
            notesCell = new TableCell(new Button("Install").Inline().Url(installUrl));
        else if (healthStatus == HealthCheckStatus.NotAuthenticated && HealthCheckHints.TryGetValue(key, out var hint))
            notesCell = new TableCell(hint);
        else if (healthStatus == HealthCheckStatus.CheckFailed)
            notesCell = new TableCell("Try clicking Recheck");
        else
            notesCell = new TableCell("");

        return new TableRow(
            new TableCell(displayName),
            new TableCell(statusText),
            notesCell
        );
    }

    private static async Task<bool> CheckPowerShell()
    {
        return await CheckCommand("pwsh", "-Version") || await CheckCommand("powershell", "-Version");
    }

    private static Task<bool> CheckCommand(string fileName, string arguments)
        => CheckProcess(fileName, arguments, 10000);

    private static async Task<HealthCheckStatus> CheckHealth(string fileName, string arguments)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
                    Arguments = OperatingSystem.IsWindows() ? $"/S /c \"{fileName} {arguments}\"" : arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (proc is null) return HealthCheckStatus.CheckFailed;
                var exited = proc.WaitForExitOrKill(30000);
                if (!exited) return HealthCheckStatus.CheckFailed;
                return proc.ExitCode == 0
                    ? HealthCheckStatus.Authenticated
                    : HealthCheckStatus.NotAuthenticated;
            });
        }
        catch
        {
            return HealthCheckStatus.CheckFailed;
        }
    }

    private static Task<HealthCheckStatus> CheckGeminiAuth()
    {
        return Task.Run(() =>
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var oauthCredsPath = Path.Combine(homeDir, ".gemini", "oauth_creds.json");

                if (File.Exists(oauthCredsPath))
                {
                    var fileInfo = new FileInfo(oauthCredsPath);
                    return fileInfo.Length > 0
                        ? HealthCheckStatus.Authenticated
                        : HealthCheckStatus.NotAuthenticated;
                }

                return HealthCheckStatus.NotAuthenticated;
            }
            catch
            {
                return HealthCheckStatus.CheckFailed;
            }
        });
    }

    private static async Task<bool> CheckProcess(string fileName, string arguments, int timeoutMs)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
                    Arguments = OperatingSystem.IsWindows() ? $"/S /c \"{fileName} {arguments}\"" : arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc.WaitForExitOrKill(timeoutMs);
                return proc?.ExitCode == 0;
            });
        }
        catch
        {
            return false;
        }
    }
}
