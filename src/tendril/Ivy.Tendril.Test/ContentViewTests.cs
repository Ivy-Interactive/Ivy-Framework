using Ivy.Tendril.Apps.Plans;
using ReviewContentView = Ivy.Tendril.Apps.Review.ContentView;

namespace Ivy.Tendril.Test;

public class ContentViewTests
{
    private static PlanFile CreateFailedPlan(string folderPath)
    {
        var metadata = new PlanMetadata(
            1, "Test", "Bug", "Test Plan", PlanStatus.Failed,
            [], [], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null);
        return new PlanFile(metadata, "", folderPath, "");
    }

    [Fact]
    public void BuildFailureCallout_WithCompletedStatusLog_ShowsStateMismatch()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var logsDir = Path.Combine(planDir, "logs");
            Directory.CreateDirectory(logsDir);

            File.WriteAllText(Path.Combine(logsDir, "001-ExecutePlan.md"),
                "# ExecutePlan\n\n- **Status:** Completed\n- **Started:** 2026-03-30 10:00:00Z\n- **Completed:** 2026-03-30 10:05:00Z\n- **Duration:** 300s\n");

            var plan = CreateFailedPlan(planDir);
            var result = ContentView.BuildFailureCallout(plan);

            var callout = Assert.IsType<Callout>(result);
            Assert.Equal(CalloutVariant.Warning, callout.Variant);
            Assert.Equal("State Mismatch", callout.Title);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BuildFailureCallout_WithFailedStatusLog_ShowsDestructiveCallout()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var logsDir = Path.Combine(planDir, "logs");
            Directory.CreateDirectory(logsDir);

            File.WriteAllText(Path.Combine(logsDir, "001-ExecutePlan.md"),
                "# ExecutePlan\n\n- **Status:** Failed\n- **Started:** 2026-03-30 10:00:00Z\n");

            var plan = CreateFailedPlan(planDir);
            var result = ContentView.BuildFailureCallout(plan);

            var callout = Assert.IsType<Callout>(result);
            Assert.Equal(CalloutVariant.Destructive, callout.Variant);
            Assert.Equal("Execution Failed", callout.Title);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BuildFailureCallout_WithNoLogs_ShowsNoDetailsAvailable()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            Directory.CreateDirectory(planDir);

            var plan = CreateFailedPlan(planDir);
            var result = ContentView.BuildFailureCallout(plan);

            var callout = Assert.IsType<Callout>(result);
            Assert.Equal(CalloutVariant.Destructive, callout.Variant);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BuildFailureCallout_WithSummaryLog_ShowsSummary()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var logsDir = Path.Combine(planDir, "logs");
            Directory.CreateDirectory(logsDir);

            File.WriteAllText(Path.Combine(logsDir, "001-ExecutePlan.md"),
                "# ExecutePlan\n\n## Summary\n\nBuild failed due to missing dependency.\n");

            var plan = CreateFailedPlan(planDir);
            var result = ContentView.BuildFailureCallout(plan);

            var callout = Assert.IsType<Callout>(result);
            Assert.Equal(CalloutVariant.Destructive, callout.Variant);
            Assert.Equal("Execution Failed", callout.Title);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ValidateArtifactPath_WithValidPath_ReturnsTrue()
    {
        Assert.True(ReviewContentView.ValidateArtifactPath(
            "D:/plans/001/artifacts/screenshots/img.png", "D:/plans/001"));
    }

    [Fact]
    public void ValidateArtifactPath_WithTraversalPath_ReturnsFalse()
    {
        Assert.False(ReviewContentView.ValidateArtifactPath(
            "D:/plans/001/artifacts/../plan.yaml", "D:/plans/001"));
    }

    [Fact]
    public void ValidateArtifactPath_WithExternalPath_ReturnsFalse()
    {
        Assert.False(ReviewContentView.ValidateArtifactPath(
            "C:/Windows/System32/config", "D:/plans/001"));
    }
}