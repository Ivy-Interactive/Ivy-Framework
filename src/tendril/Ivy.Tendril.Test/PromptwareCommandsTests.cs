using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PromptwareCommandsTests
{
    [Fact]
    public void Handle_ReturnsNegativeOne_ForEmptyArgs()
    {
        var result = PromptwareCommands.Handle(Array.Empty<string>());
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Handle_ReturnsNegativeOne_ForUnknownCommand()
    {
        var result = PromptwareCommands.Handle(new[] { "unknown-command" });
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Handle_MatchesUpdatePromptwaresCommand()
    {
        // When TENDRIL_HOME is not set, update-promptwares should return 1 (error)
        var originalHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        try
        {
            Environment.SetEnvironmentVariable("TENDRIL_HOME", null);
            var result = PromptwareCommands.Handle(new[] { "update-promptwares" });
            Assert.Equal(1, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TENDRIL_HOME", originalHome);
        }
    }

    [Fact]
    public void Handle_UpdatePromptwaresReturnsError_WhenNoEmbeddedResource()
    {
        // With TENDRIL_HOME set but no embedded resource in debug builds
        var originalHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-pw-cmd-test-{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(tempDir);
            Environment.SetEnvironmentVariable("TENDRIL_HOME", tempDir);
            var result = PromptwareCommands.Handle(new[] { "update-promptwares" });
            Assert.Equal(1, result); // No embedded resource in debug builds
        }
        finally
        {
            Environment.SetEnvironmentVariable("TENDRIL_HOME", originalHome);
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
