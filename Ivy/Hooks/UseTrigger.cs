using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Hooks;

public static class UseTriggerExtensions
{
    public static (object? triggerView, Action<T> triggerCallback) UseTrigger<TView, T>(this TView view, Func<IState<bool>, T, object?> viewFactory) where TView : ViewBase =>
        view.Context.UseTrigger(viewFactory);

    public static (object? triggerView, Action<T> triggerCallback) UseTrigger<T>(this IViewContext context, Func<IState<bool>, T, object?> factory)
    {
        var open = context.UseState(false);
        var triggerValue = context.UseState<T?>();

        return (
            open.Value && triggerValue.Value != null ? factory(open, triggerValue.Value) : null!,
            value =>
            {
                open.Set(true);
                triggerValue.Set(value);
            }
        );
    }

    public static (object? triggerView, Action triggerCallback) UseTrigger<TView>(this TView view, Func<IState<bool>, object?> viewFactory) where TView : ViewBase =>
        view.Context.UseTrigger(viewFactory);

    public static (object? triggerView, Action triggerCallback) UseTrigger(this IViewContext context, Func<IState<bool>, object?> factory)
    {
        var open = context.UseState(false);
        return (
            open.Value ? factory(open) : null!,
            () =>
            {
                open.Set(true);
            }
        );
    }
}