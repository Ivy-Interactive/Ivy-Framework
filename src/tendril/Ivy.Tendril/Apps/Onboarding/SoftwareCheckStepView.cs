using System.Diagnostics;
using Ivy.Helpers;

namespace Ivy.Tendril.Apps.Onboarding;

public class SoftwareCheckStepView(
    IState<int> stepperIndex,
    IState<Dictionary<string, bool>?> checkResults,
    IState<Dictionary<string, bool?>?> healthResults) : ViewBase
{
    private static readonly Dictionary<string, string> HealthCheckHints = new()
    {
        ["gh"] = "Run `gh auth login` to authenticate",
        ["claude"] = "Run `claude` to log in, or check your plan/credits",
        ["codex"] = "Run `codex login` to authenticate",
        ["gemini"] = "Run `gemini` to log in, or check your API key"
    };

    public override object Build()
    {
        var isChecking = UseState(false);

        var hasAnyCodingAgent = checkResults.Value != null
                                && (checkResults.Value["claude"] || checkResults.Value["codex"] ||
                                    checkResults.Value["gemini"]);

        var ghHealthy = healthResults.Value?.GetValueOrDefault("gh") == true;
        var anyAgentHealthy = healthResults.Value != null
                              && (healthResults.Value.GetValueOrDefault("claude") == true
                                  || healthResults.Value.GetValueOrDefault("codex") == true
                                  || healthResults.Value.GetValueOrDefault("gemini") == true);

        var allRequiredPassed = checkResults.Value != null
                                && checkResults.Value["gh"] && ghHealthy
                                && hasAnyCodingAgent && anyAgentHealthy
                                && checkResults.Value["git"]
                                && checkResults.Value["powershell"];

        return Layout.Vertical()
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
                     | Text.H3("Results")
                     | new Table(
                         new TableRow(
                             new TableCell("Software").IsHeader(),
                             new TableCell("Status").IsHeader(),
                             new TableCell("Notes").IsHeader()
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
                       .Icon(Icons.CheckCheck, Align.Right)
                       .Loading(isChecking.Value)
                       .Disabled(isChecking.Value)
                       .OnClick(async () => await CheckSoftware())
                   : allRequiredPassed
                       ? new Button("Continue")
                           .Primary()
                           .Large()
                           .Icon(Icons.ArrowRight, Align.Right)
                           .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                       : Layout.Vertical()
                         | Text.Warning(
                             "Please install and authenticate all required software before continuing. GitHub CLI must be logged in, and at least one coding agent must be working.")
                            | new Button("Check Again")
                                .Outline()
                                .Icon(Icons.CheckCheck, Align.Right)
                                .OnClick(async () => await CheckSoftware())
               );

        async Task CheckSoftware()
        {
            isChecking.Set(true);

            var results = new Dictionary<string, bool>
            {
                ["gh"] = await CheckCommand("gh", "--version"),
                ["claude"] = await CheckCommand("claude", "--version"),
                ["codex"] = await CheckCommand("codex", "--version"),
                ["gemini"] = await CheckCommand("gemini", "--version"),
                ["git"] = await CheckCommand("git", "--version"),
                ["powershell"] = await CheckCommand("pwsh", "-Version")
                                 || await CheckCommand("powershell", "-Version"),
                ["pandoc"] = await CheckCommand("pandoc", "--version")
            };

            checkResults.Set(results);

            var health = new Dictionary<string, bool?>();

            if (results["gh"])
                health["gh"] = await CheckHealth("gh", "auth status");

            if (results["claude"])
                health["claude"] = await CheckHealth("claude", "-p \"ping\" --max-turns 1");

            if (results["codex"])
                health["codex"] = await CheckHealth("codex", "login status");

            if (results["gemini"])
                health["gemini"] = await CheckHealth("gemini", "-p \"Reply OK\" --max-turns 0");

            healthResults.Set(health);
            isChecking.Set(false);
        }
    }

    private static TableRow MakeSoftwareRow(
        Dictionary<string, bool> results,
        Dictionary<string, bool?>? health,
        string displayName,
        string key,
        string installUrl,
        bool isRequired)
    {
        var installed = results[key];
        var healthStatus = health?.GetValueOrDefault(key);

        string statusText;
        if (!installed)
            statusText = isRequired ? "❌ Not Found" : "❌ Not Installed";
        else if (healthStatus == true)
            statusText = "✅ Ready";
        else if (healthStatus == false)
            statusText = "⚠️ Installed but not authenticated";
        else
            statusText = "✅ Installed";

        TableCell notesCell;
        if (!installed)
            notesCell = new TableCell(new Button("Install").Inline().Url(installUrl));
        else if (healthStatus == false && HealthCheckHints.TryGetValue(key, out var hint))
            notesCell = new TableCell(hint);
        else
            notesCell = new TableCell("");

        return new TableRow(
            new TableCell(displayName),
            new TableCell(statusText),
            notesCell
        );
    }

    private static Task<bool> CheckCommand(string fileName, string arguments)
        => CheckProcess(fileName, arguments, 10000);

    private static Task<bool> CheckHealth(string fileName, string arguments)
        => CheckProcess(fileName, arguments, 15000);

    private static async Task<bool> CheckProcess(string fileName, string arguments, int timeoutMs)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
                    Arguments = OperatingSystem.IsWindows() ? $"/c \"{fileName}\" {arguments}" : arguments,
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
