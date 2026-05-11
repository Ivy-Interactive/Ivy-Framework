using Ivy.Core.Auth;

namespace Ivy.Test.Auth;

public class OAuthCallbackRegistryTests
{
    [Fact]
    public void HasPendingForConnection_WithPending_ReturnsTrue()
    {
        var registry = new OAuthCallbackRegistry();
        registry.RegisterPending("conn-1", "option-1");

        Assert.True(registry.HasPendingForConnection("conn-1"));
    }

    [Fact]
    public void HasPendingForConnection_NoPending_ReturnsFalse()
    {
        var registry = new OAuthCallbackRegistry();

        Assert.False(registry.HasPendingForConnection("conn-1"));
    }

    [Fact]
    public void HasPendingForConnection_DifferentConnection_ReturnsFalse()
    {
        var registry = new OAuthCallbackRegistry();
        registry.RegisterPending("conn-1", "option-1");

        Assert.False(registry.HasPendingForConnection("conn-2"));
    }

    [Fact]
    public void HasPendingForConnection_AfterGetAndRemove_ReturnsFalse()
    {
        var registry = new OAuthCallbackRegistry();
        var state = registry.RegisterPending("conn-1", "option-1");

        registry.GetAndRemove(state);

        Assert.False(registry.HasPendingForConnection("conn-1"));
    }

    [Fact]
    public void HasPendingForConnection_MultiplePendingsForSameConnection_ReturnsTrue()
    {
        var registry = new OAuthCallbackRegistry();
        registry.RegisterPending("conn-1", "option-1");
        registry.RegisterPending("conn-1", "option-2");

        Assert.True(registry.HasPendingForConnection("conn-1"));
    }

    [Fact]
    public void HasPendingForConnection_AfterRemovingOne_StillTrueIfAnotherExists()
    {
        var registry = new OAuthCallbackRegistry();
        var state1 = registry.RegisterPending("conn-1", "option-1");
        registry.RegisterPending("conn-1", "option-2");

        registry.GetAndRemove(state1);

        Assert.True(registry.HasPendingForConnection("conn-1"));
    }
}
