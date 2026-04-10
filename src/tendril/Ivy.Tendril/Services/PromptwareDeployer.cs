using System.IO.Compression;

namespace Ivy.Tendril.Services;

internal static class PromptwareDeployer
{
    private const string ResourceName = "Ivy.Tendril.promptwares.zip";

    /// <summary>
    ///     Extracts embedded promptwares.zip to targetDir, preserving existing Logs/ and Memory/ directories.
    /// </summary>
    public static void Deploy(string targetDir)
    {
        var assembly = typeof(PromptwareDeployer).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream == null)
            throw new InvalidOperationException("Embedded promptwares.zip resource not found.");

        var tempDir = targetDir + "-deploying-" + Guid.NewGuid().ToString("N")[..8];

        try
        {
            // Extract to temp directory
            ZipFile.ExtractToDirectory(stream, tempDir);

            // Ensure target exists
            Directory.CreateDirectory(targetDir);

            // For each promptware subfolder, preserve Logs/ and Memory/
            foreach (var sourceSubDir in Directory.GetDirectories(tempDir))
            {
                var subDirName = Path.GetFileName(sourceSubDir);
                var targetSubDir = Path.Combine(targetDir, subDirName);

                // Move aside existing Logs/ and Memory/ if they exist
                var preservedDirs = new List<(string original, string aside)>();
                foreach (var preserve in new[] { "Logs", "Memory" })
                {
                    var existingDir = Path.Combine(targetSubDir, preserve);
                    if (Directory.Exists(existingDir))
                    {
                        var asideDir = existingDir + "-preserved-" + Guid.NewGuid().ToString("N")[..8];
                        Directory.Move(existingDir, asideDir);
                        preservedDirs.Add((existingDir, asideDir));
                    }
                }

                // Delete old promptware files (if target exists)
                if (Directory.Exists(targetSubDir))
                    Directory.Delete(targetSubDir, true);

                // Move new files from temp
                Directory.Move(sourceSubDir, targetSubDir);

                // Restore preserved directories
                foreach (var (original, aside) in preservedDirs)
                {
                    // Remove empty placeholder if it was created by the zip
                    if (Directory.Exists(original))
                        Directory.Delete(original, true);

                    Directory.Move(aside, original);
                }
            }

            // Copy any root-level files (e.g., .shared directory contents)
            foreach (var sourceFile in Directory.GetFiles(tempDir))
            {
                var targetFile = Path.Combine(targetDir, Path.GetFileName(sourceFile));
                File.Copy(sourceFile, targetFile, true);
            }
        }
        finally
        {
            // Clean up temp directory
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); }
                catch { /* Best effort */ }
            }
        }
    }

    public static bool IsEmbeddedAvailable()
    {
        var assembly = typeof(PromptwareDeployer).Assembly;
        return assembly.GetManifestResourceNames().Contains(ResourceName);
    }
}
