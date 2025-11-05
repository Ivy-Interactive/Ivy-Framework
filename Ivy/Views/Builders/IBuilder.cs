namespace Ivy.Views.Builders;

public interface IBuilder<in TModel>
{
    public object? Build(object? value, TModel record);
}

// public static class BuilderExtensions
// {
//
//     public static IBuilder<TModel> ToBuilder<TModel>(this Func<object?, object?> func)
//     {
//         
//     }
//     
// }


// var x = (long e) => ""; // Func<long,string>
// var builder = x.ToBuilder() // Func<IBuilderFactory<TModel>,IBuilder<TModel>>



// public class TextBuilder<TModel> : IBuilder<TModel>
// {
//     public object? Build(object? value, TModel record)
//     {
//         return value == null ? null : Text.Literal(value.ToString() ?? string.Empty);
//     }
// }
