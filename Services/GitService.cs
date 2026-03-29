using System.Diagnostics;

namespace Ivy.Tendril.Services;

public class GitService
{
    public string? GetCommitTitle(string repoPath, string commitHash)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"log -1 --format=%s {commitHash}")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            var title = process?.StandardOutput.ReadLine();
            process?.WaitForExit();
            return process?.ExitCode == 0 ? title : null;
        }
        catch { return null; }
    }
}
