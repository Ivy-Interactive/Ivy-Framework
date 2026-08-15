using System.Diagnostics;

namespace Ivy.Helpers;

public static class ProcessExtensions
{
    /// <summary>
    /// Waits for the process to exit within the specified timeout.
    /// If the timeout expires, kills the entire process tree.
    /// </summary>
    /// <returns>true if the process exited within the timeout; false if it was killed.</returns>
    public static bool WaitForExitOrKill(this Process? process, int timeoutMs)
    {
        if (process is null) return true;
        if (!process.WaitForExit(timeoutMs))
        {
            KillProcess(process);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Asynchronously waits for the process to exit within the specified timeout.
    /// If the timeout expires, kills the entire process tree.
    /// </summary>
    /// <returns>true if the process exited within the timeout; false if it was killed.</returns>
    public static async Task<bool> WaitForExitOrKillAsync(this Process? process, int timeoutMs)
    {
        if (process is null) return true;
        using var cts = new CancellationTokenSource(timeoutMs);
        return await WaitForProcessExitAsync(process, cts.Token);
    }

    /// <summary>
    /// Asynchronously waits for the process to exit, observing the given cancellation token.
    /// If cancelled, kills the entire process tree.
    /// </summary>
    /// <returns>true if the process exited normally; false if it was killed due to cancellation.</returns>
    public static async Task<bool> WaitForExitOrKillAsync(this Process? process, CancellationToken cancellationToken)
    {
        if (process is null) return true;
        return await WaitForProcessExitAsync(process, cancellationToken);
    }

    /// <summary>
    /// Waits for the OS process to terminate without blocking indefinitely on redirected stdout/stderr stream EOF.
    /// </summary>
    public static async Task<bool> WaitForProcessExitAsync(this Process? process, CancellationToken cancellationToken)
    {
        if (process is null) return true;
        if (process.HasExited) return true;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnExited(object? sender, EventArgs e) => tcs.TrySetResult(true);

        try
        {
            process.EnableRaisingEvents = true;
            process.Exited += OnExited;

            if (process.HasExited)
            {
                tcs.TrySetResult(true);
            }

            using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            await tcs.Task;

            // Once the OS process exits, allow up to 1 second for any in-flight stdout/stderr buffers to flush.
            try
            {
                using var drainCts = new CancellationTokenSource(1000);
                await process.WaitForExitAsync(drainCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Streams did not reach EOF within drain window (likely child process holding pipe open); proceed anyway.
            }
            catch (Exception)
            {
                // Ignore errors during stream draining
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            await KillProcessAsync(process);
            return false;
        }
        finally
        {
            try
            {
                process.Exited -= OnExited;
            }
            catch
            {
                // Ignore if process disposed
            }
        }
    }

    private static void KillProcess(Process process)
    {
        int? processId = null;
        try
        {
            processId = process.Id;
        }
        catch (InvalidOperationException)
        {
            // Process already disposed/exited
            return;
        }

        try
        {
            process.Kill(true);
            if (!process.WaitForExit(5000))
                CrashLog.Write($"[{DateTime.UtcNow:O}] Process {processId} did not exit within 5 seconds after Kill()");
        }
        catch (InvalidOperationException)
        {
            // Process already exited
        }
        catch (Exception ex)
        {
            CrashLog.Write($"[{DateTime.UtcNow:O}] Exception killing process {processId}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static async Task KillProcessAsync(Process process)
    {
        int? processId = null;
        try
        {
            processId = process.Id;
        }
        catch (InvalidOperationException)
        {
            // Process already disposed/exited
            return;
        }

        try
        {
            process.Kill(true);
            using var killTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (!process.HasExited)
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                void OnExited(object? sender, EventArgs e) => tcs.TrySetResult(true);
                process.EnableRaisingEvents = true;
                process.Exited += OnExited;
                try
                {
                    if (process.HasExited) tcs.TrySetResult(true);
                    using var reg = killTimeout.Token.Register(() => tcs.TrySetCanceled(killTimeout.Token));
                    await tcs.Task;
                }
                finally
                {
                    try { process.Exited -= OnExited; } catch { }
                }
            }
        }
        catch (OperationCanceledException)
        {
            CrashLog.Write($"[{DateTime.UtcNow:O}] Process {processId} did not exit within 5 seconds after Kill()");
        }
        catch (InvalidOperationException)
        {
            // Process already exited
        }
        catch (Exception ex)
        {
            CrashLog.Write($"[{DateTime.UtcNow:O}] Exception killing process {processId}: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
