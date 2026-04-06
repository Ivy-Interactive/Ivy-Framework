using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PlanReaderServiceGetPlanTests : IDisposable
{
    private readonly string _tempDir;
    private readonly PlanReaderService _service;

    public PlanReaderServiceGetPlanTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var settings = new TendrilSettings();
        var configService = new ConfigService(settings, _tempDir);
        _service = new PlanReaderService(configService, Microsoft.Extensions.Logging.Abstractions.NullLogger<PlanReaderService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string CreatePlanWithRevision(string folderName, string yaml, string? revisionContent = null)
    {
        var dir = Path.Combine(_service.PlansDirectory, folderName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), yaml);

        if (revisionContent != null)
        {
            var revisionsDir = Path.Combine(dir, "revisions");
            Directory.CreateDirectory(revisionsDir);
            File.WriteAllText(Path.Combine(revisionsDir, "001.md"), revisionContent);
        }

        return dir;
    }

    [Fact]
    public void GetPlanByFolder_Returns_PlanFile_For_Existing_Folder()
    {
        var yaml = "state: Draft\nproject: Tendril\ntitle: Test Plan\nrepos:\n- D:\\Repos\\Test\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        var folderPath = CreatePlanWithRevision("01500-TestPlan", yaml, "# Test");

        var result = _service.GetPlanByFolder(folderPath);

        Assert.NotNull(result);
        Assert.Equal(1500, result.Id);
        Assert.Equal("Test Plan", result.Title);
        Assert.Equal("Tendril", result.Project);
    }

    [Fact]
    public void GetPlanByFolder_Returns_Null_For_NonExistent_Folder()
    {
        var result = _service.GetPlanByFolder(Path.Combine(_tempDir, "99999-DoesNotExist"));

        Assert.Null(result);
    }

    [Fact]
    public void ParsePlanFolder_Handles_Null_Yaml_Lists()
    {
        // YAML with empty list fields — YamlDotNet may deserialize these as null
        var yaml = "state: Draft\nproject:\ntitle:\nlevel:\nrepos:\ncommits:\nprs:\nverifications:\nrelatedPlans:\ndependsOn:\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        var folderPath = CreatePlanWithRevision("01501-NullLists", yaml, "# Content");

        var result = _service.GetPlanByFolder(folderPath);

        Assert.NotNull(result);
        Assert.NotNull(result.Repos);
        Assert.NotNull(result.Commits);
        Assert.NotNull(result.Prs);
        Assert.NotNull(result.Verifications);
        Assert.NotNull(result.RelatedPlans);
        Assert.NotNull(result.DependsOn);
        Assert.Equal("", result.Project);
        Assert.Equal("", result.Title);
        Assert.Equal("NiceToHave", result.Level);
    }

    [Fact]
    public void GetPlanByFolder_Returns_Null_For_Invalid_Folder_Name()
    {
        // Folder name doesn't match the expected pattern (NNNNN-Name)
        var dir = Path.Combine(_tempDir, "invalid-folder");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), "state: Draft\nproject: Test\ntitle: Test\n");

        var result = _service.GetPlanByFolder(dir);

        Assert.Null(result);
    }
}
