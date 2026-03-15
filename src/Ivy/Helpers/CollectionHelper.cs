using System.Dynamic;

namespace Ivy;

public static class CollectionHelper
{
    public static ExpandoObject[] ToExpando(this IEnumerable<IDictionary<string, object>> records) =>
        records.Select(r => r.ToExpando()).ToArray();

    public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
    {
        var expando = new ExpandoObject();
        IDictionary<string, object> expandoDict = expando as IDictionary<string, object>;
        foreach (var kvp in dictionary)
        {
            if (kvp.Value is IDictionary<string, object> nested)
                expandoDict[kvp.Key] = nested.ToExpando();
            else if (kvp.Value is IEnumerable<object> list)
                expandoDict[kvp.Key] = list.Select(item => item is IDictionary<string, object> d ? d.ToExpando() : item).ToList();
            else
                expandoDict[kvp.Key] = kvp.Value;
        }
        return expando;
    }

    //todo: this needs a rename
    public static async Task<List<T>> ToListAsync2<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (source is IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();
            await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }
            return list;
        }

        // Synchronous fallback without blocking on Task.Result
        return source.ToList();
    }

    public static async Task<T[]> ToArrayAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }
        return list.ToArray();
    }

    public static Action Before(this Action action, Action before)
    {
        return () =>
        {
            before();
            action();
        };
    }

    public static Action After(this Action action, Action after)
    {
        return () =>
        {
            action();
            after();
        };
    }
}
