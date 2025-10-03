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
public class AuthService(IAuthProvider authProvider, AuthToken? token) : IAuthService
{
    private AuthToken? _token = token;
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        var token = await authProvider.LoginAsync(email, password);
        _token = token;
        return token;
    }

    /// <summary>
    /// Generates an OAuth authorization URI for the specified option.
    /// </summary>
    /// <param name="option">The OAuth authentication option</param>
    /// <param name="callback">The webhook endpoint for handling the OAuth callback</param>
    /// <returns>The OAuth authorization URI</returns>
    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
    {
        return authProvider.GetOAuthUriAsync(option, callback);
    }

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var token = await authProvider.HandleOAuthCallbackAsync(request);
        _token = token;
        return token;
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    public async Task LogoutAsync()
    {
        if (string.IsNullOrWhiteSpace(_token?.Jwt))
        {
            return;
        }

        await authProvider.LogoutAsync(_token.Jwt);
        _token = null;
    }

    /// <summary>
    /// Retrieves information about the current authenticated user.
    /// </summary>
    /// <returns>User information if authenticated, null otherwise</returns>
    public async Task<UserInfo?> GetUserInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(_token?.Jwt))
        {
            return null!;
        }

        //todo: cache this!

        return await authProvider.GetUserInfoAsync(_token.Jwt);
    }

    /// <summary>
    /// Gets the available authentication options.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    public AuthOption[] GetAuthOptions()
    {
        return authProvider.GetAuthOptions();
    }
}
