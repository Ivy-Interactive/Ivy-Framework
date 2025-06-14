using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthService
{
    Task<AuthToken?> LoginAsync(string email, string password);

    Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri);
    
    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);
    
    Task LogoutAsync();
    
    Task<UserInfo?> GetUserInfoAsync();
    
    AuthOption[] GetAuthOptions();
}