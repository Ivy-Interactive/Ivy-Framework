using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseTriggerExtensions
{
    public static (object? triggerView, Action<T> triggerCallback) UseTrigger<T>(this IViewContext context, Func<IState<bool>, T, object?> factory)
    {
        var open = context.UseRef(false);
        var triggerValue = context.UseRef<T?>();
        var hasTriggered = context.UseRef(false);

        var view = new FuncView(context2 =>
        {
            var openInternal = context2.UseState(false);

            context2.UseEffect(() =>
            {
                openInternal.Set(open.Value);
            }, open);

            return openInternal.Value && hasTriggered.Value ? factory(open, triggerValue.Value!) : null!;
        });

        return (
            view,
            Callback
        );

        void Callback(T? value)
        {
            triggerValue.Set(value);
            hasTriggered.Set(true);
            open.Set(true);
        }
    }

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
