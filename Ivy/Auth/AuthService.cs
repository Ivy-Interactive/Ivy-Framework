using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public class AuthService(IAuthProvider authProvider, string? jwt) : IAuthService
{
    public async Task<string?> LoginAsync(string email, string password)
    {
        return await authProvider.LoginAsync(email, password);
    }

    public Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri)
    {
        return authProvider.GetOAuthUriAsync(optionId, callbackUri);
    }

    public Task<string> HandleOAuthCallbackAsync(HttpRequest request)
    { 
        return authProvider.HandleOAuthCallbackAsync(request);
    }

    public Task LogoutAsync()
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return null!;
        }
        
        return authProvider.LogoutAsync(jwt);
    }

    public async Task<UserInfo?> GetUserInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return null!;
        }
        
        //todo: cache this!
        
        return await authProvider.GetUserInfoAsync(jwt);
    }

    public AuthOption[] GetAuthOptions()
    {
        return authProvider.GetAuthOptions();
    }
}
