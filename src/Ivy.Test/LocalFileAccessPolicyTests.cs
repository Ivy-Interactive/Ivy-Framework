namespace Ivy.Test;

public class LocalFileAccessPolicyTests : IDisposable
{
    private readonly string _baseDirectory;
    private readonly string _root;
    private readonly string _outside;

    public LocalFileAccessPolicyTests()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _root = Path.Combine(_baseDirectory, "root");
        _outside = Path.Combine(_baseDirectory, "outside");
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_outside);
    }

    public void Dispose()
    {
        if (Directory.Exists(_baseDirectory))
        {
            Directory.Delete(_baseDirectory, true);
        }
    }

    #region Roots

    [Fact]
    public void TryResolve_WithNoRoots_AcceptsAnyPath()
    {
        var path = CreateFile(_outside, "secret.txt");

        Assert.True(LocalFileAccessPolicy.TryResolve(path, [], [], out var fullPath));
        Assert.Equal(path, fullPath);
    }

    [Fact]
    public void TryResolve_InsideRoot_IsAccepted()
    {
        var path = CreateFile(_root, "photo.png");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        Assert.True(LocalFileAccessPolicy.TryResolve(path, roots, [], out var fullPath));
        Assert.Equal(path, fullPath);
    }

    [Fact]
    public void TryResolve_InNestedDirectoryUnderRoot_IsAccepted()
    {
        var nested = Directory.CreateDirectory(Path.Combine(_root, "a", "b")).FullName;
        var path = CreateFile(nested, "deep.png");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        Assert.True(LocalFileAccessPolicy.TryResolve(path, roots, [], out _));
    }

    [Fact]
    public void TryResolve_OutsideRoot_IsRejected()
    {
        var path = CreateFile(_outside, "secret.txt");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        Assert.False(LocalFileAccessPolicy.TryResolve(path, roots, [], out _));
    }

    [Fact]
    public void TryResolve_TraversalOutOfRoot_IsRejectedAfterNormalization()
    {
        CreateFile(_outside, "secret.txt");
        var traversal = Path.Combine(_root, "..", "outside", "secret.txt");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        Assert.False(LocalFileAccessPolicy.TryResolve(traversal, roots, [], out _));
    }

    [Fact]
    public void TryResolve_SiblingDirectoryWithRootAsPrefix_IsRejected()
    {
        var siblingPrefix = Directory.CreateDirectory(_root + "-secrets").FullName;
        var path = CreateFile(siblingPrefix, "key.pem");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        Assert.False(LocalFileAccessPolicy.TryResolve(path, roots, [], out _));
    }

    [Fact]
    public void TryResolve_WithMultipleRoots_AcceptsAnyOfThem()
    {
        var second = Directory.CreateDirectory(Path.Combine(_baseDirectory, "second")).FullName;
        var path = CreateFile(second, "photo.png");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root, second]);

        Assert.True(LocalFileAccessPolicy.TryResolve(path, roots, [], out _));
    }

    [Fact]
    public void TryResolve_RootConfiguredWithTrailingSeparator_StillConfines()
    {
        var inside = CreateFile(_root, "photo.png");
        var outside = CreateFile(_outside, "secret.txt");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root + Path.DirectorySeparatorChar]);

        Assert.True(LocalFileAccessPolicy.TryResolve(inside, roots, [], out _));
        Assert.False(LocalFileAccessPolicy.TryResolve(outside, roots, [], out _));
    }

    [Fact]
    public void TryResolve_LeafSymlinkPointingOutsideRoot_IsRejected()
    {
        var target = CreateFile(_outside, "secret.txt");
        var link = Path.Combine(_root, "link.txt");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        // The non-symlink half of the assertion always runs, so the test is never vacuous even on a
        // platform (or account) where creating a symlink is not permitted.
        Assert.True(LocalFileAccessPolicy.TryResolve(CreateFile(_root, "photo.png"), roots, [], out _));

        try
        {
            File.CreateSymbolicLink(link, target);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        {
            return; // Symlink creation not permitted here; skip the escape half.
        }

        Assert.False(LocalFileAccessPolicy.TryResolve(link, roots, [], out _));
    }

    [Fact]
    public void TryResolve_MidPathDirectoryLinkPointingOutsideRoot_IsRejected()
    {
        var target = CreateFile(_outside, "id_rsa");
        var link = Path.Combine(_root, "link");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        // The non-link half of the assertion always runs, so the test is never vacuous.
        Assert.True(LocalFileAccessPolicy.TryResolve(CreateFile(_root, "photo.png"), roots, [], out _));

        if (!TryCreateDirectoryLink(link, _outside))
            return; // Directory link creation not permitted here; skip the escape half.

        var pathThroughLink = Path.Combine(link, "id_rsa");
        Assert.True(File.Exists(pathThroughLink), "Link should allow access to target file");
        Assert.False(LocalFileAccessPolicy.TryResolve(pathThroughLink, roots, [], out _));
    }

    [Fact]
    public void TryResolve_RootReachedThroughADirectoryLink_StillConfines()
    {
        var real = Directory.CreateDirectory(Path.Combine(_baseDirectory, "real")).FullName;
        var inside = CreateFile(real, "inside.png");
        var outside = CreateFile(_outside, "secret.txt");
        var link = Path.Combine(_baseDirectory, "link");

        if (!TryCreateDirectoryLink(link, real))
        {
            // No directory link support; assert the real path works.
            var roots = LocalFileAccessPolicy.NormalizeRoots([real]);
            Assert.True(LocalFileAccessPolicy.TryResolve(inside, roots, [], out _));
            Assert.False(LocalFileAccessPolicy.TryResolve(outside, roots, [], out _));
            return;
        }

        // Configure the root as the link path.
        var roots2 = LocalFileAccessPolicy.NormalizeRoots([link]);

        // File inside root is accepted by both its link path and its real path.
        Assert.True(LocalFileAccessPolicy.TryResolve(Path.Combine(link, "inside.png"), roots2, [], out _));
        Assert.True(LocalFileAccessPolicy.TryResolve(inside, roots2, [], out _));

        // File outside is still rejected.
        Assert.False(LocalFileAccessPolicy.TryResolve(outside, roots2, [], out _));
    }

    [Fact]
    public void TryResolve_MidPathLinkChainPointingOutsideRoot_IsRejected()
    {
        var intermediate = Directory.CreateDirectory(Path.Combine(_baseDirectory, "intermediate")).FullName;
        var target = CreateFile(_outside, "secret.txt");
        var link1 = Path.Combine(_root, "link1");
        var link2 = Path.Combine(intermediate, "link2");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        if (!TryCreateDirectoryLink(link1, intermediate) || !TryCreateDirectoryLink(link2, _outside))
            return; // Directory link creation not permitted here.

        var pathThroughChain = Path.Combine(link1, "link2", "secret.txt");
        Assert.True(File.Exists(pathThroughChain), "Link chain should allow access to target file");
        Assert.False(LocalFileAccessPolicy.TryResolve(pathThroughChain, roots, [], out _));
    }

    [Fact]
    public void TryResolve_MidPathDirectoryLinkStayingInsideRoot_IsAccepted()
    {
        var subdir = Directory.CreateDirectory(Path.Combine(_root, "subdir")).FullName;
        var target = CreateFile(subdir, "photo.png");
        var link = Path.Combine(_root, "link");
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        if (!TryCreateDirectoryLink(link, subdir))
        {
            // No directory link support; assert the real path works.
            Assert.True(LocalFileAccessPolicy.TryResolve(target, roots, [], out _));
            return;
        }

        var pathThroughLink = Path.Combine(link, "photo.png");
        Assert.True(File.Exists(pathThroughLink), "Link should allow access to target file");
        Assert.True(LocalFileAccessPolicy.TryResolve(pathThroughLink, roots, [], out _));
    }

    #endregion

    #region Extensions

    [Theory]
    [InlineData("photo.png", true)]
    [InlineData("photo.PNG", true)]
    [InlineData("photo.jpg", true)]
    [InlineData("config.yml", false)]
    [InlineData("key.pem", false)]
    [InlineData("noextension", false)]
    public void TryResolve_WithExtensionAllowlist_AcceptsOnlyListedExtensions(string fileName, bool expected)
    {
        var path = CreateFile(_root, fileName);
        var extensions = LocalFileAccessPolicy.NormalizeExtensions([".png", "jpg"]);

        Assert.Equal(expected, LocalFileAccessPolicy.TryResolve(path, [], extensions, out _));
    }

    [Fact]
    public void TryResolve_WithNoExtensionAllowlist_AcceptsAnyExtension()
    {
        Assert.True(LocalFileAccessPolicy.TryResolve(CreateFile(_root, "key.pem"), [], [], out _));
        Assert.True(LocalFileAccessPolicy.TryResolve(CreateFile(_root, "noextension"), [], [], out _));
    }

    [Fact]
    public void TryResolve_AppliesRootsAndExtensionsTogether()
    {
        var extensions = LocalFileAccessPolicy.NormalizeExtensions([".png"]);
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);

        Assert.True(LocalFileAccessPolicy.TryResolve(CreateFile(_root, "in.png"), roots, extensions, out _));
        Assert.False(LocalFileAccessPolicy.TryResolve(CreateFile(_root, "in.yml"), roots, extensions, out _));
        Assert.False(LocalFileAccessPolicy.TryResolve(CreateFile(_outside, "out.png"), roots, extensions, out _));
    }

    #endregion

    #region Bad input

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryResolve_WithBlankPath_IsRejected(string? path)
    {
        Assert.False(LocalFileAccessPolicy.TryResolve(path, [], [], out var fullPath));
        Assert.Equal(string.Empty, fullPath);
    }

    [Fact]
    public void TryResolve_WithNonExistentPathInsideRoot_StillReportsTheResolvedPath()
    {
        // Existence is the caller's check; the policy only answers "may this path be served".
        var roots = LocalFileAccessPolicy.NormalizeRoots([_root]);
        var missing = Path.Combine(_root, "missing.png");

        Assert.True(LocalFileAccessPolicy.TryResolve(missing, roots, [], out var fullPath));
        Assert.Equal(missing, fullPath);
    }

    #endregion

    #region Normalization

    [Fact]
    public void NormalizeRoots_DropsBlanksTrailingSeparatorsAndDuplicates()
    {
        var roots = LocalFileAccessPolicy.NormalizeRoots(
        [
            _root,
            _root + Path.DirectorySeparatorChar,
            "  ",
            ""
        ]);

        // Assert the contract rather than the exact string, since on macOS the temp path itself may
        // contain a symlink (e.g. /var -> /private/var).
        Assert.Single(roots);
        Assert.False(roots[0].EndsWith(Path.DirectorySeparatorChar));
        Assert.False(roots[0].EndsWith(Path.AltDirectorySeparatorChar));

        // A file created under the original root path should resolve as inside it.
        var file = CreateFile(_root, "test.txt");
        Assert.True(LocalFileAccessPolicy.TryResolve(file, roots, [], out _));
    }

    [Fact]
    public void NormalizeRoots_WithNull_ReturnsEmpty()
    {
        Assert.Empty(LocalFileAccessPolicy.NormalizeRoots(null));
    }

    [Theory]
    [InlineData(".png", ".png")]
    [InlineData("png", ".png")]
    [InlineData(".PNG", ".png")]
    [InlineData("  JPG  ", ".jpg")]
    public void NormalizeExtensions_AddsTheDotAndLowerCases(string input, string expected)
    {
        Assert.Equal(new[] { expected }, LocalFileAccessPolicy.NormalizeExtensions([input]));
    }

    [Fact]
    public void NormalizeExtensions_DropsBlanksBareDotsAndDuplicates()
    {
        Assert.Equal(new[] { ".png" }, LocalFileAccessPolicy.NormalizeExtensions([".png", "PNG", ".", "", "  "]));
    }

    [Fact]
    public void NormalizeExtensions_WithNull_ReturnsEmpty()
    {
        Assert.Empty(LocalFileAccessPolicy.NormalizeExtensions(null));
    }

    #endregion

    private static string CreateFile(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, "content");
        return path;
    }

    /// <summary>
    /// Creates a directory link, preferring a symlink and falling back to a Windows junction, which needs
    /// no SeCreateSymbolicLinkPrivilege. Returns false when neither is available, so a caller can skip the
    /// link half of its assertion rather than fail.
    /// </summary>
    private static bool TryCreateDirectoryLink(string link, string target)
    {
        try
        {
            Directory.CreateSymbolicLink(link, target);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        {
            // Symlink creation not permitted; try a Windows junction.
        }

        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mklink /J \"{link}\" \"{target}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(5000);

            return Directory.Exists(link);
        }
        catch
        {
            return false;
        }
    }
}
