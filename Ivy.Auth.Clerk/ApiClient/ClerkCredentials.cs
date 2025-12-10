namespace Ivy.Auth.Clerk.ApiClient;

public class ClerkCredentials
{
    public string? ClientToken { get; set; }
    public string? DevBrowserToken { get; set; }
    public string? SessionToken { get; set; }

    public bool ClientTokenIsDirty { get; private set; }

    public void MarkClientTokenAsDirty()
    {
        ClientTokenIsDirty = true;
    }

    public bool ClearClientTokenDirtyFlag()
    {
        var wasDirty = ClientTokenIsDirty;
        ClientTokenIsDirty = false;
        return wasDirty;
    }
}
