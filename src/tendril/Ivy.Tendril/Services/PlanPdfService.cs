using System.Diagnostics;
using System.Text;

namespace Ivy.Tendril.Services;

public class PlanPdfService
{
    public byte[] GeneratePdf(string title, int planId, string markdownContent)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "tendril-pdf", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            return RunPandoc(tempDir, title, planId, markdownContent);
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    ///     Generate PDF for a plan folder, writing intermediary files to the plan's temp/ directory.
    /// </summary>
    public byte[] GeneratePdfFromPlanFolder(string planFolderPath, string title, int planId, string markdownContent)
    {
        var tempDir = Path.Combine(planFolderPath, "temp", "pdf");
        Directory.CreateDirectory(tempDir);

        return RunPandoc(tempDir, title, planId, markdownContent);
    }

    private byte[] RunPandoc(string tempDir, string title, int planId, string markdownContent)
    {
        var inputPath = Path.Combine(tempDir, "input.md");
        var outputPath = Path.Combine(tempDir, "output.pdf");

        // Prepend title as H1 if not already present
        var content = markdownContent ?? "";
        if (!content.TrimStart().StartsWith("# ")) content = $"# #{planId} {title}\n\n{content}";

        FileHelper.WriteAllText(inputPath, content);

        var psi = new ProcessStartInfo
        {
            FileName = "pandoc",
            Arguments =
                $"\"{inputPath}\" -o \"{outputPath}\" --pdf-engine=xelatex -V geometry:margin=2.5cm -V fontsize=11pt -V header-includes=\"\\usepackage{{fancyhdr}}\\pagestyle{{fancy}}\\fancyhead[L]{{Ivy Tendril — Plan \\#{planId}}}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start pandoc process");

        // Read streams asynchronously to avoid deadlock when pipe buffer fills
        var stderrTask = process.StandardError.ReadToEndAsync();
        var stdoutTask = process.StandardOutput.ReadToEndAsync();

        var exited = process.WaitForExit(30000);
        if (!exited)
        {
            process.Kill();
            throw new InvalidOperationException("pandoc timed out after 30 seconds");
        }

        var error = stderrTask.Result;

        if (process.ExitCode != 0 || !File.Exists(outputPath))
            throw new InvalidOperationException($"pandoc failed (exit {process.ExitCode}): {error}");

        return File.ReadAllBytes(outputPath);
    }
}