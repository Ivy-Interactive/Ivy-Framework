// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IOAuthTokenService : IAuthTokenHandlerService
{
    string Provider { get; }

    bool HasToken();

    void RemoveToken();
}
