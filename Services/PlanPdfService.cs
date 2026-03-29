using System.Diagnostics;

namespace Ivy.Tendril.Services;

public class PlanPdfService
{
    public byte[] GeneratePdf(string title, int planId, string markdownContent)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "tendril-pdf", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var inputPath = Path.Combine(tempDir, "input.md");
            var outputPath = Path.Combine(tempDir, "output.pdf");

            // Prepend title as H1 if not already present
            var content = markdownContent ?? "";
            if (!content.TrimStart().StartsWith("# "))
            {
                content = $"# #{planId} {title}\n\n{content}";
            }

            File.WriteAllText(inputPath, content);

            var psi = new ProcessStartInfo
            {
                FileName = "pandoc",
                Arguments = $"\"{inputPath}\" -o \"{outputPath}\" --pdf-engine=xelatex -V geometry:margin=2.5cm -V fontsize=11pt -V header-includes=\"\\usepackage{{fancyhdr}}\\pagestyle{{fancy}}\\fancyhead[L]{{Ivy Tendril — Plan \\#{planId}}}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(30000);

            if (process?.ExitCode != 0 || !File.Exists(outputPath))
            {
                var error = process?.StandardError.ReadToEnd() ?? "Unknown error";
                throw new InvalidOperationException($"pandoc failed: {error}");
            }

            return File.ReadAllBytes(outputPath);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    /// <summary>
    /// Generate PDF for a plan folder, writing intermediary files to the plan's temp/ directory.
    /// </summary>
    public byte[] GeneratePdfFromPlanFolder(string planFolderPath, string title, int planId, string markdownContent)
    {
        var tempDir = Path.Combine(planFolderPath, "temp", "pdf");
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, "input.md");
        var outputPath = Path.Combine(tempDir, "output.pdf");

        var content = markdownContent ?? "";
        if (!content.TrimStart().StartsWith("# "))
        {
            content = $"# #{planId} {title}\n\n{content}";
        }

        File.WriteAllText(inputPath, content);

        var psi = new ProcessStartInfo
        {
            FileName = "pandoc",
            Arguments = $"\"{inputPath}\" -o \"{outputPath}\" --pdf-engine=xelatex -V geometry:margin=2.5cm -V fontsize=11pt -V header-includes=\"\\usepackage{{fancyhdr}}\\pagestyle{{fancy}}\\fancyhead[L]{{Ivy Tendril — Plan \\#{planId}}}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        process?.WaitForExit(30000);

        if (process?.ExitCode != 0 || !File.Exists(outputPath))
        {
            var error = process?.StandardError.ReadToEnd() ?? "Unknown error";
            throw new InvalidOperationException($"pandoc failed: {error}");
        }

        return File.ReadAllBytes(outputPath);
    }
}
