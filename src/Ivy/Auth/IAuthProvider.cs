using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthProvider
{
    Task InitializeAsync(IAuthSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, CallbackEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default);

    Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default);
}

public record CallbackEndpoint(string Id, string BaseUrl)
{
    public CallbackEndpoint(string id, string scheme, string host) : this(id, BuildBaseUrl(scheme, host))
    {
    }

    public static string BuildBaseUrl(string scheme, string host) => $"{scheme}://{host}/ivy/webhook";

    public Uri GetUri(bool includeIdInPath = true) => includeIdInPath
        ? new Uri($"{BaseUrl}/{Id}")
        : new Uri(BaseUrl);
}

