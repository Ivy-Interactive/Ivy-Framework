namespace Ivy.Test.Auth;

public class ConnectedAccountSessionTests
{
    [Fact]
    public void ConnectedAccounts_ReturnFullIAuthSession()
    {
        var session = new AuthSession(authToken: null);
        var connectedSession = new AuthSession(authToken: new AuthToken("connected-token", "connected-refresh"));
        session.AddConnectedAccount("github", connectedSession);

        var account = session.ConnectedAccounts["github"];

        Assert.IsAssignableFrom<IAuthSession>(account);
        Assert.Equal("connected-token", account.AuthToken?.AccessToken);
        Assert.Equal("connected-refresh", account.AuthToken?.RefreshToken);
    }

    [Fact]
    public void ConnectedAccounts_CanHaveOwnBrokeredSessions()
    {
        var connectedSession = new AuthSession(authToken: new AuthToken("connected-token"));
        connectedSession.AddBrokeredSession("sub-provider", new AuthTokenHandlerSession(authToken: new AuthToken("sub-token")));

        var session = new AuthSession(authToken: null);
        session.AddConnectedAccount("github", connectedSession);

        var account = session.ConnectedAccounts["github"];
        Assert.Single(account.BrokeredSessions);
        Assert.True(account.BrokeredSessions.ContainsKey("sub-provider"));
        Assert.Equal("sub-token", account.BrokeredSessions["sub-provider"].AuthToken?.AccessToken);
    }

    [Fact]
    public void ConnectedAccount_TokenRefreshUpdatesSession()
    {
        var connectedSession = new AuthSession(authToken: new AuthToken("old-token"));
        var session = new AuthSession(authToken: null);
        session.AddConnectedAccount("github", connectedSession);

        // Simulate token refresh
        var newToken = new AuthToken("new-token", "new-refresh");
        session.ConnectedAccounts["github"].AuthToken = newToken;

        Assert.Equal("new-token", session.ConnectedAccounts["github"].AuthToken?.AccessToken);
        Assert.Equal("new-refresh", session.ConnectedAccounts["github"].AuthToken?.RefreshToken);
    }

    [Fact]
    public void AddConnectedAccount_FiresAddedEvent()
    {
        var session = new AuthSession(authToken: null);
        string? addedProvider = null;
        session.ConnectedAccountAdded += provider => addedProvider = provider;

        session.AddConnectedAccount("github", new AuthSession(authToken: null));

        Assert.Equal("github", addedProvider);
    }

    [Fact]
    public void AddConnectedAccount_DoesNotFireEventOnUpdate()
    {
        var session = new AuthSession(authToken: null);
        session.AddConnectedAccount("github", new AuthSession(authToken: new AuthToken("old-token")));

        var eventFired = false;
        session.ConnectedAccountAdded += _ => eventFired = true;

        session.AddConnectedAccount("github", new AuthSession(authToken: new AuthToken("new-token")));

        Assert.False(eventFired);
    }

    [Fact]
    public void RemoveConnectedAccount_FiresRemovedEvent()
    {
        var session = new AuthSession(authToken: null);
        session.AddConnectedAccount("github", new AuthSession(authToken: null));

        string? removedProvider = null;
        session.ConnectedAccountRemoved += provider => removedProvider = provider;

        session.RemoveConnectedAccount("github");

        Assert.Equal("github", removedProvider);
    }

    [Fact]
    public void RemoveConnectedAccount_DoesNotFireEventIfNotPresent()
    {
        var session = new AuthSession(authToken: null);

        var eventFired = false;
        session.ConnectedAccountRemoved += _ => eventFired = true;

        session.RemoveConnectedAccount("nonexistent");

        Assert.False(eventFired);
    }

    [Fact]
    public void ConnectedAccount_ConstructorPreInitialization()
    {
        var preInitialized = new Dictionary<string, IAuthSession>
        {
            ["github"] = new AuthSession(authToken: new AuthToken("pre-init-token"))
        };

        var session = new AuthSession(connectedAccounts: preInitialized);

        Assert.Single(session.ConnectedAccounts);
        Assert.Equal("pre-init-token", session.ConnectedAccounts["github"].AuthToken?.AccessToken);
    }
}
