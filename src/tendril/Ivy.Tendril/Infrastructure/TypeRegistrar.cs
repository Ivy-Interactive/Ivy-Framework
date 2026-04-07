using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Ivy.Tendril.Infrastructure;

public class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build()
    {
        return new TypeResolver(services.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        services.AddSingleton(service, provider => factory());
    }
}

public class TypeResolver : ITypeResolver, IDisposable
{
    private readonly ServiceProvider _provider;

    public TypeResolver(ServiceProvider provider)
    {
        _provider = provider;
    }

    public object Resolve(Type? type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "Type cannot be null.");
        }
        return _provider.GetService(type)!;
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}
