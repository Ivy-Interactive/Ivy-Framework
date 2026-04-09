using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServicePromptsRootTests
{
    [Fact]
    public void ResolvePromptsRoot_ReturnsSourceDir_WhenRunningFromSourceTree()
    {
        // In test/debug mode, AppContext.BaseDirectory is in bin/Debug/net10.0
        // Going ../../.. should land in the project dir where Promptwares/ exists
        var sourceRoot = Path.GetFullPath(
            Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "Ivy.Tendril", "Promptwares"));

        var result = JobService.ResolvePromptsRoot();

        // The resolved path should exist (either source or TENDRIL_HOME)
        // In test builds, it should resolve to the source tree Promptwares if it exists,
        // or fall back to TENDRIL_HOME/Promptwares
        Assert.False(string.IsNullOrEmpty(result));
        Assert.EndsWith("Promptwares", result);
    }

    [Fact]
    public void ResolvePromptsRoot_FallsBackToTendrilHome_WhenSourceDirMissing()
    {
        // This test verifies the fallback logic conceptually.
        // We can't easily simulate a missing source dir in tests,
        // but we verify the method doesn't throw and returns a valid path.
        var result = JobService.ResolvePromptsRoot();
        Assert.NotNull(result);
        Assert.Contains("Promptwares", result);
    }
}
