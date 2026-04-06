// ReSharper disable once CheckNamespace
namespace Ivy;

public class RefreshToken(IState<(Guid, object?, bool)> state) : IEffectTriggerConvertible
{
    public object? ReturnValue => state.Value.Item2;

    public Guid Token => state.Value.Item1;

    public bool IsRefreshed => state.Value.Item3;

    public void Refresh(object? returnValue = null)
    {
        state.Set((Guid.NewGuid(), returnValue, true));
    }

    public IEffectTrigger ToTrigger()
    {
        return EffectTrigger.OnStateChange(state);
    }
}

public static class UseRefreshTokenExtensions
{
    public static RefreshToken UseRefreshToken(this IViewContext context)
    {
        var state = context.UseState(() => (Guid.NewGuid(), (object?)null, false));
        return new RefreshToken(state);
    }
}
