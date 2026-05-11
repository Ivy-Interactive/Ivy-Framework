using Ivy.Core.Auth;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A reusable button for connecting or disconnecting a third-party account provider.
/// When not connected, renders a "Connect [Provider]" button that initiates the OAuth flow.
/// When connected, renders a "Disconnect" button.
/// Automatically tracks connection state via <see cref="IConnectedAccountsService"/> events.
/// </summary>
public class ConnectedAccountButton(string provider, string? displayName = null, Icons? icon = null) : ViewBase
{
    public override object? Build()
    {
        var appContext = UseService<AppContext>();
        var loginRegistry = UseService<IOAuthLoginRegistry>();
        var connectedAccounts = UseService<IConnectedAccountsService>();

        var isConnected = UseConnectedAccountState(provider);

        var name = displayName ?? FormatProviderName(provider);
        var resolvedIcon = icon ?? GetProviderIcon(provider);

        if (isConnected.Value)
        {
            return new Button($"Disconnect {name}", async () =>
            {
                await connectedAccounts.DisconnectAccountAsync(provider);
            }, variant: ButtonVariant.Destructive)
                .Icon(resolvedIcon);
        }

        var loginId = UseState(() => loginRegistry.RegisterPending(appContext.ConnectionId, provider, provider));
        var connectUrl = $"{appContext.BaseUrl.TrimEnd('/')}/ivy/auth/oauth-login?loginId={Uri.EscapeDataString(loginId.Value)}";

        return new Button($"Connect {name}")
            .Icon(resolvedIcon)
            .Variant(ButtonVariant.Outline)
            .Url(connectUrl);
    }

    internal static string FormatProviderName(string provider) =>
        string.IsNullOrEmpty(provider) ? provider : char.ToUpper(provider[0]) + provider[1..];

    internal static Icons GetProviderIcon(string provider) => provider switch
    {
        OAuthProviders.GitHub => Icons.Github,
        OAuthProviders.Google => Icons.Google,
        OAuthProviders.Microsoft => Icons.Microsoft,
        OAuthProviders.Discord => Icons.Discord,
        OAuthProviders.Apple => Icons.Apple,
        OAuthProviders.Twitch => Icons.Twitch,
        OAuthProviders.Twitter => Icons.XTwitter,
        OAuthProviders.Figma => Icons.Figma,
        OAuthProviders.Notion => Icons.Notion,
        _ => Icons.Link,
    };
}
