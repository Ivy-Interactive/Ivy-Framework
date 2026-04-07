namespace Ivy.Auth.Examples.Shared;

/// <summary>
/// Unified OAuth provider test view that automatically delegates to the appropriate
/// provider-specific test view based on the provider type.
/// </summary>
public class OAuthProviderTestView : ViewBase
{
    private readonly string _provider;
    private readonly IAuthTokenHandlerSession _session;

    public OAuthProviderTestView(string provider, IAuthTokenHandlerSession session)
    {
        _provider = provider;
        _session = session;
    }

    public override object? Build()
    {
        return _provider switch
        {
            OAuthProviders.Google => new GoogleOAuthTestView(_session),
            OAuthProviders.GitHub => new GitHubOAuthTestView(_session),
            OAuthProviders.Microsoft => new MicrosoftGraphOAuthTestView(_session),
            OAuthProviders.Apple => UnsupportedProviderView("Apple"),
            OAuthProviders.Twitter => UnsupportedProviderView("Twitter"),
            OAuthProviders.Discord => UnsupportedProviderView("Discord"),
            OAuthProviders.Twitch => UnsupportedProviderView("Twitch"),
            OAuthProviders.Figma => UnsupportedProviderView("Figma"),
            OAuthProviders.Notion => UnsupportedProviderView("Notion"),
            OAuthProviders.Azure => UnsupportedProviderView("Azure"),
            OAuthProviders.WorkOS => UnsupportedProviderView("WorkOS"),
            OAuthProviders.GitLab => UnsupportedProviderView("GitLab"),
            OAuthProviders.Bitbucket => UnsupportedProviderView("Bitbucket"),
            _ => UnsupportedProviderView(_provider.ToString())
        };
    }

    private object UnsupportedProviderView(string providerName)
    {
        return Layout.Vertical(
            Text.H4($"{providerName} OAuth"),
            Text.P($"Brokered auth session available for {providerName}, but no test view has been implemented yet."),
            Text.Muted($"Access Token: {_session.AuthToken?.AccessToken?[..Math.Min(20, _session.AuthToken?.AccessToken?.Length ?? 0)]}...")
        ).Gap(10);
    }
}
