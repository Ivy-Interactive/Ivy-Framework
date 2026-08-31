using Ivy.Core.Hooks;
using Ivy.Core.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test.Auth;

public class ConnectedAccountButtonTests
{
    [Theory]
    [InlineData("github", "Github")]
    [InlineData("google", "Google")]
    [InlineData("discord", "Discord")]
    [InlineData("myProvider", "MyProvider")]
    public void FormatProviderName_CapitalizesFirstLetter(string input, string expected)
    {
        Assert.Equal(expected, ConnectedAccountButton.FormatProviderName(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void FormatProviderName_HandlesEmptyOrNull(string? input)
    {
        Assert.Equal(input ?? "", ConnectedAccountButton.FormatProviderName(input ?? ""));
    }

    [Theory]
    [InlineData(OAuthProviders.GitHub, Icons.Github)]
    [InlineData(OAuthProviders.Google, Icons.Google)]
    [InlineData(OAuthProviders.Microsoft, Icons.Microsoft)]
    [InlineData(OAuthProviders.Discord, Icons.Discord)]
    [InlineData(OAuthProviders.Apple, Icons.Apple)]
    [InlineData(OAuthProviders.Twitch, Icons.Twitch)]
    [InlineData(OAuthProviders.Twitter, Icons.XTwitter)]
    [InlineData(OAuthProviders.Figma, Icons.Figma)]
    [InlineData(OAuthProviders.Notion, Icons.Notion)]
    [InlineData(OAuthProviders.Claude, Icons.ClaudeCode)]
    public void GetProviderIcon_ReturnsCorrectIcon(string provider, Icons expected)
    {
        Assert.Equal(expected, ConnectedAccountButton.GetProviderIcon(provider));
    }

    [Fact]
    public void GetProviderIcon_ReturnsFallbackForUnknownProvider()
    {
        Assert.Equal(Icons.Link, ConnectedAccountButton.GetProviderIcon("unknown-provider"));
    }

    [Fact]
    public void AccountConnected_EventFiresOnService()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        string? connectedProvider = null;
        service.AccountConnected += p => connectedProvider = p;

        var githubSession = new AuthSession(authToken: new AuthToken("token"));
        authSession.AddConnectedAccount("github", githubSession);

        Assert.Equal("github", connectedProvider);
    }

    [Fact]
    public void AccountDisconnected_EventFiresOnService()
    {
        var authSession = new AuthSession(authToken: null);
        var githubSession = new AuthSession(authToken: new AuthToken("token"));
        authSession.AddConnectedAccount("github", githubSession);

        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        string? disconnectedProvider = null;
        service.AccountDisconnected += p => disconnectedProvider = p;

        authSession.RemoveConnectedAccount("github");

        Assert.Equal("github", disconnectedProvider);
    }

    [Fact]
    public void EventTracking_IgnoresOtherProviders()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        string? connectedProvider = null;
        service.AccountConnected += p => connectedProvider = p;

        var googleSession = new AuthSession(authToken: new AuthToken("token"));
        authSession.AddConnectedAccount("google", googleSession);

        Assert.Equal("google", connectedProvider);
        Assert.Null(service.GetAccountSession("github"));
    }

    [Fact]
    public void IsConnected_DetectedViaAuthToken()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        Assert.Null(service.GetAccountSession("github")?.AuthToken);

        var githubSession = new AuthSession(authToken: new AuthToken("token"));
        authSession.AddConnectedAccount("github", githubSession);

        Assert.NotNull(service.GetAccountSession("github")?.AuthToken);
    }

    private static IServiceProvider CreateEmptyServiceProvider()
    {
        var services = new ServiceCollection();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void UseConnectedAccountState_InitializesWithConnectedStatus()
    {
        var authSession = new AuthSession(authToken: null);
        var githubSession = new AuthSession(authToken: new AuthToken("token"));
        authSession.AddConnectedAccount("github", githubSession);

        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var services = new ServiceCollection();
        services.AddSingleton<IConnectedAccountsService>(service);
        var provider = services.BuildServiceProvider();

        var testView = new TestConnectedAccountView("github", provider);
        var state = testView.GetConnectionState();

        Assert.True(state.Value);
    }

    [Fact]
    public void UseConnectedAccountState_InitializesWithDisconnectedStatus()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var services = new ServiceCollection();
        services.AddSingleton<IConnectedAccountsService>(service);
        var provider = services.BuildServiceProvider();

        var testView = new TestConnectedAccountView("github", provider);
        var state = testView.GetConnectionState();

        Assert.False(state.Value);
    }

    [Fact]
    public void UseConnectedAccountState_ReturnsStateInstance()
    {
        var authSession = new AuthSession(authToken: null);
        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var services = new ServiceCollection();
        services.AddSingleton<IConnectedAccountsService>(service);
        var provider = services.BuildServiceProvider();

        var testView = new TestConnectedAccountView("github", provider);
        var state = testView.GetConnectionState();

        Assert.NotNull(state);
        Assert.IsAssignableFrom<IState<bool>>(state);
    }

    [Fact]
    public void UseConnectedAccountState_WorksWithDifferentProviders()
    {
        var authSession = new AuthSession(authToken: null);
        var googleSession = new AuthSession(authToken: new AuthToken("token"));
        authSession.AddConnectedAccount("google", googleSession);

        var sessionStore = new AppSessionStore();
        var service = new ConnectedAccountsService(
            authSession, CreateEmptyServiceProvider(), null!, sessionStore);

        var services = new ServiceCollection();
        services.AddSingleton<IConnectedAccountsService>(service);
        var provider = services.BuildServiceProvider();

        var githubView = new TestConnectedAccountView("github", provider);
        var googleView = new TestConnectedAccountView("google", provider);

        Assert.False(githubView.GetConnectionState().Value);
        Assert.True(googleView.GetConnectionState().Value);
    }

    private class TestConnectedAccountView : ViewBase
    {
        private readonly string _provider;
        private readonly IServiceProvider _serviceProvider;
        private IState<bool>? _state;

        public TestConnectedAccountView(string provider, IServiceProvider serviceProvider)
        {
            _provider = provider;
            _serviceProvider = serviceProvider;
        }

        public override object? Build()
        {
            _state = UseConnectedAccountState(_provider);
            return null;
        }

        public IState<bool> GetConnectionState()
        {
            var context = new ViewContext(() => { }, null, _serviceProvider);
            BeforeBuild(context);
            Build();
            return _state!;
        }
    }
}
