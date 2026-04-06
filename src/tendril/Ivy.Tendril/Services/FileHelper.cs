namespace Ivy.Tendril.Services;

/// <summary>
/// File I/O helpers that use FileShare.ReadWrite and retry on transient lock errors.
/// Prevents "file being used by another process" errors when multiple threads or
/// processes access the same file concurrently (e.g. plan.yaml, costs.csv).
/// </summary>
internal static class FileHelper
{
    private const int MaxRetries = 5;
    private static readonly int[] RetryDelaysMs = [50, 150, 350, 750, 1500];

    public static string ReadAllText(string path)
    {
        for (var attempt = 0; ; attempt++)
        {
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
    }

    public static string[] ReadAllLines(string path)
    {
        for (var attempt = 0; ; attempt++)
        {
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
    }

    public static void WriteAllText(string path, string contents)
    {
        for (var attempt = 0; ; attempt++)
        {
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
    }

    public static void AppendAllText(string path, string contents)
    {
        for (var attempt = 0; ; attempt++)
        {
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
}
