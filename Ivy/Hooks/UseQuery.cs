using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Ivy.Core.Hooks;

namespace Ivy.Hooks;

public enum QueryStrategy
{
    StaleWhileRevalidate
}

public enum QueryScope
{
    Server,
    View
}

public record QueryOptions
{
    public QueryStrategy Strategy { get; init; } = QueryStrategy.StaleWhileRevalidate;
    public TimeSpan? Expiration { get; init; } = null;
    public QueryScope Scope { get; init; } = QueryScope.Server;
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Whether to fetch data on initial render. Default: true.
    /// When false and initialValue is provided, shows initialValue without fetching.
    /// Useful for pre-populating from a parent query (e.g., list → detail pattern).
    /// </summary>
    public bool RevalidateOnInit { get; init; } = true;

    public static implicit operator QueryOptions(QueryScope scope) => new() { Scope = scope };
    public static implicit operator QueryOptions(QueryStrategy strategy) => new() { Strategy = strategy };
}

public record QueryManagerOptions
{
    /// <summary>
    /// How often to scan for expired entries. Default: 60 seconds.
    /// </summary>
    public TimeSpan EvictionInterval { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// How long to keep entries with no subscribers before eviction. Default: 60 minutes.
    /// </summary>
    public TimeSpan OrphanedEntryTtl { get; init; } = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Maximum entries before LRU eviction kicks in. Default: 10,000. Null = unlimited.
    /// </summary>
    public int? MaxEntries { get; init; } = 10_000;
}

public record QueryMutator(
    string QueryKey,
    Action Revalidate,
    Action Invalidate)
{
    public static QueryMutator Empty { get; } = new(
        "",
        static () => { },
        static () => { });
}

public delegate void MutateDelegate<TValue>(TValue? newValue, bool revalidate);

public record QueryMutator<TValue>(
    string QueryKey,
    MutateDelegate<TValue> Mutate,
    Action Revalidate,
    Action Invalidate)
{
    public static QueryMutator<TValue> Empty { get; } = new(
        "",
        static (_, _) => { },
        static () => { },
        static () => { });

    public static implicit operator QueryMutator(QueryMutator<TValue> typed) =>
        new(typed.QueryKey, typed.Revalidate, typed.Invalidate);
}

public record QueryResult<TValue>(
    TValue? Value,
    bool IsLoading,
    bool IsValidating,
    QueryMutator<TValue> Mutator,
    Exception? Error = null);

public static class UseQueryExtensions
{
    private static string UseQueryKey(this IViewContext context, object key, QueryOptions options)
    {
        //todo: when we have more scopes we'll need to factor them in here
        return key switch
        {
            string s => s,
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => System.Text.Json.JsonSerializer.Serialize(key)
        };
    }

    /// <summary>
    /// Fetches and caches data with SWR-style revalidation.
    /// When key is null, returns an idle result without fetching (conditional fetching).
    /// </summary>
    public static QueryResult<TValue> UseQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default) where TKey : notnull
    {
        var opts = options ?? new QueryOptions();

        // Scope determines which hooks path - must be checked first for consistent hook ordering
        if (opts.Scope == QueryScope.View)
        {
            return context.UseViewScopedQuery(key, fetcher, opts, initialValue);
        }

        // Server-scoped: always call hooks in the same order (rules of hooks)
        var subscriberId = context.UseRef(Guid.NewGuid);
        var queryManager = context.UseService<QueryManager>();
        var queryKey = key is not null ? context.UseQueryKey(key, opts) : "";

        // Track subscription and previous key to manage subscription lifecycle
        var subscriptionRef = context.UseRef<IDisposable?>(() => null);
        var prevQueryKeyRef = context.UseRef(() => key is not null ? queryKey : (string?)null);
        var hasInitialValueRef = context.UseRef(() => initialValue is not null);

        var mutator = key is not null
            ? new QueryMutator<TValue>(
                queryKey,
                (newValue, revalidate) => queryManager.Mutate<TValue>(queryKey, newValue, revalidate),
                () => queryManager.Revalidate(queryKey),
                () => queryManager.Invalidate(queryKey))
            : QueryMutator<TValue>.Empty;

        // Determine initial loading state based on RevalidateOnInit
        var shouldSkipInitialFetch = !opts.RevalidateOnInit && hasInitialValueRef.Value;
        var initialIsLoading = key is not null && !shouldSkipInitialFetch;

        var resultState = context.UseState(
            () => new QueryResult<TValue>(initialValue, initialIsLoading, IsValidating: false, mutator)
        );

        // Manage subscription based on key changes
        var currentQueryKey = key is not null ? queryKey : (string?)null;
        var keyChanged = prevQueryKeyRef.Value != currentQueryKey;

        if (keyChanged)
        {
            // Dispose old subscription if exists
            subscriptionRef.Value?.Dispose();
            subscriptionRef.Value = null;

            // Subscribe if key is now non-null
            if (key is not null)
            {
                // Set loading state
                if (!resultState.Value.IsLoading && !shouldSkipInitialFetch)
                {
                    resultState.Set(resultState.Value with { Mutator = mutator, IsLoading = true });
                }
                subscriptionRef.Value = queryManager.Subscribe(resultState, subscriberId.Value, key, queryKey, fetcher, opts, initialValue);
            }

            prevQueryKeyRef.Value = currentQueryKey;
        }
        else if (key is not null && subscriptionRef.Value is null)
        {
            // First render with non-null key - always subscribe so entry exists for mutations
            subscriptionRef.Value = queryManager.Subscribe(resultState, subscriberId.Value, key, queryKey, fetcher, opts, initialValue);
        }

        // Cleanup on unmount
        context.UseEffect(() => subscriptionRef.Value ?? Disposable.Empty);

        // Return idle state when key is null
        if (key is null)
        {
            return new QueryResult<TValue>(initialValue, IsLoading: false, IsValidating: false,
                QueryMutator<TValue>.Empty);
        }

        return resultState.Value;
    }

