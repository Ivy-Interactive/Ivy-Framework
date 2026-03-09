// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IBuilder<in TModel>
{
    public object? Build(object? value, TModel record);
}
