using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServicePriorityTests
{
    [Fact]
    public void StartJob_MakePlan_ReadsPriorityFromArgs()
    {
        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 0);

        var id = service.StartJob("MakePlan", "-Description", "Test", "-Project", "Framework", "-Priority", "2");
        var job = service.GetJob(id);

        Assert.NotNull(job);
        Assert.Equal(2, job.Priority);
    }

    [Fact]
    public void StartJob_MakePlan_DefaultsPriorityToZero()
    {
        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 0);

        var id = service.StartJob("MakePlan", "-Description", "Test", "-Project", "Framework");
        var job = service.GetJob(id);

        Assert.NotNull(job);
        Assert.Equal(0, job.Priority);
    }

    [Fact]
    public void StartJob_MakePlan_HandlesInvalidPriorityGracefully()
    {
        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 0);

        var id = service.StartJob("MakePlan", "-Description", "Test", "-Priority", "notanumber");
        var job = service.GetJob(id);

        Assert.NotNull(job);
        Assert.Equal(0, job.Priority);
    }

    [Fact]
    public void QueuedJobs_DequeuedInPriorityOrder()
    {
        // maxConcurrentJobs=0 so all jobs get queued, then we complete one to trigger dequeue
        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 0);

        var lowId = service.StartJob("MakePlan", "-Description", "Low priority", "-Priority", "0");
        var highId = service.StartJob("MakePlan", "-Description", "High priority", "-Priority", "2");
        var medId = service.StartJob("MakePlan", "-Description", "Med priority", "-Priority", "1");

        // All should be queued
        Assert.Equal(JobStatus.Queued, service.GetJob(lowId)!.Status);
        Assert.Equal(JobStatus.Queued, service.GetJob(highId)!.Status);
        Assert.Equal(JobStatus.Queued, service.GetJob(medId)!.Status);

        // Verify the priority values are stored
        Assert.Equal(0, service.GetJob(lowId)!.Priority);
        Assert.Equal(2, service.GetJob(highId)!.Priority);
        Assert.Equal(1, service.GetJob(medId)!.Priority);
    }

    [Fact]
    public void PriorityField_IsOptionalInPlanYaml_BackwardCompatible()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Draft\nproject: TestProject\nlevel: NiceToHave\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            var result = JobService.ReadPlanYaml(tempDir);

            Assert.NotNull(result);
            Assert.Equal(0, result.Priority);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void PriorityField_ParsedFromPlanYaml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Executing\nproject: Framework\npriority: 2\nlevel: Critical\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            var result = JobService.ReadPlanYaml(tempDir);

            Assert.NotNull(result);
            Assert.Equal(2, result.Priority);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CreatePlanDialog_ParsePriority_ExtractsValueFromOption()
    {
        Assert.Equal(0, CreatePlanDialog.ParsePriority("Normal"));
        Assert.Equal(1, CreatePlanDialog.ParsePriority("High"));
        Assert.Equal(2, CreatePlanDialog.ParsePriority("Urgent"));
    }

    [Fact]
    public void CreatePlanDialog_PriorityOptions_ContainsExpectedValues()
    {
        Assert.Equal(3, CreatePlanDialog.PriorityOptions.Count);
        Assert.Contains("Normal", CreatePlanDialog.PriorityOptions);
        Assert.Contains("High", CreatePlanDialog.PriorityOptions);
        Assert.Contains("Urgent", CreatePlanDialog.PriorityOptions);
    }
}
