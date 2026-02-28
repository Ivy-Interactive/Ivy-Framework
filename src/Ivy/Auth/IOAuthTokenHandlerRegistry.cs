// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IOAuthTokenHandlerRegistry
{
    void Register(OAuthProvider provider, IAuthTokenHandler handler);

    IAuthTokenHandler? GetHandler(OAuthProvider provider);
}
