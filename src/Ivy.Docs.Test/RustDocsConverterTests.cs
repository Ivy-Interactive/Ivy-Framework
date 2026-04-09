using System.Diagnostics;
using System.Reflection;

namespace Ivy.Docs.Test;

public class RustDocsConverterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _inputDir;
    private readonly string _outputDir;

    public RustDocsConverterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ivy_docs_test_" + Guid.NewGuid().ToString("N"));
        _inputDir = Path.Combine(_tempDir, "input");
        _outputDir = Path.Combine(_tempDir, "output");

        Directory.CreateDirectory(_inputDir);
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string GetCliManifestPath()
    {
        var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;
        // buildDir is something like .../Ivy.Docs.Test/bin/Debug/net10.0
        // We need to resolve .../Ivy.Docs.Tools/rust_cli/Cargo.toml
        return Path.GetFullPath(Path.Combine(buildDir, "../../../../Ivy.Docs.Tools/rust_cli/Cargo.toml"));
    }

    [Fact]
    public void Convert_DeletesStaleGeneratedFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_inputDir, "DummyProj.csproj"), "<RootNamespace>TestApp</RootNamespace>");

        var mdPath1 = Path.Combine(_inputDir, "Page1.md");
        File.WriteAllText(mdPath1, "# Page 1");

        var staleGs = Path.Combine(_outputDir, "StalePage.g.cs");
        var staleMd = Path.Combine(_outputDir, "StalePage.md");
        File.WriteAllText(staleGs, "// Stale Code");
        File.WriteAllText(staleMd, "# Stale Content");

        // Act - First generation
        RunCargoConvert();

        // Assert - Old files gone, new files generated
        Assert.False(File.Exists(staleGs), "Stale .g.cs file should have been pruned.");
        Assert.False(File.Exists(staleMd), "Stale .md file should have been pruned.");

        var newGs = Path.Combine(_outputDir, "Page1.g.cs");
        var newMd = Path.Combine(_outputDir, "Page1.md");
        Assert.True(File.Exists(newGs), "New .g.cs file should have been created.");
        Assert.True(File.Exists(newMd), "New .md file should have been created.");

        // Act - Second generation
        File.Delete(mdPath1);
        RunCargoConvert();

        // Assert - Page1 files should be pruned because the .md file is gone
        Assert.False(File.Exists(newGs), "Page1.g.cs should have been pruned after source md removed.");
        Assert.False(File.Exists(newMd), "Page1.md should have been pruned after source md removed.");
    }

    private void RunCargoConvert()
    {
        var manifestPath = GetCliManifestPath();
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"Cannot locate rust cli manifest at {manifestPath}");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "cargo",
            ArgumentList = { "run", "--release", "--manifest-path", manifestPath, "--", "convert", _inputDir, _outputDir },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        Assert.NotNull(process);

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Cargo convert failed: {stderr}\n{stdout}");
        }
    }
}
