using System.Collections;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Pty.Net;

namespace Ivy.Pty;

public record PtyOptions
{
    public string? WorkingDirectory { get; init; }
    public Dictionary<string, string>? Environment { get; init; }
    public int Cols { get; init; } = 120;
    public int Rows { get; init; } = 30;
    public bool? ForceWinPty { get; init; }
}

public record PtyHandle(
    IWriteStream<string> Stream,
    Action<string> HandleInput,
    Action<int, int> HandleResize,
    Action Kill,
    bool Closed,
    int? ExitCode
);

public static class UsePtyExtensions
{
    public static PtyHandle UsePty(
        this IViewContext context,
        string[] commandLine,
        string? workingDirectory = null,
        PtyOptions? options = null)
    {
        options ??= new PtyOptions();
        var cwd = workingDirectory ?? options.WorkingDirectory;

        var stream = context.UseStream<string>();
        var closed = context.UseState(false);
        var exitCode = context.UseState<int?>(() => null);
        var pty = context.UseRef<IPtyConnection?>(() => null);
        var cts = context.UseRef<CancellationTokenSource?>(() => null);

        context.UseEffect(() =>
        {
            cts.Value = new CancellationTokenSource();
            var token = cts.Value.Token;

            _ = StartPtyAsync(commandLine, cwd, options, stream, pty, closed, exitCode, token);

            return Disposable.Create(() =>
            {
                cts.Value?.Cancel();
                KillPty(pty.Value);
                pty.Value?.Dispose();
            });
        }, EffectTrigger.OnMount());

        void HandleInput(string data)
        {
            if (pty.Value == null || closed.Value) return;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                pty.Value.WriterStream.Write(bytes);
                pty.Value.WriterStream.Flush();
            }
            catch
            {
                // Ignore write errors
            }
        }

        void HandleResize(int cols, int rows)
        {
            if (pty.Value == null || closed.Value) return;

            try
            {
                pty.Value.Resize(cols, rows);
            }
            catch
            {
                // Ignore resize errors
            }
        }

        void Kill()
        {
            if (pty.Value == null || closed.Value) return;
            KillPty(pty.Value);
        }

        return new PtyHandle(stream, HandleInput, HandleResize, Kill, closed.Value, exitCode.Value);
    }

    private static void KillPty(IPtyConnection? pty)
    {
        if (pty == null) return;

        try
        {
            pty.Kill();
        }
        catch
        {
            // Ignore kill errors
        }
    }

    private static async Task StartPtyAsync(
        string[] commandLine,
        string? workingDirectory,
        PtyOptions options,
        IWriteStream<string> stream,
        IState<IPtyConnection?> ptyRef,
        IState<bool> closed,
        IState<int?> exitCode,
        CancellationToken cancellationToken)
    {
        if (commandLine.Length == 0) return;

        var app = commandLine[0];
        var cwd = workingDirectory ?? Directory.GetCurrentDirectory();

        // ForceWinPty only applies on Windows - default to true to avoid missing conpty.dll
        var forceWinPty = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            && (options.ForceWinPty ?? true);

        // Merge with parent environment
        var env = new Dictionary<string, string>();
        foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
        {
            if (entry.Key is string key && entry.Value is string value)
            {
                env[key] = value;
            }
        }

        // Override with terminal-specific vars
        env["TERM"] = "xterm-256color";
        env["COLORTERM"] = "truecolor";

        // Override with user-specified vars
        if (options.Environment != null)
        {
            foreach (var (key, value) in options.Environment)
            {
                env[key] = value;
            }
        }

        var ptyOptions = new global::Pty.Net.PtyOptions
        {
            Name = "xterm-256color",
            Cols = options.Cols,
            Rows = options.Rows,
            Cwd = cwd,
            App = app,
            CommandLine = commandLine,
            ForceWinPty = forceWinPty,
            Environment = env
        };

        try
        {
            var pty = await PtyProvider.SpawnAsync(ptyOptions, cancellationToken);
            ptyRef.Set(pty);

            // Subscribe to process exit to capture exit code
            pty.ProcessExited += (sender, args) =>
            {
                exitCode.Set(pty.ExitCode);
                closed.Set(true);
            };

            _ = Task.Run(async () =>
            {
                var buffer = new byte[4096];
                try
                {
                    int bytesRead;
                    while ((bytesRead = await pty.ReaderStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        stream.Write(text);
                    }
                }
                catch (OperationCanceledException) { }
                catch
                {
                    // Ignore read errors
                }
                finally
                {
                    // Only set closed if not already set by ProcessExited
                    if (!closed.Value)
                    {
                        closed.Set(true);
                    }
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            stream.Write($"\r\n[Error starting PTY: {ex.Message}]\r\n");
            closed.Set(true);
        }
    }
}
