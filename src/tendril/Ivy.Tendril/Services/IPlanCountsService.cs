namespace Ivy.Tendril.Services;

public interface IPlanCountsService : IDisposable
{
    PlanCounts Current { get; }
    event Action? CountsChanged;
}