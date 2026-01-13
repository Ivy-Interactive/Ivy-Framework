using Ivy.Core.Hooks;

namespace Ivy.Hooks;

public static class UseReducerExtensions
{
    public static (T value, Func<string, T> dispatch) UseReducer<T>(this IViewContext context, Func<T, string, T> reducer, T initialState)
    {
        var state = context.UseState(initialState);

        T Dispatch(string action) =>
            state.Set((prevState) => reducer(prevState, action));

        return (state.Value, Dispatch);
    }
}
