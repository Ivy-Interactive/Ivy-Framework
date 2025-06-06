using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public interface IAuthProvider
{
     Task<string?> LoginAsync(string email, string password);

     Task LogoutAsync(string jwt);
     
     Task<bool> ValidateJwtAsync(string jwt);

     Task<UserInfo?> GetUserInfoAsync(string jwt);

     AuthOption[] GetAuthOptions();
     
     Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri);
     
     Task<string> HandleOAuthCallbackAsync(HttpRequest request);
}