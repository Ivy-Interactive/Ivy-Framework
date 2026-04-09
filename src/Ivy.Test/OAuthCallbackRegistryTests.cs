using Ivy.Core.Auth;

namespace Ivy.Test;

public class OAuthCallbackRegistryTests
{
    [Fact]
    public void RegisterPending_ReturnsUniqueStates()
    {
        var registry = new OAuthCallbackRegistry();
        var state1 = registry.RegisterPending("conn1", "opt1");
        var state2 = registry.RegisterPending("conn2", "opt2");

        Assert.NotEqual(state1, state2);
    }

    [Fact]
    public void RegisterPending_StateIsUrlSafe()
    {
        var registry = new OAuthCallbackRegistry();
        var state = registry.RegisterPending("conn1", "opt1");

        Assert.DoesNotContain("+", state);
        Assert.DoesNotContain("/", state);
        Assert.DoesNotContain("=", state);
    }

    [Fact]
    public void GetAndRemove_ValidState_ReturnsCallback()
    {
        var registry = new OAuthCallbackRegistry();
        var state = registry.RegisterPending("conn1", "opt1");

        var result = registry.GetAndRemove(state);

        Assert.NotNull(result);
        Assert.Equal("conn1", result.ConnectionId);
        Assert.Equal("opt1", result.OptionId);
    }

    [Fact]
    public void GetAndRemove_InvalidState_ReturnsNull()
    {
        var registry = new OAuthCallbackRegistry();

        var result = registry.GetAndRemove("nonexistent-state");

        Assert.Null(result);
    }

    [Fact]
    public void GetAndRemove_NullOrEmptyState_ReturnsNull()
    {
        var registry = new OAuthCallbackRegistry();

        Assert.Null(registry.GetAndRemove(null!));
        Assert.Null(registry.GetAndRemove(""));
    }

    [Fact]
    public void GetAndRemove_SameStateTwice_ReturnsNullSecondTime()
    {
        var registry = new OAuthCallbackRegistry();
        var state = registry.RegisterPending("conn1", "opt1");

        var first = registry.GetAndRemove(state);
        var second = registry.GetAndRemove(state);

        Assert.NotNull(first);
        Assert.Null(second);
    }
}
