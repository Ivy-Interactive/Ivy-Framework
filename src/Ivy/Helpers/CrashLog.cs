namespace Ivy.Helpers;

public static class CrashLog
{
    private static readonly Lazy<string> LazyPath = new(() =>
    {
        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        var logDir = !string.IsNullOrEmpty(tendrilHome) ? tendrilHome : System.IO.Path.GetTempPath();
        return System.IO.Path.Combine(logDir, "crash.log");
    });

    public static string Path => LazyPath.Value;

    public static void Write(string message)
    {
        try
        {
            File.AppendAllText(Path, message + Environment.NewLine);
        }
        catch
        {
            // Last-resort: don't let logging crash the process
        }
    }
}
