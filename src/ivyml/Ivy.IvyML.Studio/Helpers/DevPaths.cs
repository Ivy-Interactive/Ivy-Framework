namespace Ivy.IvyML.Studio.Helpers;

/// <summary>
/// Resolves repo paths from the running Studio binary. Studio is a dev-only tool that always runs
/// out of <c>src/ivyml/Ivy.IvyML.Studio/bin/{Configuration}/{Tfm}/</c>, so the <c>src</c> root, the
/// build configuration and the target framework can all be recovered from the base directory. The
/// preview runner needs these to rebuild and launch the <c>ivyml</c> CLI out of process.
/// </summary>
public static class DevPaths
{
    /// <summary>Build configuration Studio was compiled with ("Debug"/"Release").</summary>
    public static string Configuration { get; }

    /// <summary>Target framework moniker Studio was compiled for (e.g. "net10.0").</summary>
    public static string TargetFramework { get; }

    /// <summary>The repo's <c>src</c> directory, or null when Studio runs outside the repo.</summary>
    public static string? SrcRoot { get; }

    static DevPaths()
    {
        // System-qualified: Ivy has its own AppContext type that would win here.
        var baseDir = new DirectoryInfo(System.AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        TargetFramework = baseDir.Name;
        Configuration = baseDir.Parent?.Name ?? "Debug";
        SrcRoot = FindSrcRoot(baseDir);
    }

    /// <summary>True when the paths below point at a real checkout and the preview can be managed.</summary>
    public static bool IsRepoCheckout => SrcRoot != null;

    /// <summary>The <c>ivyml</c> CLI project, rebuilt before each preview launch.</summary>
    public static string? ConsoleProject => SrcRoot is null
        ? null
        : Path.Combine(SrcRoot, "ivyml", "Ivy.IvyML.Console", "Ivy.IvyML.Console.csproj");

    /// <summary>Build output of the <c>ivyml</c> CLI. Copied aside before being run, never run from.</summary>
    public static string? ConsoleBinDir => SrcRoot is null
        ? null
        : Path.Combine(SrcRoot, "ivyml", "Ivy.IvyML.Console", "bin", Configuration, TargetFramework);

    /// <summary>Build output of the Ivy framework itself; watched as the "Ivy was recompiled" signal.</summary>
    public static string? IvyBinDir => SrcRoot is null
        ? null
        : Path.Combine(SrcRoot, "Ivy", "bin", Configuration, TargetFramework);

    /// <summary>
    /// Directory the CLI is copied to and launched from. Running a shadow copy keeps the real build
    /// output unlocked, so the agent can `dotnet build` at any time without the live preview process
    /// holding its DLLs open.
    /// </summary>
    public static string PreviewRuntimeDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ivy.IvyML.Studio",
        "PreviewRuntime");

    /// <summary>Platform name of the CLI apphost inside <see cref="PreviewRuntimeDir"/>.</summary>
    public static string PreviewExeName => OperatingSystem.IsWindows() ? "ivyml.exe" : "ivyml";

    private static string? FindSrcRoot(DirectoryInfo? dir)
    {
        // Walk up until we hit the directory holding the solution — that is `src`.
        for (; dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Ivy-Framework.slnx")))
                return dir.FullName;
        }

        return null;
    }
}
