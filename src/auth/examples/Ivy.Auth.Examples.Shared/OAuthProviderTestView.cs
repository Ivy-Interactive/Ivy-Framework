using Ivy;

namespace Ivy.Auth.Examples.Shared;

/// <summary>
/// Unified OAuth provider test view that automatically delegates to the appropriate
/// provider-specific test view based on the token's provider type.
/// </summary>
public class OAuthProviderTestView : ViewBase
{
    private readonly OAuthProviderToken _token;

    public OAuthProviderTestView(OAuthProviderToken token)
    {
        _token = token;
    }

    public override object? Build()
    {
        return _token.Provider switch
        {
            OAuthProvider.Google => new GoogleOAuthTestView(_token),
            OAuthProvider.GitHub => new GitHubOAuthTestView(_token),
            OAuthProvider.Microsoft => new MicrosoftGraphOAuthTestView(_token),
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
            _ => UnsupportedProviderView(_token.Provider.ToString())
        };
    }

    private object UnsupportedProviderView(string providerName)
    {
        return Layout.Vertical(
            Text.H4($"{providerName} OAuth"),
            Text.P($"OAuth provider token available for {providerName}, but no test view has been implemented yet."),
            Text.Muted($"Access Token: {_token.AuthToken.AccessToken?[..Math.Min(20, _token.AuthToken.AccessToken?.Length ?? 0)]}...")
        ).Gap(10);
    }
}
