namespace Ivy.Services;

using System;

public class CompositeServiceProvider : IServiceProvider
{
    private readonly IServiceProvider[] _serviceProviders;

    public CompositeServiceProvider(params IServiceProvider[] serviceProviders)
    {
        if (serviceProviders == null || serviceProviders.Length == 0)
            throw new ArgumentNullException(nameof(serviceProviders));

        _serviceProviders = serviceProviders;
    }

    public object GetService(Type serviceType)
    {
        foreach (var provider in _serviceProviders)
        {
            var service = provider.GetService(serviceType);
            if (service != null)
                return service;
        }

        return null!;
    }
}
