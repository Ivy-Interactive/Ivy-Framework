using Ivy.Client;
using Ivy.Core;
using Ivy.Helpers;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public class AuthService(IAuthProvider authProvider, IAuthSession authSession, IClientProvider client, AppSessionStore sessionStore) : IAuthService
{
    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LoginAsync(authSession, email, password, ct), cancellationToken);
        authSession.AuthToken = token;

        if (authSession.HasChangedSince(oldSession))
        {
            client.SetAuthCookies(sessionStore, authSession, reloadPage: authSession.AuthToken != oldSession.AuthToken);
        }
        return token;
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var uri = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetOAuthUriAsync(authSession, option, callback, ct), cancellationToken);

        if (authSession.AuthSessionData != oldSession.AuthSessionData)
        {
            client.SetAuthSessionData(sessionStore, authSession.AuthSessionData);
        }

        return uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.HandleOAuthCallbackAsync(authSession, request, ct), cancellationToken);
        authSession.AuthToken = token;

        if (authSession.HasChangedSince(oldSession))
        {
            client.SetAuthCookies(sessionStore, authSession);
        }

        return token;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        if (string.IsNullOrWhiteSpace(oldSession.AuthToken?.AccessToken))
        {
            return;
        }

        await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LogoutAsync(authSession, ct), cancellationToken);
        authSession.AuthToken = null;

        if (authSession.HasChangedSince(oldSession))
        {
            client.SetAuthCookies(sessionStore, authSession);
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        var token = authSession.AuthToken;

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return null;
        }

        //todo: cache this!

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetUserInfoAsync(authSession, ct), cancellationToken);
    }

    public AuthOption[] GetAuthOptions()
    {
        return authProvider.GetAuthOptions();
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();
        if (oldSession.AuthToken is null)
        {
            return null;
        }

        var refreshedToken = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.RefreshAccessTokenAsync(authSession, ct), cancellationToken);
        authSession.AuthToken = refreshedToken;

        if (authSession.HasChangedSince(oldSession))
        {
            client.SetAuthCookies(sessionStore, authSession, reloadPage: authSession.AuthToken == null);
        }

        return refreshedToken;
    }

    public AuthToken? GetCurrentToken() => authSession.AuthToken;

    public string? GetCurrentSessionData() => authSession.AuthSessionData;

    public IAuthSession GetAuthSession() => authSession;
}
