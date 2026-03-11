using Ivy.Core;
using Ivy.Core.Auth;
using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Resharper disable once CheckNamespace
namespace Ivy;

public class AuthService : AuthTokenHandlerService, IAuthService
{
    private readonly IAuthProvider _authProvider;
    private readonly IAuthSession _authSession;
    private readonly IServiceProvider? _serviceProvider;

    // Hold removed OAuth provider sessions so they can be updated in place and restored later
    private readonly Dictionary<string, IAuthTokenHandlerSession> _removedOAuthSessions = new();

    public AuthService(
        IAuthProvider authProvider,
        IAuthSession authSession,
        IClientProvider client,
        AppSessionStore sessionStore,
        string machineId,
        IServiceProvider? serviceProvider = null,
        ILogger<AuthService>? logger = null)
        : base(authProvider, authSession, client, sessionStore, machineId, logger ?? NullLogger<AuthService>.Instance)
    {
        _authProvider = authProvider;
        _authSession = authSession;
        _serviceProvider = serviceProvider;
    }

    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var oldSession = _authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            _authProvider.LoginAsync(_authSession, email, password, ct), cancellationToken);
        _authSession.AuthToken = token;

        // Clear removed OAuth providers list on successful login
        if (token != null)
        {
            _sessionStore.ClearRemovedOAuthProviders(_machineId!);
        }

        if (_authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies(reloadPage: _authSession.AuthToken != oldSession.AuthToken);
        }
        return token;
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        var oldSession = _authSession.TakeSnapshot();

        var uri = await TimeoutHelper.WithTimeoutAsync(ct =>
            _authProvider.GetOAuthUriAsync(_authSession, option, callback, ct), cancellationToken);

        if (_authSession.AuthSessionData != oldSession.AuthSessionData)
        {
            SetAuthSessionDataCookies();
        }

        return uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var oldSession = _authSession.TakeSnapshot();

        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            _authProvider.HandleOAuthCallbackAsync(_authSession, request, ct), cancellationToken);
        _authSession.AuthToken = token;

        if (_authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies();
        }

        return token;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_authSession.AuthToken?.AccessToken))
        {
            await TimeoutHelper.WithTimeoutAsync(ct =>
                _authProvider.LogoutAsync(_authSession, ct), cancellationToken);
        }

        // Capture OAuth providers before clearing so we can delete their cookies
        var providersToDelete = _authSession.OAuthSessions.Keys.ToList();

        // Mark OAuth providers as removed globally so other tabs know not to re-add them
        foreach (var provider in providersToDelete)
        {
            _sessionStore.MarkOAuthProviderRemoved(_machineId!, provider);
        }

        _authSession.AuthToken = null;
        _authSession.ClearOAuthSessions();
        _removedOAuthSessions.Clear();

        // Pass the captured providers to delete their cookies
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession, _machineId!, providersToDelete);
        _client.SetAuthCookies(cookieJarId, reloadPage: true, triggerMachineReload: null);
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authProvider.GetAuthOptions();
    }

    public override async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var oldSession = _authSession.TakeSnapshot();
        var token = await base.RefreshAccessTokenAsync(cancellationToken);

        if (_authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies(reloadPage: _authSession.AuthToken == null);
        }

        return token;
    }

    public IAuthSession GetAuthSession() => _authSession;

    public async Task<OAuthSessionsResult> GetOAuthSessionsAsync(bool skipCache = false, CancellationToken cancellationToken = default)
    {
        var result = await TimeoutHelper.WithTimeoutAsync(ct =>
            _authProvider.GetOAuthSessionsAsync(_authSession, skipCache, ct), cancellationToken);

        if (result.Sessions == null)
        {
            return result;
        }

        // Filter to only include providers that have a registered handler
        var filteredSessions = _serviceProvider != null
            ? result.Sessions.Where(kvp => _serviceProvider.GetKeyedService<IAuthTokenHandler>(kvp.Key) != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            : result.Sessions;

        var unhandledSessions = result.Sessions?.Where(kvp => !filteredSessions.ContainsKey(kvp.Key)).Select(s => s.Key).ToList();
        if (unhandledSessions != null && unhandledSessions.Count > 0)
        {
            _logger.LogWarning("The following OAuth provider sessions are available but have no registered handler and will be ignored: {UnhandledProviders}", string.Join(", ", unhandledSessions));
        }

        // Diff and update _authSession.OAuthSessions
        var currentProviders = _authSession.OAuthSessions.Keys.ToHashSet();
        var newProviders = filteredSessions.Keys.ToHashSet();

        // Remove providers that are no longer present, but keep them in _removedOAuthSessions
        foreach (var provider in currentProviders.Where(p => !newProviders.Contains(p)))
        {
            if (_authSession.OAuthSessions.TryGetValue(provider, out var sessionToRemove))
            {
                _removedOAuthSessions[provider] = sessionToRemove;
            }
            _authSession.RemoveOAuthSession(provider);
        }

        // Add or update sessions
        bool hasChanges = false;
        foreach (var kvp in filteredSessions)
        {
            // Check if session exists in active sessions
            if (_authSession.OAuthSessions.TryGetValue(kvp.Key, out var existingSession))
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
                _authSession.AddOAuthSession(kvp.Key, removedSession);
                hasChanges = true;
            }
            else
            {
                // New session, add it
                _authSession.AddOAuthSession(kvp.Key, kvp.Value);
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
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession, _machineId!);
        _client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload);
    }
}
