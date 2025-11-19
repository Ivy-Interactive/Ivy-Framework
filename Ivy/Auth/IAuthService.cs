using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthService
{
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <returns>User information if authenticated, null otherwise</returns>
    Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);

    AuthToken? GetCurrentToken();
}