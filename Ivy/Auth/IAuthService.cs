using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

/// <summary>
/// Service interface for authentication operations in Ivy applications.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> LoginAsync(string email, string password);

    /// <summary>
    /// Generates an OAuth authorization URI for the specified option.
    /// </summary>
    /// <param name="option">The OAuth authentication option</param>
    /// <param name="callback">The webhook endpoint for handling the OAuth callback</param>
    /// <returns>The OAuth authorization URI</returns>
    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback);

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Retrieves information about the current authenticated user.
    /// </summary>
    /// <returns>User information if authenticated, null otherwise</returns>
    Task<UserInfo?> GetUserInfoAsync();

    /// <summary>
    /// Gets the available authentication options.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    AuthOption[] GetAuthOptions();
}