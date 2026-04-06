using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class ServiceInterfaceTests : IDisposable
{
    private readonly string _tempDir;

    public ServiceInterfaceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void PlanReaderService_ImplementsIPlanReaderService()
    {
        var settings = new TendrilSettings();
        var config = new ConfigService(settings, _tempDir);
        var service = new PlanReaderService(config);

        Assert.IsAssignableFrom<IPlanReaderService>(service);
    }

    [Fact]
    public void JobService_ImplementsIJobService()
    {
        var service = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        Assert.IsAssignableFrom<IJobService>(service);
    }

    [Fact]
    public void PlanWatcherService_ImplementsIPlanWatcherService()
    {
        var settings = new TendrilSettings();
        var config = new ConfigService(settings, _tempDir);
        using var service = new PlanWatcherService(config);

        Assert.IsAssignableFrom<IPlanWatcherService>(service);
    }
}
