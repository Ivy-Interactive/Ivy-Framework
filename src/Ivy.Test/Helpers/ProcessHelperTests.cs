using Ivy;

namespace Ivy.Test.Helpers;

public class ProcessHelperTests
{
    [Fact]
    public void IsProduction_WhenNotSet_ReturnsFalse()
    {
        var original = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", null);
            Assert.False(ProcessHelper.IsProduction());
        }
        finally
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void IsProduction_WhenSetToProduction_ReturnsTrue()
    {
        var original = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", "Production");
            Assert.True(ProcessHelper.IsProduction());
        }
        finally
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void IsDevelopment_WhenNotSet_ReturnsTrue()
    {
        var original = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", null);
            Assert.True(ProcessHelper.IsDevelopment());
        }
        finally
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void IsDevelopment_WhenSetToDevelopment_ReturnsTrue()
    {
        var original = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", "Development");
            Assert.True(ProcessHelper.IsDevelopment());
        }
        finally
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void IsDevelopment_WhenSetToProduction_ReturnsFalse()
    {
        var original = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", "Production");
            Assert.False(ProcessHelper.IsDevelopment());
        }
        finally
        {
            Environment.SetEnvironmentVariable("IVY_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void IsPortInUse_UnusedPort_ReturnsFalse()
    {
        // Port 0 is never in use (it's the OS auto-assign port)
        Assert.False(ProcessHelper.IsPortInUse(0));
    }

    [Fact]
    public void KillProcessUsingPort_NoProcessOnPort_DoesNotThrow()
    {
        // Use a random high port that is very unlikely to be in use
        var exception = Record.Exception(() => ProcessHelper.KillProcessUsingPort(39_517));
        Assert.Null(exception);
    }

    [Fact]
    public void OpenBrowser_NullUrl_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ProcessHelper.OpenBrowser(null!));
    }

    [Fact]
    public void OpenBrowser_EmptyUrl_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ProcessHelper.OpenBrowser(""));
    }
}
