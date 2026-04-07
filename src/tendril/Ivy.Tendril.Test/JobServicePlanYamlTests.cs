using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServicePlanYamlTests
{
    [Fact]
    public void ReadPlanYamlRaw_ReturnsContentWhenFileExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Draft\nproject: TestProject\nlevel: NiceToHave\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            var result = JobService.ReadPlanYamlRaw(tempDir);

            Assert.NotNull(result);
            Assert.Contains("state: Draft", result);
            Assert.Contains("project: TestProject", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ReadPlanYamlRaw_ReturnsNullWhenFileDoesNotExist()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = JobService.ReadPlanYamlRaw(tempDir);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ReadPlanYaml_DeserializesCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = """
                              state: Executing
                              project: Framework
                              level: Critical
                              title: Test Plan
                              repos:
                              - D:\Repos\TestRepo
                              prs:
                              - https://github.com/test/repo/pull/42
                              commits:
                              - abc1234
                              verifications:
                              - name: DotnetBuild
                                status: Pass
                              - name: DotnetTest
                                status: Pending
                              dependsOn:
                              - 01100-OtherPlan
                              relatedPlans: []
                              """;
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            var result = JobService.ReadPlanYaml(tempDir);

            Assert.NotNull(result);
            Assert.Equal("Executing", result.State);
            Assert.Equal("Framework", result.Project);
            Assert.Equal("Critical", result.Level);
            Assert.Equal("Test Plan", result.Title);
            Assert.Single(result.Repos);
            Assert.Single(result.Prs);
            Assert.Equal("https://github.com/test/repo/pull/42", result.Prs[0]);
            Assert.Single(result.Commits);
            Assert.Equal("abc1234", result.Commits[0]);
            Assert.Equal(2, result.Verifications.Count);
            Assert.Equal("DotnetBuild", result.Verifications[0].Name);
            Assert.Equal("Pass", result.Verifications[0].Status);
            Assert.Equal("Pending", result.Verifications[1].Status);
            Assert.Single(result.DependsOn);
            Assert.Equal("01100-OtherPlan", result.DependsOn[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ReadPlanYaml_ReturnsNullWhenFileDoesNotExist()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = JobService.ReadPlanYaml(tempDir);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ReadPlanYaml_HandlesInvalidYaml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), "{{{{not valid yaml: [[[");

            var result = JobService.ReadPlanYaml(tempDir);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void UpdatePlanYamlFields_UpdatesSingleField()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Draft\nproject: TestProject\nupdated: 2026-01-01T00:00:00Z\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            JobService.UpdatePlanYamlFields(tempDir, ("state", "Executing"));

            var result = File.ReadAllText(Path.Combine(tempDir, "plan.yaml"));
            Assert.Contains("state: Executing", result);
            Assert.Contains("project: TestProject", result);
            Assert.Contains("updated: 2026-01-01T00:00:00Z", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void UpdatePlanYamlFields_UpdatesMultipleFields()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Draft\nproject: TestProject\nupdated: 2026-01-01T00:00:00Z\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            JobService.UpdatePlanYamlFields(tempDir,
                ("state", "Executing"),
                ("updated", "2026-04-06T20:00:00Z"));

            var result = File.ReadAllText(Path.Combine(tempDir, "plan.yaml"));
            Assert.Contains("state: Executing", result);
            Assert.Contains("updated: 2026-04-06T20:00:00Z", result);
            Assert.Contains("project: TestProject", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void UpdatePlanYamlFields_HandlesNonExistentFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Should not throw — just returns early
            JobService.UpdatePlanYamlFields(tempDir, ("state", "Executing"));

            Assert.False(File.Exists(Path.Combine(tempDir, "plan.yaml")));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void UpdatePlanYamlFields_PreservesOtherContent()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent =
                "state: Draft\nproject: TestProject\nlevel: Critical\ntitle: My Plan\nupdated: 2026-01-01T00:00:00Z\nrepos:\n- D:\\Repos\\Test\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            JobService.UpdatePlanYamlFields(tempDir, ("state", "Completed"));

            var result = File.ReadAllText(Path.Combine(tempDir, "plan.yaml"));
            Assert.Contains("state: Completed", result);
            Assert.Contains("project: TestProject", result);
            Assert.Contains("level: Critical", result);
            Assert.Contains("title: My Plan", result);
            Assert.Contains("updated: 2026-01-01T00:00:00Z", result);
            Assert.Contains("- D:\\Repos\\Test", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SetPlanStateByFolder_UpdatesStateAndTimestamp()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Draft\nproject: TestProject\nupdated: 2026-01-01T00:00:00Z\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            JobService.SetPlanStateByFolder(tempDir, "Executing");

            var result = File.ReadAllText(Path.Combine(tempDir, "plan.yaml"));
            Assert.Contains("state: Executing", result);
            Assert.DoesNotContain("updated: 2026-01-01T00:00:00Z", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SetPlanStateByFolder_UsesCorrectTimestampFormat()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var yamlContent = "state: Draft\nproject: TestProject\nupdated: 2026-01-01T00:00:00Z\n";
            File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yamlContent);

            JobService.SetPlanStateByFolder(tempDir, "ReadyForReview");

            var result = File.ReadAllText(Path.Combine(tempDir, "plan.yaml"));
            // Verify ISO 8601 format with Z suffix: yyyy-MM-ddTHH:mm:ssZ
            Assert.Matches(@"updated: \d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}