using System.Collections.Immutable;

namespace Ivy.Core.Helpers;

public static class ImmutableArrayExtensions
{
    public static ImmutableArray<T> MoveUp<T>(this ImmutableArray<T> array, int index)
    {
        if (index <= 0 || index >= array.Length) return array;
        var item = array[index];
        array = array.RemoveAt(index);
        return array.Insert(index - 1, item);
    }

    public static ImmutableArray<T> MoveDown<T>(this ImmutableArray<T> array, int index)
    {
        if (index < 0 || index >= array.Length - 1) return array;
        var item = array[index];
        array = array.RemoveAt(index);
        return array.Insert(index + 1, item);
    }
}