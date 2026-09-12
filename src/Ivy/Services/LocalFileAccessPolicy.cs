// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Decides whether <c>GET /ivy/local-file</c> may serve a path, given the roots and extensions
/// configured on the server. An empty root or extension list means "no restriction", which is what
/// the no-argument <see cref="Server.DangerouslyAllowLocalFiles()"/> overload leaves behind.
/// </summary>
internal static class LocalFileAccessPolicy
{
    /// <summary>
    /// Normalizes configured roots to full paths without a trailing separator. The whole root path is
    /// canonicalized at configure time, so a root reached through a directory link still matches request
    /// paths canonicalized the same way.
    /// </summary>
    internal static string[] NormalizeRoots(IEnumerable<string>? roots)
    {
        if (roots == null)
            return [];

        return roots
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(root => Path.TrimEndingDirectorySeparator(ResolveFullPath(Path.GetFullPath(root.Trim()))))
            .Where(root => root.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Normalizes configured extensions to lower-case with a leading dot, accepting input with or
    /// without the dot.
    /// </summary>
    internal static string[] NormalizeExtensions(IEnumerable<string>? extensions)
    {
        if (extensions == null)
            return [];

        return extensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.Trim().ToLowerInvariant())
            .Select(extension => extension.StartsWith('.') ? extension : "." + extension)
            .Where(extension => extension.Length > 1)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Resolves <paramref name="path"/> and reports whether the policy allows serving it. Returns
    /// false rather than throwing for any unusable path, so the caller can answer 404 uniformly and
    /// never turn the endpoint into an existence oracle.
    /// </summary>
    internal static bool TryResolve(
        string? path,
        IReadOnlyList<string> roots,
        IReadOnlyList<string> extensions,
        out string fullPath)
    {
        fullPath = string.Empty;

        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception e) when (e is ArgumentException or NotSupportedException or PathTooLongException or IOException)
        {
            fullPath = string.Empty;
            return false;
        }

        if (extensions.Count > 0)
        {
            var extension = Path.GetExtension(fullPath);
            if (string.IsNullOrEmpty(extension) || !extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        if (roots.Count == 0)
            return true;

        // A link inside a root that points outside it is deliberately rejected, wherever in the path it sits.
        var resolved = ResolveFullPath(fullPath);

        // Check extension allowlist against resolved path too, so a symlink cannot rename the extension.
        if (extensions.Count > 0)
        {
            var resolvedExtension = Path.GetExtension(resolved);
            if (string.IsNullOrEmpty(resolvedExtension) || !extensions.Contains(resolvedExtension, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        return roots.Any(root => IsWithin(resolved, root));
    }

    private static bool IsWithin(string fullPath, string root)
    {
        if (string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
            return true;

        // Requiring the separator is what stops root "/data/pub" from matching "/data/pub-secrets".
        return fullPath.Length > root.Length
            && fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            && (fullPath[root.Length] == Path.DirectorySeparatorChar
                || fullPath[root.Length] == Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Resolves every link in <paramref name="fullPath"/>, not only its final component, by walking the
    /// path from its root and following any segment that turns out to be a symlink or a junction. A
    /// directory link in the middle of the path is what a leaf-only resolver misses.
    /// </summary>
    private static string ResolveFullPath(string fullPath)
    {
        var pathRoot = Path.GetPathRoot(fullPath);
        if (string.IsNullOrEmpty(pathRoot))
            return fullPath;

        var current = pathRoot;

        foreach (var segment in fullPath[pathRoot.Length..].Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Join(current, segment);
            current = ResolveLink(candidate) ?? candidate;
        }

        return current;
    }

    private static string? ResolveLink(string candidate)
    {
        try
        {
            // FileInfo resolves a directory link too, so one call covers both segment kinds, and
            // returnFinalTarget follows a chain of links in a single step.
            var target = new FileInfo(candidate).ResolveLinkTarget(returnFinalTarget: true);
            if (target == null)
                return null;

            // A relative link target belongs to the link's own directory, never to the process working
            // directory. GetFullPath ignores the base when the target is already rooted.
            return Path.GetFullPath(target.FullName, Path.GetDirectoryName(candidate) ?? candidate);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // The segment does not exist, or is an unresolvable link such as a cycle. Treat it as a
            // literal name: a path we cannot resolve cannot be opened either, so the caller's
            // File.Exists check rejects it for the same reason.
            return null;
        }
    }
}
