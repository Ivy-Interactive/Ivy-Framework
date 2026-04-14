using Ivy.Tendril.Commands;

namespace Ivy.Tendril.Test;

public class DoctorCommandPlansTests : IDisposable
{
    private readonly string _plansDir;

    public DoctorCommandPlansTests()
    {
        _plansDir = Path.Combine(Path.GetTempPath(), $"tendril-doctor-plans-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_plansDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_plansDir))
            try { Directory.Delete(_plansDir, true); }
            catch { /* best effort */ }
    }

    private string CreatePlan(string folderName, string? yamlContent = null)
    {
        var planDir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(planDir);
        if (yamlContent != null)
            File.WriteAllText(Path.Combine(planDir, "plan.yaml"), yamlContent);
        return planDir;
    }

    private static readonly string ValidYaml = """
        state: Completed
        project: TestProject
        title: Test Plan
        repos:
        - /dummy/repo
        commits: []
        """;

    [Fact]
    public void DoctorPlans_HealthyPlan_ReturnsOK()
    {
        CreatePlan("00001-HealthyPlan", ValidYaml);

        var results = DoctorCommand.ScanPlans(_plansDir);

        Assert.Single(results);
        Assert.Equal("00001", results[0].Id);
        Assert.Equal("HealthyPlan", results[0].Title);
        Assert.Equal("Completed", results[0].State);
        Assert.Equal(0, results[0].Worktrees);
        Assert.Equal("OK", results[0].Health);
        Assert.True(results[0].IsHealthy);
    }

    [Fact]
    public void DoctorPlans_MissingYaml_ReportsError()
    {
        CreatePlan("00002-MissingYaml");

        var results = DoctorCommand.ScanPlans(_plansDir);

        Assert.Single(results);
        Assert.False(results[0].IsHealthy);
        Assert.Contains("YAML:Missing", results[0].Health);
        Assert.Equal("Unknown", results[0].State);
    }

    [Fact]
    public void DoctorPlans_InvalidYaml_ReportsError()
    {
        CreatePlan("00003-InvalidYaml", "title: OnlyTitle\nrepos: []\n");

        var results = DoctorCommand.ScanPlans(_plansDir);

        Assert.Single(results);
        Assert.False(results[0].IsHealthy);
        Assert.Contains("YAML:No repos", results[0].Health);
    }

    [Fact]
    public void DoctorPlans_WithWorktrees_CountsCorrectly()
    {
        var planDir = CreatePlan("00004-WithWorktrees", ValidYaml);
        var wtDir = Path.Combine(planDir, "worktrees");
        Directory.CreateDirectory(Path.Combine(wtDir, "RepoA"));
        Directory.CreateDirectory(Path.Combine(wtDir, "RepoB"));

        var results = DoctorCommand.ScanPlans(_plansDir);

        Assert.Single(results);
        Assert.Equal(2, results[0].Worktrees);
        Assert.False(results[0].IsHealthy);
        Assert.Contains("StaleWorktree", results[0].Health);
    }

    [Fact]
    public void DoctorPlans_NestedWorktree_DetectsIssue()
    {
        var planDir = CreatePlan("00005-NestedWt", ValidYaml);
        var wtRepoDir = Path.Combine(planDir, "worktrees", "SomeRepo");
        Directory.CreateDirectory(wtRepoDir);
        File.WriteAllText(Path.Combine(wtRepoDir, ".git"), "gitdir: /some/path");

        var results = DoctorCommand.ScanPlans(_plansDir);

        Assert.Single(results);
        Assert.False(results[0].IsHealthy);
        Assert.Contains("NestedWorktree", results[0].Health);
    }

    [Fact]
    public void DoctorPlans_UnhealthyFlag_FiltersResults()
    {
        CreatePlan("00010-Healthy", ValidYaml);
        CreatePlan("00011-Broken");

        var allResults = DoctorCommand.ScanPlans(_plansDir);
        var filtered = allResults.Where(r => !r.IsHealthy).ToList();

        Assert.Equal(2, allResults.Count);
        Assert.Single(filtered);
        Assert.Equal("00011", filtered[0].Id);
    }

    [Fact]
    public void HasNestedWorktrees_WithNestedGit_ReturnsTrue()
    {
        var planDir = CreatePlan("00020-WithNested", ValidYaml);
        var wtRepoDir = Path.Combine(planDir, "worktrees", "SomeRepo");
        Directory.CreateDirectory(wtRepoDir);
        File.WriteAllText(Path.Combine(wtRepoDir, ".git"), "gitdir: /some/path");

        var result = DoctorCommand.HasNestedWorktrees(planDir);

        Assert.True(result);
    }

    [Fact]
    public void HasNestedWorktrees_NoWorktrees_ReturnsFalse()
    {
        var planDir = CreatePlan("00021-NoWt", ValidYaml);

        var result = DoctorCommand.HasNestedWorktrees(planDir);

        Assert.False(result);
    }

    [Fact]
    public void HasStaleWorktrees_WithStaleDir_ReturnsTrue()
    {
        var planDir = CreatePlan("00022-Stale", ValidYaml);
        var wtDir = Path.Combine(planDir, "worktrees", "StaleRepo");
        Directory.CreateDirectory(wtDir);

        var result = DoctorCommand.HasStaleWorktrees(planDir);

        Assert.True(result);
    }

    [Fact]
    public void HasStaleWorktrees_WithValidGit_ReturnsFalse()
    {
        var planDir = CreatePlan("00023-Valid", ValidYaml);
        var wtDir = Path.Combine(planDir, "worktrees", "ValidRepo");
        Directory.CreateDirectory(wtDir);
        File.WriteAllText(Path.Combine(wtDir, ".git"), "gitdir: /some/path");

        var result = DoctorCommand.HasStaleWorktrees(planDir);

        Assert.False(result);
    }

    [Fact]
    public void RepairPlan_StaleWorktree_RemovesDirectory()
    {
        var planDir = CreatePlan("00024-RepairStale", ValidYaml);
        var wtDir = Path.Combine(planDir, "worktrees", "StaleRepo");
        Directory.CreateDirectory(wtDir);
        File.WriteAllText(Path.Combine(wtDir, "dummy.txt"), "test");

        var healthResult = new DoctorCommand.PlanHealthResult(
            "00024", "RepairStale", "Draft", 1, "StaleWorktree", false);

        var result = DoctorCommand.RepairPlan(planDir, healthResult);

        Assert.True(result.Success);
        Assert.Contains("removed stale worktrees", result.Message);
        Assert.False(Directory.Exists(wtDir));
    }

    [Fact]
    public void RepairPlan_NestedWorktree_RemovesNested()
    {
        var planDir = CreatePlan("00025-RepairNested", ValidYaml);
        var wtDir = Path.Combine(planDir, "worktrees", "SomeRepo");
        Directory.CreateDirectory(wtDir);
        var nestedPlansDir = Path.Combine(wtDir, "Plans");
        Directory.CreateDirectory(nestedPlansDir);
        File.WriteAllText(Path.Combine(nestedPlansDir, "dummy.txt"), "test");

        var healthResult = new DoctorCommand.PlanHealthResult(
            "00025", "RepairNested", "Draft", 1, "NestedWorktree", false);

        var result = DoctorCommand.RepairPlan(planDir, healthResult);

        Assert.True(result.Success);
        Assert.Contains("cleaned nested worktrees", result.Message);
        Assert.False(Directory.Exists(nestedPlansDir));
    }

    [Fact]
    public void CheckYamlHealth_EmptyFile_ReportsEmpty()
    {
        var path = Path.Combine(_plansDir, "empty.yaml");
        File.WriteAllText(path, "");

        var (healthy, error, state) = DoctorCommand.CheckYamlHealth(path);

        Assert.False(healthy);
        Assert.Equal("Empty", error);
        Assert.Equal("Unknown", state);
    }

    [Fact]
    public void CheckYamlHealth_ValidFile_ExtractsState()
    {
        var path = Path.Combine(_plansDir, "valid.yaml");
        File.WriteAllText(path, ValidYaml);

        var (healthy, error, state) = DoctorCommand.CheckYamlHealth(path);

        Assert.True(healthy);
        Assert.Null(error);
        Assert.Equal("Completed", state);
    }
}
