// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IOAuthTokenHandlerRegistry
{
    void Register(string provider, IAuthTokenHandler handler);

    IAuthTokenHandler? GetHandler(string provider);
}
