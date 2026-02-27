// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IOAuthTokenService : IAuthTokenHandlerService
{
    OAuthProvider Provider { get; }

    bool HasToken();

    void RemoveToken();
}
