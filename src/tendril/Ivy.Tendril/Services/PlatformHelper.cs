using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Ivy.Helpers;

namespace Ivy.Tendril.Services;

public static class PlatformHelper
{
    /// <summary>
    /// Returns true if condition evaluates to exit code 0, false otherwise.
    /// </summary>
    public static bool EvaluatePowerShellCondition(string condition, string workingDirectory, int timeoutMs = 5000)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "pwsh",
                Arguments = $"-NoProfile -Command \"if ({condition}) {{ exit 0 }} else {{ exit 1 }}\"",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc is not null)
            {
                if (!proc.WaitForExitOrKill(timeoutMs))
                    return false;
                return proc.ExitCode == 0;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to evaluate PowerShell condition: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Launches a PowerShell action. Returns false if pwsh is not found or the launch fails.
    /// </summary>
    public static bool RunPowerShellAction(string action, string workingDirectory)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "pwsh",
                Arguments = $"-NoProfile -Command \"{action}\"",
                WorkingDirectory = workingDirectory,
                UseShellExecute = true
            });
            return true;
        }
        catch (Win32Exception ex)
        {
            Console.Error.WriteLine($"Failed to run PowerShell action: {ex.Message}");
            return false;
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"Failed to run PowerShell action: {ex.Message}");
            return false;
        }
    }

    public static void OpenInTerminal(string workingDirectory)
    {
        var psi = new ProcessStartInfo { UseShellExecute = true };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            psi.FileName = "wt.exe";
            psi.Arguments = $"-d \"{workingDirectory}\"";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            psi.FileName = "open";
            psi.Arguments = $"-a Terminal \"{workingDirectory}\"";
        }
        else
        {
            psi.FileName = "xdg-open";
            psi.Arguments = workingDirectory;
        }

        Process.Start(psi);
    }

    public static bool OpenInEditor(string editorCommand, string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = editorCommand,
                Arguments = $"\"{target}\"",
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception)
        {
            // On macOS, fall back to 'open' which opens with the default app
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{target}\"",
                        UseShellExecute = true
                    });
                    return true;
                }
                catch { }
            }
            return false;
        }
    }

    public static void OpenInFileManager(string folderPath)
    {
        var psi = new ProcessStartInfo { UseShellExecute = true };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            psi.FileName = "explorer.exe";
            psi.Arguments = folderPath;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            psi.FileName = "open";
            psi.Arguments = folderPath;
        }
        else
        {
            psi.FileName = "xdg-open";
            psi.Arguments = folderPath;
        }

        Process.Start(psi);
    }
}