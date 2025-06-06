using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthProvider
{
     Task<AuthToken?> LoginAsync(string email, string password);

     Task LogoutAsync(string jwt);
     
     Task<bool> ValidateJwtAsync(string jwt);

     Task<UserInfo?> GetUserInfoAsync(string jwt);

     AuthOption[] GetAuthOptions();
     
     Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri);
     
     Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);
}