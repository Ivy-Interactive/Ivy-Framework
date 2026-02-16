using Ivy.Client;
using Ivy.Core;
using Ivy.Helpers;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public class AuthService(IAuthProvider authProvider, IAuthSession authSession, IClientProvider client, AppSessionStore sessionStore) : IAuthService
{
    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LoginAsync(authSession, email, password, ct), cancellationToken);
        authSession.AccessToken = token?.AccessToken;
        authSession.RefreshToken = token?.RefreshToken;

        if (authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies(reloadPage: authSession.AccessToken != oldSession.AccessToken || authSession.RefreshToken != oldSession.RefreshToken);
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
            SetAuthSessionDataCookies();
        }

        return uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.HandleOAuthCallbackAsync(authSession, request, ct), cancellationToken);
        authSession.AccessToken = token?.AccessToken;
        authSession.RefreshToken = token?.RefreshToken;

        if (authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies();
        }

        return token;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            await TimeoutHelper.WithTimeoutAsync(ct =>
                authProvider.LogoutAsync(authSession, ct), cancellationToken);
        }

        authSession.AccessToken = null;
        authSession.RefreshToken = null;
        SetAuthCookies();
    }

    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authSession.AccessToken))
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
        if (oldSession.AccessToken is null)
        {
            return null;
        }

        var refreshedToken = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.RefreshAccessTokenAsync(authSession, ct), cancellationToken);
        authSession.AccessToken = refreshedToken?.AccessToken;
        authSession.RefreshToken = refreshedToken?.RefreshToken;

        if (authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies(reloadPage: authSession.AccessToken == null);
        }

        return refreshedToken;
    }

    public AuthToken? GetCurrentToken() =>
        authSession.AccessToken != null
            ? new AuthToken(authSession.AccessToken, authSession.RefreshToken)
            : null;

    public string? GetCurrentSessionData() => authSession.AuthSessionData;

    public IAuthSession GetAuthSession() => authSession;

    public void SetAuthCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        var cookieJarId = sessionStore.RegisterAuthSessionCookies(authSession);
        client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void SetAuthTokenCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        var authToken = authSession.AccessToken != null
            ? new AuthToken(authSession.AccessToken, authSession.RefreshToken)
            : null;
        var cookieJarId = sessionStore.RegisterAuthTokenCookies(authToken);
        client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void SetAuthSessionDataCookies(bool reloadPage = false, bool? triggerMachineReload = null)
    {
        var cookieJarId = sessionStore.RegisterAuthSessionDataCookies(authSession.AuthSessionData);
        client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }
}
