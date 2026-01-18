using System.Diagnostics;
using System.Reactive.Disposables;
using Ivy;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Xterm;

var server = new Server();
server.AddApp<TerminalView>();
await server.RunAsync();

[App]
class TerminalView : ViewBase
{
    private Process? _process;
    private StreamWriter? _stdin;

    public override object Build()
    {
        var output = UseStream<string>();

        UseEffect(() =>
        {
            StartProcess(output);
            return Disposable.Create(KillProcess);
        }, OnMount());

        return new Terminal()
            .Stream(output)
            .HandleInput(SendToProcess)
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }

    private void StartProcess(IWriteStream<string> output)
    {
        var helloAppPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".console", "HelloApp");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run",
            WorkingDirectory = Path.GetFullPath(helloAppPath),
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            Environment = { ["TERM"] = "xterm-256color" }
        };

        _process = new Process { StartInfo = startInfo };

        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                output.Write(e.Data + "\r\n");
                Console.WriteLine("Wrote to terminal: " + e.Data);
            }
        };

        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                output.Write("\x1b[31m" + e.Data + "\x1b[0m\r\n");
        };

        _process.Start();
        _stdin = _process.StandardInput;

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    private void SendToProcess(string data)
    {
        if (_stdin == null) return;

        // Handle special keys
        if (data == "\r" || data == "\n")
        {
            _stdin.WriteLine();
        }
        else
        {
            _stdin.Write(data);
        }

        _stdin.Flush();
    }

    private void KillProcess()
    {
        try
        {
            Console.WriteLine("Cleaning up process...");
            _stdin?.Close();
            _process?.Kill();
            _process?.Dispose();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
