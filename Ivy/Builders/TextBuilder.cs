using Ivy.Views;

namespace Ivy.Builders;

public class TextBuilder<TModel> : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        return value == null ? null : Text.Literal(value.ToString() ?? string.Empty);
    }
}