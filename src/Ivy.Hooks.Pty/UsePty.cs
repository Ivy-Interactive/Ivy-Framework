using System.Collections;
using System.Reactive.Disposables;
using System.Text;
using Ivy.Core.Hooks;
using Porta.Pty;

namespace Ivy.Hooks.Pty;

public record PtyOptions
{
    public string? WorkingDirectory { get; init; }
    public Dictionary<string, string>? Environment { get; init; }
    public int Cols { get; init; } = 120;
    public int Rows { get; init; } = 30;
    public Action<string>? OnOutput { get; init; }
}

public record PtyHandle(
    IWriteStream<byte[]> Stream,
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
        PtyOptions? options = null,
        IWriteStream<byte[]>? existingStream = null)
    {
        options ??= new PtyOptions();
        var cwd = workingDirectory ?? options.WorkingDirectory;

        var stream = existingStream ?? context.UseStream<byte[]>();
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
        IWriteStream<byte[]> stream,
        IState<IPtyConnection?> ptyRef,
        IState<bool> closed,
        IState<int?> exitCode,
        CancellationToken cancellationToken)
    {
        if (commandLine.Length == 0) return;

        var app = commandLine[0];
        var cwd = workingDirectory ?? Directory.GetCurrentDirectory();

        // Merge with parent environment
        var env = new Dictionary<string, string>();
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            if (entry is { Key: string key, Value: string value })
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

        var ptyOptions = new global::Porta.Pty.PtyOptions
        {
            Name = "xterm-256color",
            Cols = options.Cols,
            Rows = options.Rows,
            Cwd = cwd,
            App = app,
            CommandLine = commandLine[1..],
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
                        // Send raw bytes - JSON serializer will base64 encode automatically
                        var data = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, data, 0, bytesRead);
                        stream.Write(data);

                        // OnOutput callback gets the decoded text for parsing
                        var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        options.OnOutput?.Invoke(text);
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
            var errorMsg = Encoding.UTF8.GetBytes($"\r\n[Error starting PTY: {ex.Message}]\r\n");
            stream.Write(errorMsg);
            closed.Set(true);
        }
    }
}
