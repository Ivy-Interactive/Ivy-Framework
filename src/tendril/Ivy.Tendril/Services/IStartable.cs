namespace Ivy.Tendril.Services;

/// <summary>
/// Marker interface for services that require an explicit Start() call after DI resolution.
/// Services implementing this will be automatically started by BackgroundServiceActivator.
/// </summary>
public interface IStartable
{
    void Start();
}
