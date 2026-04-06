using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class IConfigServiceTests
{
    [Fact]
    public void ConfigService_ImplementsInterface()
    {
        var config = new ConfigService(new TendrilSettings(), "");

        Assert.IsAssignableFrom<IConfigService>(config);
    }

    [Fact]
    public void InterfaceExposes_AllExpectedProperties()
    {
        IConfigService config = new ConfigService(new TendrilSettings(), "/tmp/test");

        Assert.NotNull(config.Settings);
        Assert.NotNull(config.TendrilHome);
        Assert.NotNull(config.ConfigPath);
        Assert.NotNull(config.PlanFolder);
        Assert.NotNull(config.Projects);
        Assert.NotNull(config.Levels);
        Assert.NotNull(config.LevelNames);
        Assert.NotNull(config.Editor);
    }

    [Fact]
    public void InterfaceExposes_AllExpectedMethods()
    {
        IConfigService config = new ConfigService(new TendrilSettings
        {
            Projects = new List<ProjectConfig>
            {
                new() { Name = "TestProject", Color = "Blue" }
            }
        }, "");

        Assert.Equal("TestProject", config.GetProject("TestProject")?.Name);
        Assert.Null(config.GetProject("NonExistent"));
        Assert.Equal(BadgeVariant.Outline, config.GetBadgeVariant("UnknownLevel"));
        Assert.Null(config.GetProjectColor("NonExistent"));
        Assert.Equal(Colors.Blue, config.GetProjectColor("TestProject"));
    }

    [Fact]
    public void InterfaceExposes_PendingState()
    {
        IConfigService config = new ConfigService(new TendrilSettings(), "");

        config.SetPendingTendrilHome("/tmp/pending");
        Assert.Equal("/tmp/pending", config.GetPendingTendrilHome());

        var project = new ProjectConfig { Name = "Test" };
        config.SetPendingProject(project);
        Assert.Equal("Test", config.GetPendingProject()?.Name);
    }

    [Fact]
    public void LevelNames_ReturnsSameArrayOnSubsequentCalls()
    {
        var config = new ConfigService(new TendrilSettings
        {
            Levels = new List<LevelConfig>
            {
                new() { Name = "Bug" },
                new() { Name = "Critical" }
            }
        }, "");

        var first = config.LevelNames;
        var second = config.LevelNames;

        Assert.Same(first, second);
    }

    [Fact]
    public void LevelNames_InvalidatedAfterSaveSettings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var config = new ConfigService(new TendrilSettings
            {
                Levels = new List<LevelConfig>
                {
                    new() { Name = "Bug" },
                    new() { Name = "Critical" }
                }
            }, tempDir);

            var first = config.LevelNames;
            config.SaveSettings();
            var second = config.LevelNames;

            Assert.NotSame(first, second);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LevelNames_ReturnsCorrectNames()
    {
        var config = new ConfigService(new TendrilSettings
        {
            Levels = new List<LevelConfig>
            {
                new() { Name = "Bug", Badge = "Destructive" },
                new() { Name = "Critical", Badge = "Warning" },
                new() { Name = "NiceToHave", Badge = "Outline" }
            }
        }, "");

        var names = config.LevelNames;

        Assert.Equal(new[] { "Bug", "Critical", "NiceToHave" }, names);
    }
}
