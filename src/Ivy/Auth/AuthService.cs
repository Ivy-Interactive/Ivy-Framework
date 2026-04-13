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

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var oldSession = _authSession.TakeSnapshot();

        var result = await TimeoutHelper.WithTimeoutAsync(ct =>
            _authProvider.LoginAsync(_authSession, email, password, ct), cancellationToken);
        _authSession.AuthToken = result.Token;

        if (_authSession.HasChangedSince(oldSession))
        {
            SetAuthCookies(reloadPage: _authSession.AuthToken != oldSession.AuthToken);
        }
        return result;
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
        // Capture OAuth providers before clearing so we can delete their cookies
        var providersToDelete = _authSession.BrokeredSessions.Keys.ToList();

        if (!string.IsNullOrWhiteSpace(_authSession.AuthToken?.AccessToken))
        {
            await TimeoutHelper.WithTimeoutAsync(ct =>
                _authProvider.LogoutAsync(_authSession, ct), cancellationToken);
        }

        _authSession.AuthToken = null;
        _authSession.ClearBrokeredSessions();

        // Pass the captured providers to delete their cookies
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession, providersToDelete);
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

    public async Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(bool skipCache = false, CancellationToken cancellationToken = default)
    {
        var result = await TimeoutHelper.WithTimeoutAsync(ct =>
            _authProvider.GetBrokeredSessionsAsync(_authSession, skipCache, ct), cancellationToken);

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
            _logger.LogWarning("The following brokered auth sessions are available but have no registered handler and will be ignored: {UnhandledProviders}", string.Join(", ", unhandledSessions));
        }

        // Diff and update _authSession.BrokeredSessions
        var currentProviders = _authSession.BrokeredSessions.Keys.ToHashSet();
        var newProviders = filteredSessions.Keys.ToHashSet();

        // Remove providers that no longer exist
        var removedProviders = currentProviders.Except(newProviders).ToList();
        foreach (var provider in removedProviders)
        {
            _authSession.RemoveBrokeredSession(provider);
        }

        // Add or update sessions
        bool hasChanges = removedProviders.Count > 0;
        foreach (var kvp in filteredSessions)
        {
            // Check if session exists in active sessions
            if (_authSession.BrokeredSessions.TryGetValue(kvp.Key, out var existingSession))
            {
                // Update existing active session in place to preserve references
                existingSession.AuthToken = kvp.Value.AuthToken;
                existingSession.AuthSessionData = kvp.Value.AuthSessionData;
            }
            else
            {
                // New session, add it
                _authSession.AddBrokeredSession(kvp.Key, kvp.Value);
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            // Pass removed providers so their cookies get deleted
            var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession, removedProviders);
            _client.SetAuthCookies(cookieJarId, reloadPage: false, triggerMachineReload: null, triggerMachineBrokeredRefresh: true);
        }

        return BrokeredSessionsResult.Success(filteredSessions);
    }

    public void SetAuthCookies(bool reloadPage = true, bool? triggerMachineReload = null, bool triggerMachineBrokeredRefresh = false)
    {
        var cookieJarId = _sessionStore.RegisterAuthSessionCookies(_authSession);
        _client.SetAuthCookies(cookieJarId, reloadPage, triggerMachineReload, triggerMachineBrokeredRefresh);
    }
}
