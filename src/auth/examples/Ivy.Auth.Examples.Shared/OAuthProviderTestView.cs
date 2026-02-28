using Ivy;

namespace Ivy.Auth.Examples.Shared;

/// <summary>
/// Unified OAuth provider test view that automatically delegates to the appropriate
/// provider-specific test view based on the provider type.
/// </summary>
public class OAuthProviderTestView : ViewBase
{
    private readonly OAuthProvider _provider;
    private readonly IAuthTokenHandlerSession _session;

    public OAuthProviderTestView(OAuthProvider provider, IAuthTokenHandlerSession session)
    {
        _provider = provider;
        _session = session;
    }

    public override object? Build()
    {
        return _provider switch
        {
            OAuthProvider.Google => new GoogleOAuthTestView(_session),
            OAuthProvider.GitHub => new GitHubOAuthTestView(_session),
            OAuthProvider.Microsoft => new MicrosoftGraphOAuthTestView(_session),
            OAuthProvider.Apple => UnsupportedProviderView("Apple"),
            OAuthProvider.Twitter => UnsupportedProviderView("Twitter"),
            OAuthProvider.Discord => UnsupportedProviderView("Discord"),
            OAuthProvider.Twitch => UnsupportedProviderView("Twitch"),
            OAuthProvider.Figma => UnsupportedProviderView("Figma"),
            OAuthProvider.Notion => UnsupportedProviderView("Notion"),
            OAuthProvider.Azure => UnsupportedProviderView("Azure"),
            OAuthProvider.WorkOS => UnsupportedProviderView("WorkOS"),
            OAuthProvider.GitLab => UnsupportedProviderView("GitLab"),
            OAuthProvider.Bitbucket => UnsupportedProviderView("Bitbucket"),
            _ => UnsupportedProviderView(_provider.ToString())
        };
    }

    private object UnsupportedProviderView(string providerName)
    {
        return Layout.Vertical(
            Text.H4($"{providerName} OAuth"),
            Text.P($"OAuth provider session available for {providerName}, but no test view has been implemented yet."),
            Text.Muted($"Access Token: {_session.AuthToken?.AccessToken?[..Math.Min(20, _session.AuthToken?.AccessToken?.Length ?? 0)]}...")
        ).Gap(10);
    }
}
