namespace Ivy.Views.Builders;

public class FuncBuilder<TModel, TIn>(Func<TIn, object?> func) : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        return func((TIn)value!);
    }
}