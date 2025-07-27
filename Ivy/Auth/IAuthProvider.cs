using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthProvider
{
    Task<AuthToken?> LoginAsync(string email, string password);

    Task LogoutAsync(string jwt);

    Task<AuthToken?> RefreshJwtAsync(AuthToken jwt);

    Task<bool> ValidateJwtAsync(string jwt);

    Task<UserInfo?> GetUserInfoAsync(string jwt);

    AuthOption[] GetAuthOptions();

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);
}