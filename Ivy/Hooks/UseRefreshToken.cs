using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Hooks;

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
        return EffectTrigger.AfterChange(state);
    }
}

public static class UseRefreshTokenExtensions
{
    public static RefreshToken UseRefreshToken<TView>(this TView view) where TView : ViewBase
    {
        var state = view.Context.UseState(() => (Guid.NewGuid(), (object?)null, false));
        return new RefreshToken(state);
    }
}