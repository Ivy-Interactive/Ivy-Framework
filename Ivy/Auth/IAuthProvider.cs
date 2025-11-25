using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthProvider
{

    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(string token, CancellationToken cancellationToken = default);

    Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken = default);

    Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken = default);

    void SetHttpContext(HttpContext context)
    {
    }
}