    private static QueryResult<TValue> UseViewScopedQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions opts,
        TValue? initialValue) where TKey : notnull
    {
        var ctsRef = context.UseRef<CancellationTokenSource?>(() => null);
        var fetchVersionRef = context.UseRef(() => 0);
        var prevKeyRef = context.UseRef(() => key);
        var hasFetchedRef = context.UseRef(() => false);
        var hasInitialValueRef = context.UseRef(() => initialValue is not null);

        // Determine initial loading state based on RevalidateOnInit
        var shouldSkipInitialFetch = !opts.RevalidateOnInit && hasInitialValueRef.Value;
        var initialIsLoading = key is not null && !shouldSkipInitialFetch;

        var resultState = context.UseState(() =>
            new QueryResult<TValue>(initialValue, initialIsLoading, IsValidating: false, QueryMutator<TValue>.Empty));

        var mutator = key is not null
            ? new QueryMutator<TValue>(
                "",
                (newValue, revalidate) =>
                {
                    if (revalidate)
                    {
                        fetchVersionRef.Value++;
                        resultState.Set(resultState.Value with { Value = newValue, IsValidating = true });
                    }
                    else
                    {
                        resultState.Set(resultState.Value with
                        {
                            Value = newValue,
                            IsLoading = false,
                            IsValidating = false,
                            Error = null
                        });
                    }
                },
                () =>
                {
                    fetchVersionRef.Value++;
                    resultState.Set(resultState.Value with { IsValidating = true });
                },
                () =>
                {
                    ctsRef.Value?.Cancel();
                    fetchVersionRef.Value++;
                    resultState.Set(new QueryResult<TValue>(default, true, false, resultState.Value.Mutator));
                })
            : QueryMutator<TValue>.Empty;

        // Update mutator if needed
        if (key is not null && resultState.Value.Mutator == QueryMutator<TValue>.Empty)
        {
            resultState.Set(resultState.Value with { Mutator = mutator });
        }

        // Detect key changes and trigger fetch
        var keyChanged = !EqualityComparer<TKey?>.Default.Equals(prevKeyRef.Value, key);
        var needsFetch = key is not null && (keyChanged || !hasFetchedRef.Value) && !shouldSkipInitialFetch;

        if (keyChanged)
        {
            prevKeyRef.Value = key;

            // Cancel existing fetch
            if (ctsRef.Value is { } existingCts)
            {
                try { existingCts.Cancel(); existingCts.Dispose(); }
                catch (ObjectDisposedException) { }
            }
            ctsRef.Value = null;
        }

        if (needsFetch)
        {
            hasFetchedRef.Value = true;

            // Set loading state if not already
            if (!resultState.Value.IsLoading && resultState.Value.Value is null)
            {
                resultState.Set(resultState.Value with { Mutator = mutator, IsLoading = true });
            }

            // Start async fetch
            var cts = new CancellationTokenSource();
            var token = cts.Token; // Capture token before it might be disposed
            ctsRef.Value = cts;
            var myVersion = ++fetchVersionRef.Value;

            _ = Task.Run(async () =>
            {
                try
                {
                    var value = await fetcher(key!, token);

                    if (!token.IsCancellationRequested && fetchVersionRef.Value == myVersion)
                    {
                        resultState.Set(new QueryResult<TValue>(value, false, false, mutator));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Cancelled - ignore
                }
                catch (ObjectDisposedException)
                {
                    // CTS was disposed - ignore
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested && fetchVersionRef.Value == myVersion)
                    {
                        resultState.Set(new QueryResult<TValue>(resultState.Value.Value, false, false, mutator, ex));
                    }
                }
            });
        }

        // Cleanup on unmount
        context.UseEffect(() =>
        {
            return new ViewQueryDisposable(ctsRef.Value ?? new CancellationTokenSource());
        });

        // Return idle state when key is null
        if (key is null)
        {
            return new QueryResult<TValue>(initialValue, IsLoading: false, IsValidating: false,
                QueryMutator<TValue>.Empty);
        }

        // If skipping initial fetch with initialValue, return non-loading state
        if (shouldSkipInitialFetch && !hasFetchedRef.Value)
        {
            return new QueryResult<TValue>(initialValue, IsLoading: false, IsValidating: false, mutator);
        }

        return resultState.Value;
    }

    public static QueryMutator UseMutation(this IViewContext context, object key, QueryOptions? options = null)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        var opts = options ?? new QueryOptions();
        if (opts.Scope == QueryScope.View) throw new ArgumentException("UseMutation does not support View scope.", nameof(options));
        var queryManager = context.UseService<QueryManager>();
        var queryKey = context.UseQueryKey(key, opts);

        return new QueryMutator(
            queryKey,
            () => queryManager.Revalidate(queryKey),
            () => queryManager.Invalidate(queryKey));
    }

    public static QueryMutator<TValue> UseMutation<TValue>(this IViewContext context, object key, QueryOptions? options = null)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        var opts = options ?? new QueryOptions();
        if (opts.Scope == QueryScope.View) throw new ArgumentException("UseMutation does not support View scope.", nameof(options));
        var queryManager = context.UseService<QueryManager>();
        var queryKey = context.UseQueryKey(key, opts);

        return new QueryMutator<TValue>(
            queryKey,
            (newValue, revalidate) => queryManager.Mutate<TValue>(queryKey, newValue, revalidate),
            () => queryManager.Revalidate(queryKey),
            () => queryManager.Invalidate(queryKey));
    }

    public static QueryResult<TValue> UseQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default) where TKey : notnull
    {
        return context.UseQuery<TValue, TKey>(
            key,
            (_, ct) => fetcher(ct),
            options,
            initialValue);
    }

    public static QueryResult<TValue> UseQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default) where TKey : notnull
    {
        return context.UseQuery<TValue, TKey>(
            key,
            (_, __) => fetcher(),
            options,
            initialValue);
    }

    public static QueryResult<TValue> UseQuery<TValue>(
        this IViewContext context,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        // Use caller location as a stable, unique key per call site
        var key = $"{Path.GetFileName(callerFile)}:{callerLine}";
        return context.UseQuery<TValue, string>(
            key,
            (_, ct) => fetcher(ct),
            options,
            initialValue);
    }

    /// <summary>
    /// Fetches and caches data with a computed key for dependent fetching.
    /// When keyFactory returns null, returns an idle result without fetching.
    /// Re-evaluates the key on each render, enabling dependent data patterns.
    /// </summary>
    /// <example>
    /// var user = Context.UseQuery("user", FetchUser);
    /// var projects = Context.UseQuery(
    ///     () => user.Value?.Id,
    ///     async (userId, ct) => await FetchProjects(userId, ct));
    /// </example>
    public static QueryResult<TValue> UseQuery<TValue, TKey>(
        this IViewContext context,
        Func<TKey?> keyFactory,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default) where TKey : notnull
    {
        var key = keyFactory();
        return context.UseQuery(key, fetcher, options, initialValue);
    }

    /// <summary>
    /// Fetches and caches data with a computed key for dependent fetching.
    /// When keyFactory returns null, returns an idle result without fetching.
    /// </summary>
    public static QueryResult<TValue> UseQuery<TValue, TKey>(
        this IViewContext context,
        Func<TKey?> keyFactory,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default) where TKey : notnull
    {
        var key = keyFactory();
        return context.UseQuery<TValue, TKey>(key, (_, ct) => fetcher(ct), options, initialValue);
    }

    /// <summary>
    /// Fetches and caches data with a computed key for dependent fetching.
    /// When keyFactory returns null, returns an idle result without fetching.
    /// </summary>
    public static QueryResult<TValue> UseQuery<TValue, TKey>(
        this IViewContext context,
        Func<TKey?> keyFactory,
        Func<Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default) where TKey : notnull
    {
        var key = keyFactory();
        return context.UseQuery<TValue, TKey>(key, (_, _) => fetcher(), options, initialValue);
    }
}
