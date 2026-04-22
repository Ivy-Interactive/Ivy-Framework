namespace Ivy.Test.Auth;

public class ConnectedAccountLifecycleTests
{
    [Fact]
    public void ConnectedAccounts_PersistThroughMainAuthLogout()
    {
        var session = new AuthSession(authToken: new AuthToken("main-token"));
        var githubSession = new AuthSession(authToken: new AuthToken("github-token"));
        session.AddConnectedAccount("github", githubSession);
        session.AddBrokeredSession("google", new AuthTokenHandlerSession(authToken: new AuthToken("google-brokered")));

        // Simulate logout: clear main token and brokered sessions
        session.AuthToken = null;
        session.ClearBrokeredSessions();

        // Connected accounts should persist
        Assert.Single(session.ConnectedAccounts);
        Assert.True(session.ConnectedAccounts.ContainsKey("github"));
        Assert.Equal("github-token", session.ConnectedAccounts["github"].AuthToken?.AccessToken);

        // Brokered sessions should be cleared
        Assert.Empty(session.BrokeredSessions);
    }

    [Fact]
    public void ConnectedAccounts_ClearedOnlyWhenExplicitlyDisconnected()
    {
        var session = new AuthSession(authToken: new AuthToken("main-token"));
        var githubSession = new AuthSession(authToken: new AuthToken("github-token"));
        var slackSession = new AuthSession(authToken: new AuthToken("slack-token"));
        session.AddConnectedAccount("github", githubSession);
        session.AddConnectedAccount("slack", slackSession);

        // Disconnect only github
        session.RemoveConnectedAccount("github");

        Assert.Single(session.ConnectedAccounts);
        Assert.False(session.ConnectedAccounts.ContainsKey("github"));
        Assert.True(session.ConnectedAccounts.ContainsKey("slack"));
    }

    [Fact]
    public void ClearConnectedAccounts_RemovesAll()
    {
        var session = new AuthSession(authToken: new AuthToken("main-token"));
        session.AddConnectedAccount("github", new AuthSession(authToken: new AuthToken("github-token")));
        session.AddConnectedAccount("slack", new AuthSession(authToken: new AuthToken("slack-token")));

        session.ClearConnectedAccounts();

        Assert.Empty(session.ConnectedAccounts);
    }

    [Fact]
    public void ClearConnectedAccounts_FiresEventsForEachProvider()
    {
        var session = new AuthSession(authToken: null);
        session.AddConnectedAccount("github", new AuthSession(authToken: null));
        session.AddConnectedAccount("slack", new AuthSession(authToken: null));

        var removedProviders = new List<string>();
        session.ConnectedAccountRemoved += provider => removedProviders.Add(provider);

        session.ClearConnectedAccounts();

        Assert.Equal(2, removedProviders.Count);
        Assert.Contains("github", removedProviders);
        Assert.Contains("slack", removedProviders);
    }
}
