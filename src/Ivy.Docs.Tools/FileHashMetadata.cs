using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ivy.Docs.Tools;

public static class FileHashMetadata
{
    private const string AttributeName = "hash";
    private const string StreamName = "hash";

    public static void WriteHash(string filePath, string hash)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var adsPath = filePath + ":" + StreamName;
                File.WriteAllText(adsPath, hash);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "xattr",
                    Arguments = $"-w {AttributeName} \"{hash.Replace("\"", "\\\"")}\" \"{filePath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                using var proc = Process.Start(psi) ?? throw new Exception("Failed to start process");
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    throw new Exception("Failed to write extended attribute: " + proc.StandardError.ReadToEnd());
            }
            else // Linux
            {
                var escapedHash = hash.Replace("\"", "\\\""); // escape quotes for shell
                var psi = new ProcessStartInfo
                {
                    FileName = "setfattr",
                    Arguments = $"-n {AttributeName} -v \"{escapedHash}\" \"{filePath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                using var proc = Process.Start(psi) ?? throw new Exception("Failed to start process");
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    throw new Exception("Failed to write extended attribute: " + proc.StandardError.ReadToEnd());
            }
        }
        catch (Exception)
        {
            // ignore
            //Console.WriteLine("Failed to write hash. Exception: " + e);
        }
    }

    public static string? ReadHash(string filePath)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var adsPath = filePath + ":" + StreamName;
                return File.Exists(adsPath) ? File.ReadAllText(adsPath).Trim() : null;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "xattr",
                    Arguments = $"-p {AttributeName} \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                using var proc = Process.Start(psi) ?? throw new Exception("Failed to start process");
                var output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();
                return proc.ExitCode == 0 ? output : null;
            }
            else // Linux
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "getfattr",
                    Arguments = $"-n {AttributeName} --only-values \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                using var proc = Process.Start(psi) ?? throw new Exception("Failed to start process");
                var output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();
                return proc.ExitCode == 0 ? output : null;
            }
        }
        catch (Exception)
        {
            //ignore
            return null;
        }
    }
}