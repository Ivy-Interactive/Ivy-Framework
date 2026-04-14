using System.Diagnostics;
using System.Text;
using Ivy.Helpers;

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
            process.WaitForExitOrKill(10000);
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
            process.WaitForExitOrKill(10000);
            return process?.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null; /* git may not be installed, or repo path invalid */
        }
    }

    public int? GetCommitFileCount(string repoPath, string commitHash)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"diff-tree --no-commit-id --name-only -r {commitHash}")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd();
            process.WaitForExitOrKill(10000);
            if (process?.ExitCode != 0 || output == null) return null;

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
        }
        catch
        {
            return null;
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
            process.WaitForExitOrKill(10000);
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

    public string? GetCombinedDiff(string repoPath, string firstCommit, string lastCommit)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"diff {firstCommit}^..{lastCommit}")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd();
            process.WaitForExitOrKill(10000);
            return process?.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }

    public List<(string Status, string FilePath)>? GetCombinedChangedFiles(string repoPath, string firstCommit, string lastCommit)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"diff --name-status {firstCommit}^..{lastCommit}")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd();
            process.WaitForExitOrKill(10000);
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
            return null;
        }
    }
}