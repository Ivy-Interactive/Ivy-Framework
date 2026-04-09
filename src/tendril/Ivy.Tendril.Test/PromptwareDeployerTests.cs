using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PromptwareDeployerTests : IDisposable
{
    private readonly string _tempDir;

    public PromptwareDeployerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"promptware-deploy-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Best effort cleanup
        }
    }

    [Fact]
    public void IsEmbeddedAvailable_ReturnsFalse_InDebugBuilds()
    {
        // In debug/test builds, the embedded resource is not included
        var result = PromptwareDeployer.IsEmbeddedAvailable();
        Assert.False(result);
    }

    [Fact]
    public void Deploy_ThrowsWhenNoEmbeddedResource()
    {
        var targetDir = Path.Combine(_tempDir, "Promptwares");
        Assert.Throws<InvalidOperationException>(() => PromptwareDeployer.Deploy(targetDir));
    }
}
