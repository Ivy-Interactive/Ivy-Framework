using Ivy.Core;
using Ivy.Core.Auth;
using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;

// Resharper disable once CheckNamespace
namespace Ivy;

public class AuthProviderService(IAuthProvider authProvider, IAuthProviderSession authSession, IClientProvider client, AppSessionStore sessionStore, IOAuthTokenHandlerRegistry? oauthRegistry = null) : IAuthProviderService
{
    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LoginAsync(authSession, email, password, ct), cancellationToken);
        authSession.AuthToken = token;

        if (authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies(reloadPage: authSession.AuthToken != oldSession.AuthToken);
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
        authSession.AuthToken = token;

        if (authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies();
        }

        return token;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(authSession.AuthToken?.AccessToken))
        {
            await TimeoutHelper.WithTimeoutAsync(ct =>
                authProvider.LogoutAsync(authSession, ct), cancellationToken);
        }

        authSession.AuthToken = null;
        authSession.ClearOAuthProviderSessions();
        SetAuthCookies();
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
            SetAuthCookies(reloadPage: authSession.AuthToken == null);
        }

        return refreshedToken;
    }

    public async Task<bool> ValidateAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (authSession.AuthToken is null)
        {
            return false;
        }

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.ValidateAccessTokenAsync(authSession, ct), cancellationToken);
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(CancellationToken cancellationToken)
    {
        if (authSession.AuthToken is null)
        {
            return null;
        }

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetAccessTokenLifetimeAsync(authSession, ct), cancellationToken);
    }

    public AuthToken? GetCurrentToken() => authSession.AuthToken;

    public string? GetCurrentSessionData() => authSession.AuthSessionData;

    public IAuthTokenHandlerSession GetAuthTokenHandlerSession() => authSession;

    public IAuthProviderSession GetAuthProviderSession() => authSession;

    public async Task<OAuthProviderSessionsResult> GetOAuthProviderSessionsAsync(bool skipCache = false, CancellationToken cancellationToken = default)
    {
        var result = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetOAuthProviderSessionsAsync(authSession, skipCache, ct), cancellationToken);

        if (result.Sessions == null)
        {
            return result;
        }

        // Filter to only include providers that have a registered handler
        var filteredSessions = oauthRegistry != null
            ? result.Sessions.Where(kvp => oauthRegistry.GetHandler(kvp.Key) != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            : result.Sessions;

        // Diff and update authSession.OAuthProviderSessions
        var currentProviders = authSession.OAuthProviderSessions.Keys.ToHashSet();
        var newProviders = filteredSessions.Keys.ToHashSet();

        foreach (var provider in currentProviders.Where(p => !newProviders.Contains(p)))
        {
            authSession.RemoveOAuthProviderSession(provider);
        }

        // Add or update providers
        foreach (var kvp in filteredSessions)
        {
            authSession.AddOAuthProviderSession(kvp.Key, kvp.Value);
        }

        return OAuthProviderSessionsResult.Success(filteredSessions);
    }

    public void SetAuthCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        var cookieJarId = sessionStore.RegisterAuthSessionCookies(authSession);
        client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void SetAuthTokenCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        var cookieJarId = sessionStore.RegisterAuthTokenCookies(authSession.AuthToken);
        client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }

    public void SetAuthSessionDataCookies(bool reloadPage = false, bool? triggerMachineReload = null)
    {
        var cookieJarId = sessionStore.RegisterAuthSessionDataCookies(authSession.AuthSessionData);
        client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }
}
