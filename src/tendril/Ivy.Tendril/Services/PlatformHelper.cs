using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ivy.Tendril.Services;

public static class PlatformHelper
{
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