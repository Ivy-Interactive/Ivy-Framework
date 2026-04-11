using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PromptwareCommandsTests
{
    [Fact]
    public void Handle_ReturnsNegativeOne_ForEmptyArgs()
    {
        var result = PromptwareCommands.Handle(Array.Empty<string>());
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Handle_ReturnsNegativeOne_ForUnknownCommand()
    {
        var result = PromptwareCommands.Handle(new[] { "unknown-command" });
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Handle_MatchesUpdatePromptwaresCommand()
    {
        var result = PromptwareCommands.Handle(new[] { "update-promptwares" });
        Assert.NotEqual(-1, result);
    }
}
