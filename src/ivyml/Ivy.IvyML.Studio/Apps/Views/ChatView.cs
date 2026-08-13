using Ivy.Hooks.Pty;
using Ivy.IvyML.Studio.Helpers;
using Ivy.Widgets.Xterm;
using Xterm = Ivy.Widgets.Xterm;

namespace Ivy.IvyML.Studio.Apps.Views;

public class ChatView : ViewBase
{
    private readonly string? _systemPromptOverride;

    public ChatView(string? systemPrompt = null)
    {
        // Settable system prompt; defaults to the IvyML + wireframe-workflow explanation.
        _systemPromptOverride = systemPrompt;
    }

    public override object Build()
    {
        // IVYHOOK005: UsePty must be the first statement in Build(), so compute its arguments inline.
        // Run the agent inside the wireframe library so new 0000N.ivyml files land in the right place.
        var ptyHandle = Context.UsePty(
            BuildClaudeCommandLine(_systemPromptOverride),
            WireframeLibrary.Directory,
            new PtyOptions { Environment = BuildEnvironment() }
        );

        return new Xterm.Terminal()
            .Stream(ptyHandle.Stream)
            .OnInput(ptyHandle.HandleInput)
            .OnResize(ptyHandle.HandleResize)
            .Closed(ptyHandle.Closed)
            .AllowClipboard()
            .Loading("Starting Claude...")
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }

    private static string[] BuildClaudeCommandLine(string? systemPromptOverride)
    {
        var systemPrompt = string.IsNullOrWhiteSpace(systemPromptOverride)
            ? StudioPrompts.BuildIvyMl(WireframeLibrary.Directory)
            : systemPromptOverride!;

        // Write the system prompt to a file and pass it with --append-system-prompt-file. This
        // avoids fragile command-line quoting of multi-line prompts (which may contain XML angle
        // brackets that cmd.exe would otherwise treat as redirection).
        var promptFile = Path.Combine(Path.GetTempPath(), $"ivyml-studio-sysprompt-{Guid.NewGuid():N}.md");
        File.WriteAllText(promptFile, systemPrompt);

        var claudeArgs = new[]
        {
            "claude",
            "--dangerously-skip-permissions",
            "--append-system-prompt-file", promptFile,
        };

        // Resolve "claude" through the shell so npm shims (.cmd) work on Windows.
        return OperatingSystem.IsWindows()
            ? new[] { "cmd", "/c" }.Concat(claudeArgs).ToArray()
            : claudeArgs;
    }

    private static Dictionary<string, string> BuildEnvironment()
    {
        var env = new Dictionary<string, string>
        {
            ["FORCE_COLOR"] = "1",
        };

        // Put the dev `ivyml` CLI on PATH for the agent.
        IvymlShim.ApplyEnvironment(env);

        return env;
    }
}
