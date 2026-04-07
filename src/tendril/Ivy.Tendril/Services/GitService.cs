using System.Diagnostics;
using System.Text;

namespace Ivy.Tendril.Services;

public class GitService : IGitService
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
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            var title = process?.StandardOutput.ReadLine();
            process?.WaitForExit();
            return process?.ExitCode == 0 ? title : null;
        }
        catch
        {
            return null; /* git may not be installed, or repo path invalid */
        }
    }

    public string? GetCommitDiff(string repoPath, string commitHash)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"show --format=\"\" --patch {commitHash}")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd();
            process?.WaitForExit();
            return process?.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null; /* git may not be installed, or repo path invalid */
        }
    }

    public List<(string Status, string FilePath)>? GetCommitFiles(string repoPath, string commitHash)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"diff-tree --no-commit-id --name-status -r {commitHash}")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd();
            process?.WaitForExit();
            if (process?.ExitCode != 0 || output == null) return null;

            var files = new List<(string Status, string FilePath)>();
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('\t', 2);
                if (parts.Length == 2)
                    files.Add((parts[0].Trim(), parts[1].Trim()));
            }

            return files;
        }
        catch
        {
            return null; /* git may not be installed, or repo path invalid */
        }
    }
}