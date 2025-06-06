using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public interface IAuthService
{
    Task<string?> LoginAsync(string email, string password);

    Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri);
    
    Task<string> HandleOAuthCallbackAsync(HttpRequest request);
    
    Task LogoutAsync();
    
    Task<UserInfo?> GetUserInfoAsync();
    
    AuthOption[] GetAuthOptions();
}