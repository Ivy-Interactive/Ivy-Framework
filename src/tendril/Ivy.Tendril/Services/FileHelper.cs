using System.Globalization;
using System.Text.RegularExpressions;

namespace Ivy.Tendril.Services;

/// <summary>
///     File I/O helpers that use FileShare.ReadWrite and retry on transient lock errors.
///     Prevents "file being used by another process" errors when multiple threads or
///     processes access the same file concurrently (e.g. plan.yaml, costs.csv).
/// </summary>
internal static class FileHelper
{
    private const int MaxRetries = 5;

    private static readonly Regex CompletedTimestampRegex =
        new(@"\*\*Completed:\*\*\s*(.+)", RegexOptions.Compiled);

    private static readonly int[] RetryDelaysMs = [50, 150, 350, 750, 1500];

    /// <summary>
    ///     Extracts the "**Completed:** &lt;timestamp&gt;" value from a log file.
    ///     Returns null if the file doesn't exist, can't be read, or has no completed timestamp.
    /// </summary>
    public static DateTime? ExtractCompletedTimestamp(string logFilePath)
    {
        try
        {
            foreach (var line in ReadAllLines(logFilePath))
            {
                var match = CompletedTimestampRegex.Match(line);
                if (match.Success && DateTime.TryParse(match.Groups[1].Value.Trim(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal, out var dt))
                    return dt;
            }
        }
        catch
        {
            /* Best-effort: file may be locked or missing */
        }

        return null;
    }

    public static string ReadAllText(string path)
    {
        for (var attempt = 0; ; attempt++)
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                Thread.Sleep(RetryDelaysMs[attempt]);
            }
    }

    public static string[] ReadAllLines(string path)
    {
        for (var attempt = 0; ; attempt++)
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                var lines = new List<string>();
                while (reader.ReadLine() is { } line)
                    lines.Add(line);
                return lines.ToArray();
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                Thread.Sleep(RetryDelaysMs[attempt]);
            }
    }

    public static void WriteAllText(string path, string contents)
    {
        for (var attempt = 0; ; attempt++)
            try
            {
                using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream);
                writer.Write(contents);
                return;
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                Thread.Sleep(RetryDelaysMs[attempt]);
            }
    }

    public static async Task<string> ReadAllTextAsync(string path)
    {
        for (var attempt = 0; ; attempt++)
            try
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                await using (stream.ConfigureAwait(false))
                {
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt]).ConfigureAwait(false);
            }
    }

    public static async Task WriteAllTextAsync(string path, string contents)
    {
        for (var attempt = 0; ; attempt++)
            try
            {
                var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                await using (stream.ConfigureAwait(false))
                {
                    await using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(contents).ConfigureAwait(false);
                    return;
                }
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt]).ConfigureAwait(false);
            }
    }

    public static void AppendAllText(string path, string contents)
    {
        for (var attempt = 0; ; attempt++)
            try
            {
                using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream);
                writer.Write(contents);
                return;
            }
            catch (IOException) when (attempt < MaxRetries)
            {
                Thread.Sleep(RetryDelaysMs[attempt]);
            }
    }
}