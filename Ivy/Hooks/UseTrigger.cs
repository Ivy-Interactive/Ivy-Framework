using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Views;

namespace Ivy.Hooks;

public static class UseTriggerExtensions
{
    public static (object? triggerView, Action<T> triggerCallback) UseTrigger<TView, T>(this TView view, Func<IState<bool>, T, object?> viewFactory) where TView : ViewBase =>
        view.Context.UseTrigger(viewFactory);

    public static (object? triggerView, Action<T> triggerCallback) UseTrigger<T>(this IViewContext context, Func<IState<bool>, T, object?> factory)
    {
        var open = context.UseRef(false);
        var triggerValue = context.UseRef<T?>();

        var view = new FuncView(context2 =>
        {
            var openInternal = context2.UseState(false);

            context2.UseEffect(() =>
            {
                openInternal.Set(open.Value);
            }, open);

            return openInternal.Value && triggerValue.Value != null ? factory(open, triggerValue.Value) : null!;
        });

        return (
            view,
            Callback
        );

        void Callback(T? value)
        {
            triggerValue.Set(value);
            open.Set(true);
        }
    }

    public static (object? triggerView, Action triggerCallback) UseTrigger<TView>(this TView view, Func<IState<bool>, object?> viewFactory) where TView : ViewBase =>
        view.Context.UseTrigger(viewFactory);

    public static (object? triggerView, Action triggerCallback) UseTrigger(this IViewContext context, Func<IState<bool>, object?> factory)
    {
        var open = context.UseRef(false);

        var view = new FuncView(context2 =>
        {
            var openInternal = context2.UseState(false);

            context2.UseEffect(() =>
            {
                openInternal.Set(open.Value);
            }, open);

            return openInternal.Value ? factory(open) : null!;
        });

        return (view, Callback);

        void Callback()
        {
            open.Set(true);
        }
    }
}