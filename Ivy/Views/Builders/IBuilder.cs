namespace Ivy.Views.Builders;

public interface IBuilder<in TModel>
{
    public object? Build(object? value, TModel record);
}