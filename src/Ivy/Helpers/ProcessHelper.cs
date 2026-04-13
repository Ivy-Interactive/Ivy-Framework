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
        var pids = GetPidsOnPort("netstat", "-ano", output =>
        {
            var results = new List<int>();
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
                if (!int.TryParse(localAddress[(colonIndex + 1)..], out int linePort) || linePort != port)
                    continue;
                if (int.TryParse(pidStr, out int pid) && pid != 0)
                    results.Add(pid);
            }

            return results;
        });

        if (pids == null) return;

        foreach (var pid in pids)
        {
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

    private static void KillProcessUsingPortUnix(int port)
    {
        var pids = GetPidsOnPort("lsof", $"-ti tcp:{port}", ParseLsofOutput)
                   ?? GetPidsOnPort("ss", $"-tlnp sport = :{port}", ParseSsOutput);

        if (pids == null) return;

        foreach (var pid in pids)
        {
            if (pid == 0) continue;
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

    private static List<int>? GetPidsOnPort(string fileName, string arguments, Func<string, IEnumerable<int>> parser)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExitOrKill(10000);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                return null;

            return parser(output).ToList();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    private static IEnumerable<int> ParseLsofOutput(string output)
    {
        return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => int.TryParse(line.Trim(), out var pid) ? pid : 0)
            .Where(pid => pid != 0);
    }

    private static IEnumerable<int> ParseSsOutput(string output)
    {
        return Regex.Matches(output, @"pid=(\d+)")
            .Select(m => int.TryParse(m.Groups[1].Value, out var pid) ? pid : 0)
            .Where(pid => pid != 0)
            .Distinct();
    }

    public static void OpenBrowser(string localUrl)
    {
        if (string.IsNullOrWhiteSpace(localUrl))
            throw new ArgumentNullException(nameof(localUrl));

        Process.Start(new ProcessStartInfo(localUrl) { UseShellExecute = true });
    }
}
