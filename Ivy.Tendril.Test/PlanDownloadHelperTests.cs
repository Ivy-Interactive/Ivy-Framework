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
        // Create a temporary directory for test plans
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create a test plan file
        var testPlanPath = Path.Combine(tempDir, "test.md");
        File.WriteAllText(testPlanPath, "# Test Plan\n\nTest content");

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
        // This test verifies that UsePlanDownload maintains consistent hook ordering
        // when transitioning from null plan to non-null plan.
        // Before the fix, conditional hooks caused InvalidOperationException: State type mismatch.

        var (ctx, tempDir) = CreateTestEnvironment();
        try
        {
            var planService = ctx.UseService<PlanReaderService>();

            // First render: call with null plan
            var result1 = PlanDownloadHelper.UsePlanDownload(ctx, planService, null);
            Assert.NotNull(result1);

            // Simulate re-render with state change (null -> non-null plan)
            ctx.Reset();
            var metadata = new PlanMetadata(1, "Test", "Test", "Test Plan");
            var testPlan = new PlanFile(metadata, "", "", "test.md", PlanStatus.Draft);

            // Second render: call with non-null plan
            // This should NOT throw InvalidOperationException
            var result2 = PlanDownloadHelper.UsePlanDownload(ctx, planService, testPlan);
            Assert.NotNull(result2);
        }
        finally
        {
            // Clean up temp directory
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
            var metadata = new PlanMetadata(1, "Test", "Test", "Test Plan");
            var testPlan = new PlanFile(metadata, "", "", "test.md", PlanStatus.Draft);

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
