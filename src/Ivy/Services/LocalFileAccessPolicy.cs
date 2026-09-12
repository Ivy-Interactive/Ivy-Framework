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
    /// Normalizes configured roots to full paths without a trailing separator. Each root's own leaf
    /// link is resolved here, at configure time, so a symlinked root directory cannot cause a false
    /// mismatch against an already-resolved request path.
    /// </summary>
    internal static string[] NormalizeRoots(IEnumerable<string>? roots)
    {
        if (roots == null)
            return [];

        return roots
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(root => Path.TrimEndingDirectorySeparator(ResolveLeafLink(Path.GetFullPath(root.Trim()))))
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

        // A symlink inside a root that points outside it is deliberately rejected.
        var resolved = ResolveLeafLink(fullPath);
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
    /// The startup warning for an enabled but unconfined endpoint, or null when the configuration
    /// needs no warning.
    /// </summary>
    internal static string? DescribeUnconfinedAccess(ServerArgs args)
    {
        if (!args.DangerouslyAllowLocalFiles || args.LocalFileRoots.Length > 0)
            return null;

        return "[WARNING] DangerouslyAllowLocalFiles() with no roots serves any readable file on this machine over\n" +
               "          the unauthenticated GET /ivy/local-file endpoint. Pass roots to confine it, e.g.\n" +
               "          DangerouslyAllowLocalFiles(\"C:/Users/me/Photos\"), or call AllowLocalFileExtensions(...)\n" +
               "          to narrow it further when the roots are not known ahead of time.";
    }

    private static string ResolveLeafLink(string fullPath)
    {
        try
        {
            return new FileInfo(fullPath).ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? fullPath;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Nothing there to resolve (or nothing we may read); fall back to the literal path,
            // which the caller's File.Exists check rejects.
            return fullPath;
        }
    }
}
