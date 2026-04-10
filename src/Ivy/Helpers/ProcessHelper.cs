using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Ivy.Helpers;

namespace Ivy;

public static class ProcessHelper
{
    public static bool IsProduction()
    {
        var env = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        return string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(env);
    }

    public static bool IsPortInUse(int port)
    {
        // Check active TCP listeners using the system's network information
        var ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
        var listeners = ipGlobalProperties.GetActiveTcpListeners();
        return listeners.Any(endpoint => endpoint.Port == port);
    }

    public static void KillProcessUsingPort(int port)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            KillProcessUsingPortWindows(port);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            KillProcessUsingPortUnix(port);
        else
            throw new PlatformNotSupportedException("KillProcessUsingPort is not supported on this platform.");
    }

    private static void KillProcessUsingPortWindows(int port)
    {
        using var netstat = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        netstat.Start();
        string output = netstat.StandardOutput.ReadToEnd();
        netstat.WaitForExitOrKill(10000);

        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var regex = new Regex(@"\s+");

        foreach (var line in lines)
        {
            if (!line.Trim().StartsWith("TCP"))
                continue;
            var parts = regex.Split(line.Trim());
            if (parts.Length < 5)
                continue;
            string localAddress = parts[1];
            string pidStr = parts[4];
            int colonIndex = localAddress.LastIndexOf(':');
            if (colonIndex == -1)
                continue;
            if (!int.TryParse(localAddress[(colonIndex + 1)..], out int linePort) || linePort != port) continue;
            if (!int.TryParse(pidStr, out int pid)) continue;
            if (pid == 0) continue;
            try
            {
                using var proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (Exception)
            {
                //ignore
            }
        }
    }

    private static void KillProcessUsingPortUnix(int port)
    {
        using var lsof = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "lsof",
                Arguments = $"-ti tcp:{port}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        lsof.Start();
        string output = lsof.StandardOutput.ReadToEnd();
        lsof.WaitForExitOrKill(10000);

        if (lsof.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            return;

        var pids = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var pidStr in pids)
        {
            if (!int.TryParse(pidStr.Trim(), out int pid) || pid == 0)
                continue;
            try
            {
                using var proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (Exception)
            {
                // ignore - process may have already exited
            }
        }
    }

    public static void OpenBrowser(string localUrl)
    {
        if (string.IsNullOrWhiteSpace(localUrl))
            throw new ArgumentNullException(nameof(localUrl));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {localUrl}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", localUrl);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", localUrl);
        }
    }
}
