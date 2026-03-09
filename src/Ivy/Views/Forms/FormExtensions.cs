using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static partial class FormExtensions
{
    public static FormBuilder<T> ToForm<T>(this IState<T> obj, string submitTitle = "Save")
    {
        return new FormBuilder<T>(obj, submitTitle);
    }

    public static FormBuilder<T> ToForm<T>(this T obj, string submitTitle = "Save")
    {
        return new FormBuilder<T>(new State<T>(obj), submitTitle);
    }
}
