using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

/// <summary>
/// Default implementation of the authentication service that delegates to an auth provider.
/// </summary>
/// <param name="authProvider">The underlying authentication provider</param>
/// <param name="token">The current authentication token</param>
public class AuthService(IAuthProvider authProvider, AuthToken? token = null) : IAuthService
{
    private volatile AuthToken? _token = token;

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var token = await authProvider.LoginAsync(email, password, cancellationToken);
        _token = token;
        return token;
    }

    /// <summary>
    /// Generates an OAuth authorization URI for the specified option.
    /// </summary>
    /// <param name="option">The OAuth authentication option</param>
    /// <param name="callback">The webhook endpoint for handling the OAuth callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The OAuth authorization URI</returns>
    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        return authProvider.GetOAuthUriAsync(option, callback, cancellationToken);
    }

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var token = await authProvider.HandleOAuthCallbackAsync(request, cancellationToken);
        _token = token;
        return token;
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var token = Interlocked.Exchange(ref _token, null);

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return;
        }

        await authProvider.LogoutAsync(token.AccessToken, cancellationToken);
    }

    /// <summary>
    /// Retrieves information about the current authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if authenticated, null otherwise</returns>
    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        var token = _token;

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return null;
        }

        //todo: cache this!

        return await authProvider.GetUserInfoAsync(token.AccessToken, cancellationToken);
    }

    /// <summary>
    /// Gets the available authentication options.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    public AuthOption[] GetAuthOptions()
    {
        return authProvider.GetAuthOptions();
    }

    /// <summary>
    /// Refreshes the current authentication token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// The refreshed authentication token if successful; otherwise, null.
    /// </returns>
    public async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var token = _token;
        if (token is null)
        {
            return null;
        }

        var refreshedToken = await authProvider.RefreshAccessTokenAsync(token, cancellationToken);

        // Update _token only if it's still the same object we read earlier.
        var seen = Interlocked.CompareExchange(ref _token, refreshedToken, token);
        if (!ReferenceEquals(seen, token))
        {
            // Another thread updated the token in the meantime; return it if valid.
            if (seen is not null && await authProvider.ValidateAccessTokenAsync(seen.AccessToken, cancellationToken))
            {
                return seen;
            }

            // Otherwise, set and return null.
            _token = null;
            return null;
        }

        return refreshedToken;
    }

    /// <summary>
    /// Gets the current authentication token.
    /// </summary>
    /// <returns>The current authentication token if available, null otherwise</returns>
    public AuthToken? GetCurrentToken()
    {
        return _token;
    }
}
