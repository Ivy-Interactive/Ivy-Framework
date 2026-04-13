using Ivy.Core.Exceptions;
using Ivy.Core.Hooks;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class PlanDownloadHelperTests
{
    private static (ViewContext, string, TrackingDownloadService) CreateTestEnvironment()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create test plan folder structures
        var planFolder = Path.Combine(tempDir, "00001-TestPlan");
        Directory.CreateDirectory(planFolder);
        Directory.CreateDirectory(Path.Combine(planFolder, "revisions"));
        File.WriteAllText(Path.Combine(planFolder, "revisions", "001.md"), "# Test Plan\n\nTest content");
        File.WriteAllText(Path.Combine(planFolder, "plan.yaml"),
            "state: Draft\nproject: Test\nlevel: Test\ntitle: Test Plan\nrepos: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\ninitialPrompt: test\nprs: []\ncommits: []\n");

        var planFolder2 = Path.Combine(tempDir, "00002-OtherPlan");
        Directory.CreateDirectory(planFolder2);
        Directory.CreateDirectory(Path.Combine(planFolder2, "revisions"));
        File.WriteAllText(Path.Combine(planFolder2, "revisions", "001.md"), "# Other Plan\n\nOther content");
        File.WriteAllText(Path.Combine(planFolder2, "plan.yaml"),
            "state: Draft\nproject: Test\nlevel: Test\ntitle: Other Plan\nrepos: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\ninitialPrompt: test\nprs: []\ncommits: []\n");

        File.WriteAllText(Path.Combine(tempDir, ".counter"), "3");

        var downloadService = new TrackingDownloadService();
        var services = new ServiceCollection();
        services.AddSingleton<IExceptionHandler>(new StubExceptionHandler());
        services.AddSingleton<IDownloadService>(downloadService);
        var testConfig = new TestConfigService(tempDir);
        services.AddSingleton<IConfigService>(testConfig);
        services.AddSingleton<ConfigService>(testConfig);
        services.AddSingleton<ILogger<PlanReaderService>>(NullLogger<PlanReaderService>.Instance);
        services.AddSingleton<PlanReaderService>();
        var provider = services.BuildServiceProvider();
        var context = new ViewContext(() => { }, null, provider);

        return (context, tempDir, downloadService);
    }

    [Fact]
    public async Task UsePlanDownload_ShouldUpdateUrl_WhenPlanChangesFromNullToNonNull()
    {
        var (ctx, tempDir, downloadService) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();

            PlanDownloadHelper.UsePlanDownload(ctx, planService, null);

            // Allow effects to process
            await Task.Delay(100);

            ctx.Reset();
            var metadata = new PlanMetadata(1, "Test", "Test", "Test Plan", PlanStatus.Draft, [], [], [], [], [], [],
                DateTime.UtcNow, DateTime.UtcNow, null, null);
            var testPlan = new PlanFile(metadata, "", Path.Combine(tempDir, "00001-TestPlan"), "");

            PlanDownloadHelper.UsePlanDownload(ctx, planService, testPlan);

            // Allow effects to process
            await Task.Delay(100);

            // The download should have been re-registered with the plan's filename
            Assert.Contains(downloadService.RegisteredFileNames, f => f == "00001-TestPlan.pdf");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task UsePlanDownload_SwitchingPlans_UpdatesFilenameAndContent()
    {
        var (ctx, tempDir, downloadService) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();

            // First render with plan 1
            var metadata1 = new PlanMetadata(1, "Test", "Test", "Test Plan", PlanStatus.Draft, [], [], [], [], [], [],
                DateTime.UtcNow, DateTime.UtcNow, null, null);
            var plan1 = new PlanFile(metadata1, "", Path.Combine(tempDir, "00001-TestPlan"), "");
            PlanDownloadHelper.UsePlanDownload(ctx, planService, plan1);

            await Task.Delay(100);
            Assert.Contains(downloadService.RegisteredFileNames, f => f == "00001-TestPlan.pdf");

            // Second render with plan 2
            ctx.Reset();
            var metadata2 = new PlanMetadata(2, "Test", "Test", "Other Plan", PlanStatus.Draft, [], [], [], [], [], [],
                DateTime.UtcNow, DateTime.UtcNow, null, null);
            var plan2 = new PlanFile(metadata2, "", Path.Combine(tempDir, "00002-OtherPlan"), "");
            PlanDownloadHelper.UsePlanDownload(ctx, planService, plan2);

            await Task.Delay(100);
            Assert.Contains(downloadService.RegisteredFileNames, f => f == "00002-OtherPlan.pdf");

            // Verify the factory produces non-empty bytes for the second plan
            var lastFactory = downloadService.LastFactory;
            Assert.NotNull(lastFactory);
            var bytes = await lastFactory!();
            Assert.NotEmpty(bytes);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void UsePlanDownload_WithNullPlan_ReturnsState()
    {
        var (ctx, tempDir, _) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();

            var result = PlanDownloadHelper.UsePlanDownload(ctx, planService, null);

            Assert.NotNull(result);
            Assert.Null(result.Value);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void UsePlanDownload_WithValidPlan_ReturnsState()
    {
        var (ctx, tempDir, _) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();
            var metadata = new PlanMetadata(1, "Test", "Test", "Test Plan", PlanStatus.Draft, [], [], [], [], [], [],
                DateTime.UtcNow, DateTime.UtcNow, null, null);
            var testPlan = new PlanFile(metadata, "", Path.Combine(tempDir, "00001-TestPlan"), "");

            var result = PlanDownloadHelper.UsePlanDownload(ctx, planService, testPlan);

            Assert.NotNull(result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private class StubExceptionHandler : IExceptionHandler
    {
        public bool HandleException(Exception exception)
        {
            return false;
        }
    }

    internal class TrackingDownloadService : IDownloadService
    {
        public List<string> RegisteredFileNames { get; } = [];
        public Func<Task<byte[]>>? LastFactory { get; private set; }

        public (IDisposable cleanup, string url) AddDownload(Func<Task<byte[]>> factory, string mimeType,
            string fileName)
        {
            RegisteredFileNames.Add(fileName);
            LastFactory = factory;
            return (new StubDisposable(), $"blob:stub-url-{fileName}");
        }

        public (IDisposable cleanup, string url) AddStreamDownload(Func<Task<Stream>> factory, string mimeType,
            string fileName)
        {
            return (new StubDisposable(), "blob:stub-url");
        }

        public Task<IActionResult> Download(string downloadId)
        {
            throw new NotImplementedException();
        }

        private class StubDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    private class TestConfigService : ConfigService, IConfigService
    {
        public TestConfigService(string planFolder)
        {
            PlanFolder = planFolder;
        }

        public new string PlanFolder { get; }

        string IConfigService.PlanFolder => PlanFolder;
    }
}