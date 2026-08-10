using System.Globalization;

namespace Ivy.IvyML.Studio.Helpers;

/// <summary>
/// A snapshot of the current "latest" wireframe in the library.
/// </summary>
public record WireframeSnapshot(string? Path, string? Content)
{
    public string? Name => Path is null ? null : System.IO.Path.GetFileName(Path);
    public static readonly WireframeSnapshot Empty = new(null, null);
}

/// <summary>
/// File-backed store of IvyML wireframes under local app data
/// (<c>%LOCALAPPDATA%/Ivy.IvyML.Studio/Wireframes</c>). Designs are append-only: each new design is
/// written as a new zero-padded file (<c>00001.ivyml</c>, <c>00002.ivyml</c>, ...) and existing
/// files are never edited. The Studio agent writes here; the code/preview panels read the latest.
/// </summary>
public static class WireframeLibrary
{
    /// <summary>Absolute path to the wireframes directory; created on first access.</summary>
    public static string Directory
    {
        get
        {
            var dir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Ivy.IvyML.Studio",
                "Wireframes");
            System.IO.Directory.CreateDirectory(dir);
            return dir;
        }
    }

    /// <summary>Returns the path of the highest-numbered <c>*.ivyml</c> file, or null if none.</summary>
    public static string? GetLatestFile()
    {
        if (!System.IO.Directory.Exists(Directory))
            return null;

        return System.IO.Directory.EnumerateFiles(Directory, "*.ivyml")
            .OrderByDescending(ParseIndex)
            .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
            .FirstOrDefault();
    }

    /// <summary>Loads the latest wireframe's path and content. Tolerant of in-progress writes.</summary>
    public static WireframeSnapshot LoadLatest()
    {
        var path = GetLatestFile();
        if (path is null)
            return WireframeSnapshot.Empty;

        try
        {
            // Share read/write so we don't fight the agent mid-write.
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            return new WireframeSnapshot(path, reader.ReadToEnd());
        }
        catch (IOException)
        {
            // File momentarily locked/partial; return path with no content so callers can retry.
            return new WireframeSnapshot(path, null);
        }
    }

    /// <summary>
    /// Overwrites an existing wireframe file in place. Used by the code editor's on-blur save; the
    /// directory watcher then refreshes the preview. (The agent still follows the append-only rule;
    /// this is an explicit manual edit of the current design.)
    /// </summary>
    public static void Save(string path, string content)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using var writer = new StreamWriter(fs);
        writer.Write(content);
    }

    private static int ParseIndex(string filePath)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
        return int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : -1;
    }
}
