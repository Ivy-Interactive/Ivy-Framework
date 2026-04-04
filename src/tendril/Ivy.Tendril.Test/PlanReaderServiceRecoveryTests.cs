using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PlanReaderServiceRecoveryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly PlanReaderService _service;

    public PlanReaderServiceRecoveryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var settings = new TendrilSettings();
        Directory.CreateDirectory(Path.Combine(_tempDir, "Plans"));
        var configService = new ConfigService(settings, _tempDir);
        _service = new PlanReaderService(configService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void CreatePlan(string folderName, string state)
    {
        var dir = Path.Combine(_tempDir, folderName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"),
            $"state: {state}\nproject: Test\ntitle: Test Plan\nupdated: 2026-01-01T00:00:00Z\n");
    }

    private string ReadState(string folderName)
    {
        var yaml = File.ReadAllText(Path.Combine(_tempDir, folderName, "plan.yaml"));
        var match = System.Text.RegularExpressions.Regex.Match(yaml, @"(?m)^state:\s*(.+)$");
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    [Fact]
    public void Executing_Plans_Are_Recovered_To_Failed()
    {
        CreatePlan("01099-TestPlan", "Executing");

        _service.RecoverStuckPlans();

        Assert.Equal("Failed", ReadState("01099-TestPlan"));
    }

    [Fact]
    public void Building_Plans_Are_Recovered_To_Draft()
    {
        CreatePlan("01100-BuildPlan", "Building");

        _service.RecoverStuckPlans();

        Assert.Equal("Draft", ReadState("01100-BuildPlan"));
    }

    [Fact]
    public void Updating_Plans_Are_Recovered_To_Draft()
    {
        CreatePlan("01101-UpdatePlan", "Updating");

        _service.RecoverStuckPlans();

        Assert.Equal("Draft", ReadState("01101-UpdatePlan"));
    }

    [Fact]
    public void Draft_Plans_Are_Not_Changed()
    {
        CreatePlan("01102-DraftPlan", "Draft");

        _service.RecoverStuckPlans();

        Assert.Equal("Draft", ReadState("01102-DraftPlan"));
    }

    [Fact]
    public void Failed_Plans_Are_Not_Changed()
    {
        CreatePlan("01103-FailedPlan", "Failed");

        _service.RecoverStuckPlans();

        Assert.Equal("Failed", ReadState("01103-FailedPlan"));
    }

    [Fact]
    public void One_Bad_Plan_Does_Not_Block_Others()
    {
        // Create a plan with an unreadable plan.yaml (directory instead of file)
        var badDir = Path.Combine(_tempDir, "01104-BadPlan");
        Directory.CreateDirectory(badDir);
        Directory.CreateDirectory(Path.Combine(badDir, "plan.yaml")); // directory, not file

        CreatePlan("01105-GoodPlan", "Executing");

        _service.RecoverStuckPlans();

        Assert.Equal("Failed", ReadState("01105-GoodPlan"));
    }

    [Fact]
    public void Multiple_Stuck_Plans_Are_All_Recovered()
    {
        CreatePlan("01200-Plan1", "Executing");
        CreatePlan("01201-Plan2", "Building");
        CreatePlan("01202-Plan3", "Updating");
        CreatePlan("01203-Plan4", "Draft");
        CreatePlan("01204-Plan5", "Failed");

        _service.RecoverStuckPlans();

        Assert.Equal("Failed", ReadState("01200-Plan1"));
        Assert.Equal("Draft", ReadState("01201-Plan2"));
        Assert.Equal("Draft", ReadState("01202-Plan3"));
        Assert.Equal("Draft", ReadState("01203-Plan4"));
        Assert.Equal("Failed", ReadState("01204-Plan5"));
    }
}
