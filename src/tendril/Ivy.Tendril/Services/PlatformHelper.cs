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