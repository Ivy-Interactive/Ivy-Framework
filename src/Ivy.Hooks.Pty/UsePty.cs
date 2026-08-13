using System.Collections;
using System.Reactive.Disposables;
using System.Text;
using Porta.Pty;

namespace Ivy.Hooks.Pty;

public record PtyOptions
{
    public string? WorkingDirectory { get; init; }
    public Dictionary<string, string>? Environment { get; init; }
    public int Cols { get; init; } = 120;
    public int Rows { get; init; } = 30;
    public Action<string>? OnOutput { get; init; }

    // Opt-in accumulated transcript, exposed via PtyHandle.Output. Off by default: most callers
    // only need the live OnOutput callback or the raw byte Stream, and an unbounded StringBuilder
    // per PTY session is wasted memory otherwise.
    public bool CaptureOutput { get; init; }

    // Character cap for the accumulated transcript (only relevant when CaptureOutput is true).
    // Once exceeded, the oldest text is dropped so PtyHandle.Output always holds a bounded tail
    // of the most recent output instead of growing unbounded for a long-running interactive CLI.
    public int MaxCaptureLength { get; init; } = 1_000_000;
}

// GetProcessId reads the spawned process's current OS process id, or null before the process
// has started (or after it has exited/been disposed). A live accessor rather than a plain field:
// the value is only known once the async spawn in UsePtyExtensions.UsePty completes, which may
// be after the render that returned this handle, so callers must read it at the point of use
// (e.g. inside their own effect cleanup) rather than capture it.
public record PtyHandle(
    IWriteStream<byte[]> Stream,
    Action<string> HandleInput,
    Action<int, int> HandleResize,
    Action Kill,
    bool Closed,
    int? ExitCode,
    Func<int?>? GetProcessId = null
)
{
    // Live accessor into the hook-state-held PtyOutputDecoder, not a captured value: the decoder
    // outlives any single PtyHandle instance (which is rebuilt every render), and Output must
    // reflect output that has arrived since this PtyHandle was constructed.
    internal Func<string>? OutputProvider { get; init; }

    /// <summary>
    /// The accumulated output text captured since the PTY started, when <see cref="PtyOptions.CaptureOutput"/>
    /// is set (empty string otherwise). Decoded across chunk boundaries and capped at
    /// <see cref="PtyOptions.MaxCaptureLength"/> characters, keeping the newest output. Contains raw
    /// ANSI escape sequences — pipe through <see cref="AnsiEscape.Strip"/> for readable text. Materializes
    /// a new string on each access, so read it when needed rather than on every render.
    /// </summary>
    public string Output => OutputProvider?.Invoke() ?? string.Empty;
}

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

        // Lives in hook state (not the PtyHandle record) so the decoder — and any transcript it is
        // accumulating — survives PtyHandle being rebuilt on every render.
        var decoder = context.UseRef<PtyOutputDecoder>(
            () => new PtyOutputDecoder(options.CaptureOutput, options.MaxCaptureLength));

        context.UseEffect(() =>
        {
            cts.Value = new CancellationTokenSource();
            var token = cts.Value.Token;

            _ = StartPtyAsync(commandLine, cwd, options, stream, pty, closed, exitCode, decoder.Value, token);

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

        return new PtyHandle(stream, HandleInput, HandleResize, Kill, closed.Value, exitCode.Value, () => pty.Value?.Pid)
        {
            OutputProvider = () => decoder.Value.Text
        };
    }

    // CommandLineToArgvW-compatible argument quoting (the same algorithm as .NET's
    // PasteArguments and PowerShell 7 "Standard" native-argument passing): embedded quotes
    // become \" and runs of backslashes preceding a quote are doubled. This round-trips
    // through every standard Windows argv parser, unlike Porta's "" -doubling default.
    private static string EscapeWindowsArgument(string arg)
    {
        if (arg.Length > 0 && arg.IndexOfAny([' ', '\t', '\n', '\v', '"']) < 0)
            return arg;

        var sb = new StringBuilder();
        sb.Append('"');
        for (var i = 0; i < arg.Length;)
        {
            var backslashes = 0;
            while (i < arg.Length && arg[i] == '\\') { backslashes++; i++; }
            if (i == arg.Length)
            {
                // Backslashes that precede the closing quote must be doubled so the quote
                // we append below is not itself escaped.
                sb.Append('\\', backslashes * 2);
            }
            else if (arg[i] == '"')
            {
                sb.Append('\\', backslashes * 2 + 1).Append('"');
                i++;
            }
            else
            {
                sb.Append('\\', backslashes).Append(arg[i]);
                i++;
            }
        }
        sb.Append('"');
        return sb.ToString();
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
        PtyOutputDecoder decoder,
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

        if (options.Environment != null)
        {
            foreach (var (key, value) in options.Environment)
            {
                if (string.IsNullOrEmpty(value))
                    env.Remove(key);
                else
                    env[key] = value;
            }
        }

        // Porta.Pty's default Windows arg formatting (WindowsArguments.Format) escapes embedded
        // quotes by DOUBLING them (" -> "") and wraps each arg in quotes. CommandLineToArgvW
        // consumers — Node, Rust, Go: i.e. every CLI we spawn (claude.exe, codex, …) — do not
        // decode that convention and silently truncate an argument at its first embedded quote.
        // So on Windows we pre-escape each arg with the standard backslash convention and hand
        // Porta a single verbatim command line. On Unix, Porta passes the array straight to
        // execvp(), so the array is forwarded untouched.
        var args = commandLine[1..];
        var verbatim = OperatingSystem.IsWindows();
        var ptyOptions = new global::Porta.Pty.PtyOptions
        {
            Name = "xterm-256color",
            Cols = options.Cols,
            Rows = options.Rows,
            Cwd = cwd,
            App = app,
            CommandLine = verbatim && args.Length > 0
                ? [string.Join(" ", args.Select(EscapeWindowsArgument))]
                : args,
            VerbatimCommandLine = verbatim,
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

            // Constant for the connection's lifetime, so the decoder never sees a gap in its byte
            // stream (skipping Decode on some chunks would desync its stateful UTF-8 decoder).
            var wantsText = options.OnOutput != null || options.CaptureOutput;

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

                        if (wantsText)
                        {
                            // Decoded across read boundaries by the stateful decoder (see
                            // PtyOutputDecoder) so multi-byte UTF-8 sequences split across two
                            // 4KB reads don't decode to U+FFFD replacement characters.
                            var text = decoder.Decode(buffer, bytesRead);
                            if (text.Length > 0) options.OnOutput?.Invoke(text);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch
                {
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
            var errorText = $"\r\n[Error starting PTY: {ex.Message}]\r\n";
            var errorMsg = Encoding.UTF8.GetBytes(errorText);
            stream.Write(errorMsg);

            // Feed the error text through the decoder (but not OnOutput, preserving today's
            // callback contract) so PtyHandle.Output explains a failed spawn too.
            decoder.Decode(errorMsg, errorMsg.Length);

            closed.Set(true);
        }
    }
}
