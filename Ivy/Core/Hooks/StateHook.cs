namespace Ivy.Core.Hooks;

public class StateHook(int identity, Lazy<IAnyState> lazyState, bool renderOnChange)
{
    public int Identity { get; } = identity;

    public IAnyState State => lazyState.Value;

    public bool RenderOnChange { get; } = renderOnChange;

    public static StateHook Create(int identity, Func<IAnyState> stateFactory, bool renderOnChange) =>
    new(identity, new Lazy<IAnyState>(stateFactory), renderOnChange);
}