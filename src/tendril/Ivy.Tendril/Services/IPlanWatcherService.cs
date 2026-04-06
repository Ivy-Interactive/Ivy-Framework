namespace Ivy.Tendril.Services;

public interface IPlanWatcherService : IDisposable
{
    event Action? PlansChanged;
}
