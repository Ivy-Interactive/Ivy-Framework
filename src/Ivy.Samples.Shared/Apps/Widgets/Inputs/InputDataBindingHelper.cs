
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

internal static class InputDataBindingHelper
{
    public static object CreateInputVariants(
        object state,
        Func<IAnyState, object> nonNullableFactory,
        Func<IAnyState, object> nullableFactory)
    {
        if (state is not IAnyState anyState)
            return Text.Block("Not an IAnyState");

        var stateType = anyState.GetStateType();
        var isNullable = stateType.IsNullableType();

        return isNullable ? nullableFactory(anyState) : nonNullableFactory(anyState);
    }
}
