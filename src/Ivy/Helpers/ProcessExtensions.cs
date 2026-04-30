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
        try
        {
            await process.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            await KillProcessAsync(process);
            return false;
        }
    }

    /// <summary>
    /// Asynchronously waits for the process to exit, observing the given cancellation token.
    /// If cancelled, kills the entire process tree.
    /// </summary>
    /// <returns>true if the process exited normally; false if it was killed due to cancellation.</returns>
    public static async Task<bool> WaitForExitOrKillAsync(this Process? process, CancellationToken cancellationToken)
    {
        if (process is null) return true;
        try
        {
            await process.WaitForExitAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            await KillProcessAsync(process);
            return false;
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
            await process.WaitForExitAsync(killTimeout.Token);
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
