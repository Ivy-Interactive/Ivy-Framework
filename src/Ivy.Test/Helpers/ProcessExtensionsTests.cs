using System.Diagnostics;
using Ivy.Helpers;

namespace Ivy.Test.Helpers;

public class ProcessExtensionsTests
{
    [Fact]
    public void WaitForExitOrKill_ProcessExitsBeforeTimeout_ReturnsTrue()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-c \"exit 0\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        var result = process.WaitForExitOrKill(10000);

        Assert.True(result);
    }

    [Fact]
    public void WaitForExitOrKill_NullProcess_ReturnsTrue()
    {
        Process? process = null;

        var result = process.WaitForExitOrKill(10000);

        Assert.True(result);
    }

    [Fact]
    public void WaitForExitOrKill_ProcessTimesOut_ReturnsFalseAndKills()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c ping -n 60 127.0.0.1 >nul" : "-c \"sleep 60\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        var result = process.WaitForExitOrKill(100);

        Assert.False(result);
        Assert.True(process!.HasExited);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_ProcessExitsBeforeTimeout_ReturnsTrue()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-c \"exit 0\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        var result = await process.WaitForExitOrKillAsync(10000);

        Assert.True(result);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_NullProcess_ReturnsTrue()
    {
        Process? process = null;

        var result = await process.WaitForExitOrKillAsync(10000);

        Assert.True(result);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_ProcessTimesOut_ReturnsFalseAndKills()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c ping -n 60 127.0.0.1 >nul" : "-c \"sleep 60\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        var result = await process.WaitForExitOrKillAsync(100);

        Assert.False(result);
        Assert.True(process!.HasExited);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_WithToken_ProcessExitsBeforeCancel_ReturnsTrue()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-c \"exit 0\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        var result = await process.WaitForExitOrKillAsync(CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_WithToken_NullProcess_ReturnsTrue()
    {
        Process? process = null;

        var result = await process.WaitForExitOrKillAsync(CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_WithToken_Cancelled_ReturnsFalseAndKills()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c ping -n 60 127.0.0.1 >nul" : "-c \"sleep 60\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        using var cts = new CancellationTokenSource(100);
        var result = await process.WaitForExitOrKillAsync(cts.Token);

        Assert.False(result);
        Assert.True(process!.HasExited);
    }

    [Fact]
    public async Task WaitForExitOrKillAsync_PostKillTimeout_CompletesWithinReasonableTime()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c ping -n 120 127.0.0.1 >nul" : "-c \"sleep 120\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        var sw = Stopwatch.StartNew();
        using var cts = new CancellationTokenSource(100);
        var result = await process.WaitForExitOrKillAsync(cts.Token);
        sw.Stop();

        Assert.False(result);
        Assert.True(process!.HasExited);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(10),
            $"Post-kill should complete within 10s (5s kill timeout + margin), took {sw.Elapsed}");
    }
}
