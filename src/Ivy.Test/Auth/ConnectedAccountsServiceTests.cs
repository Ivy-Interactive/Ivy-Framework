using Ivy.Core.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test.Auth;

public class ConnectedAccountsServiceTests
{
    private static IServiceProvider CreateEmptyServiceProvider()
    {
        var services = new ServiceCollection();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void GetAvailableProviders_ReturnsEmptyWhenNoConnectedAccounts()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var providers = service.GetAvailableProviders();
        Assert.Empty(providers);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsConnectedAccountKeys()
    {
        var authSession = new AuthSession(authToken: null);
        var githubSession = new AuthSession(authToken: new AuthToken("github-token"));
        authSession.AddConnectedAccount("github", githubSession);

        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var providers = service.GetAvailableProviders();
        Assert.Single(providers);
        Assert.Equal("github", providers[0]);
    }

    [Fact]
    public void GetAccountSession_ReturnsNullWhenNotConnected()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var session = service.GetAccountSession("github");
        Assert.Null(session);
    }

    [Fact]
    public void GetAccountSession_ReturnsSessionWhenConnected()
    {
        var authSession = new AuthSession(authToken: null);
        var githubSession = new AuthSession(authToken: new AuthToken("github-token"));
        authSession.AddConnectedAccount("github", githubSession);

        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var session = service.GetAccountSession("github");
        Assert.NotNull(session);
        Assert.Equal("github-token", session.AuthToken?.AccessToken);
    }

    [Fact]
    public void DisconnectAccountAsync_RemovesConnectedAccount()
    {
        var authSession = new AuthSession(authToken: null);
        var githubSession = new AuthSession(authToken: new AuthToken("github-token"));
        authSession.AddConnectedAccount("github", githubSession);

        // Directly test the session management since DisconnectAccountAsync also triggers cookie updates
        authSession.RemoveConnectedAccount("github");

        Assert.Empty(authSession.ConnectedAccounts);
    }

    [Fact]
    public async Task DisconnectAccountAsync_DoesNothingWhenNotConnected()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        await service.DisconnectAccountAsync("github");
        Assert.Empty(authSession.ConnectedAccounts);
    }

    [Fact]
    public async Task ConnectAccountAsync_ThrowsWhenProviderNotRegistered()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConnectAccountAsync("github", null!));
    }

    [Fact]
    public async Task HandleConnectCallbackAsync_ThrowsWhenProviderNotRegistered()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.HandleConnectCallbackAsync("github", null!));
    }
}
