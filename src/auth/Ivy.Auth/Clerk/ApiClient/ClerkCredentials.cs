using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk.ApiClient;

public class ClerkCredentials
{
    public string? DevBrowserToken { get; set; }
    public string? SessionToken { get; set; }

    public ClerkSession? Session { get; set; }
}
