using Ivy.Core;
using Ivy.Core.Auth;
using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Resharper disable once CheckNamespace
namespace Ivy;

public class AuthService(IAuthProvider authProvider, IAuthSession authSession, IClientProvider client, AppSessionStore sessionStore, string machineId, IServiceProvider? serviceProvider = null, ILogger<AuthService>? logger = null) : IAuthService
{
    // Hold removed OAuth provider sessions so they can be updated in place and restored later
    private readonly Dictionary<string, IAuthTokenHandlerSession> _removedOAuthSessions = new();

    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var oldSession = authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LoginAsync(authSession, email, password, ct), cancellationToken);
        authSession.AuthToken = token;

        // Clear removed OAuth providers list on successful login
        if (token != null)
        {
            sessionStore.ClearRemovedOAuthProviders(machineId);
        }

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

        // Capture OAuth providers before clearing so we can delete their cookies
        var providersToDelete = authSession.OAuthSessions.Keys.ToList();

        // Mark OAuth providers as removed globally so other tabs know not to re-add them
        foreach (var provider in providersToDelete)
        {
            sessionStore.MarkOAuthProviderRemoved(machineId, provider);
        }

        authSession.AuthToken = null;
        authSession.ClearOAuthSessions();
        _removedOAuthSessions.Clear();

        // Pass the captured providers to delete their cookies
        var cookieJarId = sessionStore.RegisterAuthSessionCookies(authSession, machineId, providersToDelete);
        client.SetAuthCookies(cookieJarId, reloadPage: true, triggerMachineReload: null);
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

    public IAuthSession GetAuthSession() => authSession;

    public async Task<OAuthSessionsResult> GetOAuthSessionsAsync(bool skipCache = false, CancellationToken cancellationToken = default)
    {
        var result = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetOAuthSessionsAsync(authSession, skipCache, ct), cancellationToken);

        if (result.Sessions == null)
        {
            return result;
        }

        // Filter to only include providers that have a registered handler
        var filteredSessions = serviceProvider != null
            ? result.Sessions.Where(kvp => serviceProvider.GetKeyedService<IAuthTokenHandler>(kvp.Key) != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            : result.Sessions;

        var unhandledSessions = result.Sessions?.Where(kvp => !filteredSessions.ContainsKey(kvp.Key)).Select(s => s.Key).ToList();
        if (unhandledSessions != null && unhandledSessions.Count > 0)
        {
            logger?.LogWarning("The following OAuth provider sessions are available but have no registered handler and will be ignored: {UnhandledProviders}", string.Join(", ", unhandledSessions));
        }

        // Diff and update authSession.OAuthSessions
        var currentProviders = authSession.OAuthSessions.Keys.ToHashSet();
        var newProviders = filteredSessions.Keys.ToHashSet();

        // Remove providers that are no longer present, but keep them in _removedOAuthSessions
        foreach (var provider in currentProviders.Where(p => !newProviders.Contains(p)))
        {
            if (authSession.OAuthSessions.TryGetValue(provider, out var sessionToRemove))
            {
                _removedOAuthSessions[provider] = sessionToRemove;
            }
            authSession.RemoveOAuthSession(provider);
        }

        // Add or update sessions
        bool hasChanges = false;
        foreach (var kvp in filteredSessions)
        {
            // Check if session exists in active sessions
            if (authSession.OAuthSessions.TryGetValue(kvp.Key, out var existingSession))
            {
                // Update existing active session in place to preserve references
                existingSession.AuthToken = kvp.Value.AuthToken;
                existingSession.AuthSessionData = kvp.Value.AuthSessionData;
            }
            // Check if session exists in removed sessions
            else if (_removedOAuthSessions.Remove(kvp.Key, out var removedSession))
            {
                // Update the removed session in place and restore it to active sessions
                removedSession.AuthToken = kvp.Value.AuthToken;
                removedSession.AuthSessionData = kvp.Value.AuthSessionData;
                authSession.AddOAuthSession(kvp.Key, removedSession);
                hasChanges = true;
            }
            else
            {
                // New session, add it
                authSession.AddOAuthSession(kvp.Key, kvp.Value);
                hasChanges = true;
            }
        }

        if (hasChanges || currentProviders.Count != newProviders.Count)
        {
            SetAuthCookies(reloadPage: false);
        }

        return OAuthSessionsResult.Success(filteredSessions);
    }

    public void SetAuthCookies(bool reloadPage = true, bool? triggerMachineReload = null)
    {
        var cookieJarId = sessionStore.RegisterAuthSessionCookies(authSession, machineId);
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
