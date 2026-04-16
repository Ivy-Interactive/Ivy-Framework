using System.Diagnostics;
using Ivy;
using Ivy.Widgets.ClaudeJsonRenderer;

var server = new Server();
server.AddApp<ClaudeJsonRendererDemo>();
await server.RunAsync();

[App]
class ClaudeJsonRendererDemo : ViewBase
{
    public override object Build()
    {
        var prompt = Context.UseState("Explain what the Ivy framework is in 2 sentences");
        var stream = Context.UseStream<string>();
        var running = Context.UseState(false);
        var resetToken = Context.UseState(0);
        var showThinking = Context.UseState(true);
        var showSystemEvents = Context.UseState(true);
        var autoScroll = Context.UseState(true);

        var sidebarContent = Layout.Vertical().Gap(2).Padding(2)
            | prompt.ToTextInput().Placeholder("Enter a prompt for Claude...")
            | new Button(running.Value ? "Running..." : "Run Claude")
                .Icon(Icons.Play)
                .OnClick(async () =>
                {
                    if (running.Value) return;
                    resetToken.Set(resetToken.Value + 1);
                    running.Set(true);
                    await Task.Run(() => RunClaude(prompt.Value, stream));
                    running.Set(false);
                })
                .Disabled(running.Value)
                .Width(Size.Full())
            | showThinking.ToSwitchInput(label: "Show Thinking")
            | showSystemEvents.ToSwitchInput(label: "Show System Events")
            | autoScroll.ToSwitchInput(label: "Auto Scroll");

        var mainContent = new ClaudeJsonRenderer()
            .Stream(stream)
            .ResetToken(resetToken.Value)
            .ShowThinking(showThinking.Value)
            .ShowSystemEvents(showSystemEvents.Value)
            .AutoScroll(autoScroll.Value)
            .Height(Size.Full());

        return new SidebarLayout(
            mainContent: mainContent,
            sidebarContent: sidebarContent
        ).Padding(0);
    }

    static void RunClaude(string prompt, IWriteStream<string> stream)
    {
        var escaped = prompt.Replace("\"", "\\\"");
        var psi = new ProcessStartInfo
        {
            FileName = "claude",
            Arguments = $"-p \"{escaped}\" --output-format stream-json --verbose --max-turns 50 --dangerously-skip-permissions",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process == null) return;

        while (!process.StandardOutput.EndOfStream)
        {
            var line = process.StandardOutput.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
                stream.Write(line);
        }

        process.WaitForExit();
    }
}
