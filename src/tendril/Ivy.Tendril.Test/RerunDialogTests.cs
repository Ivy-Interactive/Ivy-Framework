using Ivy.Tendril.Apps.Review.Dialogs;

namespace Ivy.Tendril.Test;

public class RerunDialogTests
{
    [Fact]
    public void CleanPlanState_DeletesArtifactsAndLogs()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var artifactsDir = Path.Combine(planDir, "artifacts");
            var logsDir = Path.Combine(planDir, "logs");
            Directory.CreateDirectory(artifactsDir);
            Directory.CreateDirectory(logsDir);
            File.WriteAllText(Path.Combine(artifactsDir, "summary.md"), "test");
            File.WriteAllText(Path.Combine(logsDir, "001.md"), "test");

            RerunDialog.CleanPlanState(planDir);

            Assert.False(Directory.Exists(artifactsDir));
            Assert.False(Directory.Exists(logsDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CleanPlanState_HandlesNonExistentDirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            Directory.CreateDirectory(planDir);

            var ex = Record.Exception(() => RerunDialog.CleanPlanState(planDir));
            Assert.Null(ex);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CleanPlanState_PreservesOtherDirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var artifactsDir = Path.Combine(planDir, "artifacts");
            var logsDir = Path.Combine(planDir, "logs");
            var verificationDir = Path.Combine(planDir, "verification");
            var revisionsDir = Path.Combine(planDir, "revisions");
            Directory.CreateDirectory(artifactsDir);
            Directory.CreateDirectory(logsDir);
            Directory.CreateDirectory(verificationDir);
            Directory.CreateDirectory(revisionsDir);

            RerunDialog.CleanPlanState(planDir);

            Assert.False(Directory.Exists(artifactsDir));
            Assert.False(Directory.Exists(logsDir));
            Assert.True(Directory.Exists(verificationDir));
            Assert.True(Directory.Exists(revisionsDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CleanPlanState_DeletesNestedArtifacts()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-{Guid.NewGuid()}");
        try
        {
            var planDir = Path.Combine(tempDir, "00001-TestPlan");
            var screenshotsDir = Path.Combine(planDir, "artifacts", "screenshots");
            var sampleDir = Path.Combine(planDir, "artifacts", "sample", "bin");
            Directory.CreateDirectory(screenshotsDir);
            Directory.CreateDirectory(sampleDir);
            File.WriteAllText(Path.Combine(screenshotsDir, "img.png"), "test");
            File.WriteAllText(Path.Combine(sampleDir, "app.dll"), "test");

            RerunDialog.CleanPlanState(planDir);

            Assert.False(Directory.Exists(Path.Combine(planDir, "artifacts")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
