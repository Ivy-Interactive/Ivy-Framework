using Ivy;
using Ivy.Core;
using Ivy.Core.Exceptions;
using Ivy.Core.Hooks;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Tendril.Test;

public class PlanDownloadHelperTests
{
    private static (ViewContext, string) CreateTestEnvironment()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create a test plan folder structure
        var planFolder = Path.Combine(tempDir, "00001-TestPlan");
        Directory.CreateDirectory(planFolder);
        Directory.CreateDirectory(Path.Combine(planFolder, "revisions"));
        File.WriteAllText(Path.Combine(planFolder, "revisions", "001.md"), "# Test Plan\n\nTest content");
        File.WriteAllText(Path.Combine(planFolder, "plan.yaml"), "state: Draft\nproject: Test\nlevel: Test\ntitle: Test Plan\nrepos: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\ninitialPrompt: test\nprs: []\ncommits: []\n");
        File.WriteAllText(Path.Combine(tempDir, ".counter"), "2");

        var services = new ServiceCollection();
        services.AddSingleton<IExceptionHandler>(new StubExceptionHandler());
        services.AddSingleton<IDownloadService>(new StubDownloadService());
        services.AddSingleton<ConfigService>(new TestConfigService(tempDir));
        services.AddSingleton<PlanReaderService>();
        var provider = services.BuildServiceProvider();
        var context = new ViewContext(() => { }, null, provider);

        return (context, tempDir);
    }

    [Fact]
    public void UsePlanDownload_ShouldNotThrow_WhenPlanChangesFromNullToNonNull()
    {
        var (ctx, tempDir) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();

            var result1 = PlanDownloadHelper.UsePlanDownload(ctx, planService, null);
            Assert.NotNull(result1);

            ctx.Reset();
            var metadata = new PlanMetadata(1, "Test", "Test", "Test Plan", PlanStatus.Draft, [], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow);
            var testPlan = new PlanFile(metadata, "", Path.Combine(tempDir, "00001-TestPlan"), "");

            var result2 = PlanDownloadHelper.UsePlanDownload(ctx, planService, testPlan);
            Assert.NotNull(result2);
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
        var (ctx, tempDir) = CreateTestEnvironment();
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
        var (ctx, tempDir) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();
            var metadata = new PlanMetadata(1, "Test", "Test", "Test Plan", PlanStatus.Draft, [], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow);
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
        public bool HandleException(Exception exception) => false;
    }

    private class StubDownloadService : IDownloadService
    {
        public (IDisposable cleanup, string url) AddDownload(Func<Task<byte[]>> factory, string mimeType, string fileName)
        {
            return (new StubDisposable(), "blob:stub-url");
        }

        public (IDisposable cleanup, string url) AddStreamDownload(Func<Task<Stream>> factory, string mimeType, string fileName)
        {
            return (new StubDisposable(), "blob:stub-url");
        }

        public Task<IActionResult> Download(string downloadId)
        {
            throw new NotImplementedException();
        }

        private class StubDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    private class TestConfigService : ConfigService
    {
        private readonly string _planFolder;

        public TestConfigService(string planFolder)
        {
            _planFolder = planFolder;
        }

        public new string PlanFolder => _planFolder;
    }
}
