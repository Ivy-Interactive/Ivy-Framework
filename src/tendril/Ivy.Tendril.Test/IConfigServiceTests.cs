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
}
