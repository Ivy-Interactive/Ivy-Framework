namespace Ivy.IvyML.Studio.Helpers;

/// <summary>
/// Exposes the dev build of the <c>ivyml</c> console CLI to a spawned process (the Claude PTY)
/// without requiring the tool to be installed globally. Mirrors the Tendril shim approach: in dev
/// there is no <c>ivyml</c> binary on PATH, so we generate a tiny shim that proxies to
/// <c>dotnet exec Ivy.IvyML.Console.dll</c> (built as <c>ivyml.dll</c>) and prepend it to PATH.
/// </summary>
public static class IvymlShim
{
    /// <summary>
    /// Returns a directory to prepend to PATH so a spawned process can invoke <c>ivyml ...</c>.
    /// Generates <c>ivyml.cmd</c> (Windows) and <c>ivyml</c> (bash) shims that exec the console DLL
    /// shipped next to this app. Returns null if the console DLL can't be located.
    /// </summary>
    public static string? EnsureShimDir()
    {
        var baseDir = System.AppContext.BaseDirectory;
        // The console project sets <AssemblyName>ivyml</AssemblyName>, so it builds as ivyml.dll.
        var dllPath = Path.Combine(baseDir, "ivyml.dll");
        if (!File.Exists(dllPath))
            return null;

        var shimDir = Path.Combine(Path.GetTempPath(), "ivyml-shim");
        Directory.CreateDirectory(shimDir);

        var cmdShim = Path.Combine(shimDir, "ivyml.cmd");
        File.WriteAllText(cmdShim, $"@dotnet exec \"{dllPath}\" %*\r\n");

        var bashDllPath = dllPath.Replace('\\', '/');
        var bashShim = Path.Combine(shimDir, "ivyml");
        File.WriteAllText(bashShim, $"#!/usr/bin/env bash\ndotnet exec '{bashDllPath}' \"$@\"\n");

        return shimDir;
    }

    /// <summary>
    /// Prepends the <c>ivyml</c> shim directory to the PATH entry of a PTY environment dictionary,
    /// so the spawned agent can run <c>ivyml draw/docs/icons</c> against the dev build.
    /// </summary>
    public static void ApplyEnvironment(IDictionary<string, string> env)
    {
        var shimDir = EnsureShimDir();
        if (shimDir == null)
            return;

        var current = env.TryGetValue("PATH", out var p) && !string.IsNullOrEmpty(p)
            ? p
            : Environment.GetEnvironmentVariable("PATH");
        env["PATH"] = $"{shimDir}{Path.PathSeparator}{current}";
    }
}
