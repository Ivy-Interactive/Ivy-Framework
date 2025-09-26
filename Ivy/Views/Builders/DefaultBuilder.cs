namespace Ivy.Views.Builders;

//This is a default builder that does nothing, i.e. it lets the DefaultContentBuilder handle the value
public class DefaultBuilder<TModel> : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        return value;
    }
}