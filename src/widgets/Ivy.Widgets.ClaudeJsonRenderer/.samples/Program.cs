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

        return Layout.Vertical().Gap(3)
            | prompt.ToTextInput().Placeholder("Enter a prompt for Claude...")
            | new Button(running.Value ? "Running..." : "Run Claude").OnClick(async () =>
            {
                if (running.Value) return;
                running.Set(true);
                await Task.Run(() => RunClaude(prompt.Value, stream));
                running.Set(false);
            }).Disabled(running.Value)
            | new ClaudeJsonRenderer()
                .Stream(stream)
                .ShowThinking(true)
                .ShowSystemEvents(true)
                .Height(Size.Px(600));
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
