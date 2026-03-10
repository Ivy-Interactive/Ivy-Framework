using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum QueryScope
{
    Server,
    View,
    App,
    Device,
    //User
}

public record QueryOptions
{
    public TimeSpan? Expiration { get; init; } = null;
    public QueryScope Scope { get; init; } = QueryScope.Server;

    /// <summary>
    /// Whether to revalidate data on mount/subscribe. Default: true (SWR pattern).
    /// When true: always revalidates in background (unless Expiration is set).
    /// When false with initialValue: populates cache with initialValue, no fetch.
    /// When false without initialValue: fetches once, no background revalidation.
    /// Useful for pre-populating from a parent query (e.g., list â†’ detail pattern).
    /// </summary>
    public bool RevalidateOnMount { get; init; } = true;

    /// <summary>
    /// When true, keeps showing previous data while fetching with a new key.
    /// Ideal for pagination to avoid loading flicker. Default: false.
    /// </summary>
    public bool KeepPrevious { get; init; } = false;

    /// <summary>
    /// Automatically revalidate data at this interval while there are active subscribers.
    /// Null disables polling. Example: TimeSpan.FromSeconds(30) for 30s polling.
    /// </summary>
    public TimeSpan? RefreshInterval { get; init; } = null;

    public static implicit operator QueryOptions(QueryScope scope) => new() { Scope = scope };
}

public record QueryServiceOptions
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

    /// <summary>
    /// How often to check for entries needing refresh. Default: 1 second.
    /// Lower values = more precise refresh timing but higher CPU usage.
    /// </summary>
    public TimeSpan RefreshTickInterval { get; init; } = TimeSpan.FromSeconds(1);
}

public record QueryMutator(
    Action Revalidate,
    Action Invalidate)
{
    public static QueryMutator Empty { get; } = new(
        static () => { },
        static () => { });
}

public delegate void MutateDelegate<TValue>(TValue? newValue, bool revalidate);

public record QueryMutator<TValue>(
    MutateDelegate<TValue> Mutate,
    Action Revalidate,
    Action Invalidate)
{
    public static implicit operator QueryMutator(QueryMutator<TValue> typed) =>
        new(typed.Revalidate, typed.Invalidate);
}

public record QueryResult<TValue>(
    TValue? Value,
    bool Loading,
    bool Validating,
    bool Previous,
    QueryMutator<TValue> Mutator,
    Exception? Error = null) : IEffectTriggerConvertible
{
    internal IAnyState? State { get; init; }

    public IEffectTrigger ToTrigger()
    {
        if (State is null)
            throw new InvalidOperationException("Cannot use an idle QueryResult as an effect trigger.");
        return EffectTrigger.OnStateChange(State);
    }
}

public static class QueryResultExtensions
{
    //Debug helper to visualize QueryResult state
    public static object? Build<TValue>(this QueryResult<TValue> qr, IViewContext _)
    {
        var states = Layout.Horizontal()
               | new Badge("IsLoading").Variant(qr.Loading ? BadgeVariant.Primary : BadgeVariant.Outline)
               | new Badge("IsValidating").Variant(qr.Validating ? BadgeVariant.Primary : BadgeVariant.Outline)
               | new Badge("IsPrevious").Variant(qr.Previous ? BadgeVariant.Primary : BadgeVariant.Outline)
            ;
        return Layout.Vertical()
               | states
               | qr.Error
            ;
    }
}


public static class UseQueryExtensions
{
    private static object UseScopedQueryKey(this IViewContext context, object key, QueryOptions options)
    {
        var appContext = context.UseService<Ivy.AppContext>();
        //var auth = context.UseService<IAuthService?>();

        if (options.Scope == QueryScope.View)
        {
            throw new InvalidOperationException("UseScopedQueryKey cannot be used with View scope.");
        }

        if (options.Scope == QueryScope.App)
        {
            key = (appContext.ConnectionId, key);
        }

        if (options.Scope == QueryScope.Device)
        {
            key = (appContext.MachineId, key);
        }

        // if (options.Scope == QueryScope.User)
        // {
        //     throw new NotImplementedException("User scope is not implemented yet. We need to implement GetUserId() in IAuthService.");
        //     var userId = auth?.GetUserId()
        //     if (userId is not null)
        //     {
        //         key = (userId, key);
        //     }
        // }

        return key;
    }

    /// <summary>
    /// Fetches and caches data with SWR-style revalidation.
    /// When key is null, returns an idle result without fetching (conditional fetching).
    /// </summary>
    public static QueryResult<TValue> UseQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull
    {
        var opts = options ?? new QueryOptions();

        var scopeRef = context.UseRef(() => opts.Scope);
        if (scopeRef.Value != opts.Scope)
            throw new InvalidOperationException("QueryScope cannot change between renders");

        if (opts.Scope == QueryScope.View)
        {
            return context.UseViewScopedQuery(key, fetcher, opts, initialValue);
        }

        // Server-scoped:
        var subscriberId = context.UseRef(Guid.NewGuid);
        var queryService = (QueryService)context.UseService<IQueryService>();
        var scopedKey = key is not null ? context.UseScopedQueryKey(key, opts) : null;
        var serializedScopedKey = scopedKey is not null ? QueryService.SerializeKey(scopedKey) : "";

        var subscriptionRef = context.UseRef<IDisposable?>(() => null);
        var prevQueryKeyRef = context.UseRef(() => key is not null ? serializedScopedKey : (string?)null);
        var hasInitialValueRef = context.UseRef(() => initialValue is not null);

        var emptyMutator = new QueryMutator<TValue>(
            static (_, _) => { },
            static () => { },
            static () => { });

        var mutator = key is not null
            ? new QueryMutator<TValue>(
                (newValue, revalidate) => queryService.Mutate<TValue>(serializedScopedKey, newValue, revalidate),
                () => queryService.Revalidate(serializedScopedKey),
                () => queryService.Invalidate(serializedScopedKey))
            : emptyMutator;

        var shouldSkipInitialFetch = !opts.RevalidateOnMount && hasInitialValueRef.Value;
        var initialIsLoading = key is not null && !shouldSkipInitialFetch;

        var resultState = context.UseState(
            () => new QueryResult<TValue>(initialValue, initialIsLoading, Validating: false, Previous: false, mutator)
        );

        var currentQueryKey = key is not null ? serializedScopedKey : (string?)null;
        var keyChanged = prevQueryKeyRef.Value != currentQueryKey;

        if (keyChanged)
        {
            // Dispose old subscription if exists
            subscriptionRef.Value?.Dispose();
            subscriptionRef.Value = null;

            // Subscribe if key is now non-null
            if (key is not null)
            {
                // Set loading state - keep previous data if configured
                if (!shouldSkipInitialFetch)
                {
                    if (opts.KeepPrevious && resultState.Value.Value is not null)
                    {
                        // Keep showing previous data while loading new data
                        resultState.Set(resultState.Value with
                        {
                            Mutator = mutator,
                            Loading = true,
                            Previous = true
                        });
                    }
                    else if (!resultState.Value.Loading)
                    {
                        resultState.Set(resultState.Value with { Mutator = mutator, Loading = true });
                    }
                }
                subscriptionRef.Value = queryService.Subscribe(resultState, subscriberId.Value, key, scopedKey!, serializedScopedKey, fetcher, opts, tags, initialValue);
            }

            prevQueryKeyRef.Value = currentQueryKey;
        }
        else if (key is not null && subscriptionRef.Value is null)
        {
            // First render with non-null key - always subscribe so entry exists for mutations
            subscriptionRef.Value = queryService.Subscribe(resultState, subscriberId.Value, key, scopedKey!, serializedScopedKey, fetcher, opts, tags, initialValue);
        }

        context.UseEffect(() => subscriptionRef.Value ?? Disposable.Empty);

        // Return idle state when key is null
        if (key is null)
        {
            return new QueryResult<TValue>(initialValue, Loading: false, Validating: false,
                Previous: false, emptyMutator);
        }

        return resultState.Value with { State = resultState };
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
        var shouldSkipInitialFetch = !opts.RevalidateOnMount && hasInitialValueRef.Value;
        var initialIsLoading = key is not null && !shouldSkipInitialFetch;

        var emptyMutator = new QueryMutator<TValue>(
            static (_, _) => { },
            static () => { },
            static () => { });

        var resultState = context.UseState(() =>
            new QueryResult<TValue>(initialValue, initialIsLoading, Validating: false, Previous: false, emptyMutator));

        var mutator = key is not null
            ? new QueryMutator<TValue>(
                (newValue, revalidate) =>
                {
                    if (revalidate)
                    {
                        fetchVersionRef.Value++;
                        resultState.Set(resultState.Value with { Value = newValue, Validating = true });
                    }
                    else
                    {
                        resultState.Set(resultState.Value with
                        {
                            Value = newValue,
                            Loading = false,
                            Validating = false,
                            Error = null
                        });
                    }
                },
                () =>
                {
                    fetchVersionRef.Value++;
                    resultState.Set(resultState.Value with { Validating = true });
                },
                () =>
                {
                    ctsRef.Value?.Cancel();
                    fetchVersionRef.Value++;
                    resultState.Set(new QueryResult<TValue>(default, true, false, false, resultState.Value.Mutator));
                })
            : emptyMutator;

        // Update mutator if needed
        if (key is not null && resultState.Value.Mutator == emptyMutator)
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

            // Set loading state - keep previous data if configured
            if (keyChanged && opts.KeepPrevious && resultState.Value.Value is not null)
            {
                // Keep showing previous data while loading new data
                resultState.Set(resultState.Value with
                {
                    Mutator = mutator,
                    Loading = true,
                    Previous = true
                });
            }
            else if (resultState.Value is { Loading: false, Value: null })
            {
                resultState.Set(resultState.Value with { Mutator = mutator, Loading = true });
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
                        resultState.Set(new QueryResult<TValue>(value, false, false, Previous: false, mutator));
                    }
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested && fetchVersionRef.Value == myVersion)
                    {
                        resultState.Set(new QueryResult<TValue>(resultState.Value.Value, false, false, Previous: false, mutator, ex));
                    }
                }
            });
        }

        // Cleanup on destroy
        context.UseEffect(() => new ViewQueryDisposable(ctsRef.Value ?? new CancellationTokenSource()));

        // Return idle state when key is null
        if (key is null)
        {
            return new QueryResult<TValue>(initialValue, Loading: false, Validating: false,
                Previous: false, emptyMutator);
        }

        // If skipping initial fetch with initialValue, return non-loading state
        if (shouldSkipInitialFetch && !hasFetchedRef.Value)
        {
            return new QueryResult<TValue>(initialValue, Loading: false, Validating: false, Previous: false, mutator) { State = resultState };
        }

        return resultState.Value with { State = resultState };
    }

    public static QueryMutator UseMutation(this IViewContext context, object key, QueryOptions? options = null)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        var opts = options ?? new QueryOptions();

        var scopeRef = context.UseRef(() => opts.Scope);
        if (scopeRef.Value != opts.Scope)
            throw new InvalidOperationException("QueryScope cannot change between renders.");
        if (opts.Scope == QueryScope.View)
            throw new ArgumentException("UseMutation does not support View scope.", nameof(options));

        var queryManager = (QueryService)context.UseService<IQueryService>();
        var scopedKey = context.UseScopedQueryKey(key, opts);
        var serializedScopedKey = QueryService.SerializeKey(scopedKey);

        return new QueryMutator(
            () => queryManager.Revalidate(serializedScopedKey),
            () => queryManager.Invalidate(serializedScopedKey));
    }

    public static QueryMutator<TValue> UseMutation<TValue, TKey>(this IViewContext context, TKey key, QueryOptions? options = null)
        where TKey : notnull
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        var opts = options ?? new QueryOptions();

        var scopeRef = context.UseRef(() => opts.Scope);
        if (scopeRef.Value != opts.Scope)
            throw new InvalidOperationException("QueryScope cannot change between renders.");
        if (opts.Scope == QueryScope.View)
            throw new ArgumentException("UseMutation does not support 'View' QueryScope.", nameof(options));

        var queryManager = (QueryService)context.UseService<IQueryService>();
        var scopedKey = context.UseScopedQueryKey(key, opts);
        var serializedScopedKey = QueryService.SerializeKey(scopedKey);

        return new QueryMutator<TValue>(
            (newValue, revalidate) => queryManager.Mutate<TValue>(serializedScopedKey, newValue, revalidate),
            () => queryManager.Revalidate(serializedScopedKey),
            () => queryManager.Invalidate(serializedScopedKey));
    }

    public static QueryResult<TValue> UseQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull
    {
        return context.UseQuery<TValue, TKey>(
            key,
            (_, ct) => fetcher(ct),
            options,
            initialValue,
            tags);
    }

    public static QueryResult<TValue> UseQuery<TValue, TKey>(this IViewContext context, TKey? key,
        Func<Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull
    {
        return context.UseQuery<TValue, TKey>(
            key,
            (_, __) => fetcher(),
            options,
            initialValue,
            tags);
    }

    public static QueryResult<TValue> UseQuery<TValue>(
        this IViewContext context,
        Func<CancellationToken, Task<TValue>> fetcher,
        QueryOptions? options = null,
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        // Use caller location as a stable, unique key per call site
        var key = $"{Path.GetFileName(callerFile)}:{callerLine}";
        return context.UseQuery<TValue, string>(
            key,
            (_, ct) => fetcher(ct),
            options,
            initialValue,
            tags);
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
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull
    {
        var key = keyFactory();
        return context.UseQuery(key, fetcher, options, initialValue, tags);
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
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull
    {
        var key = keyFactory();
        return context.UseQuery<TValue, TKey>(key, (_, ct) => fetcher(ct), options, initialValue, tags);
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
        TValue? initialValue = default,
        IReadOnlyList<object>? tags = null) where TKey : notnull
    {
        var key = keyFactory();
        return context.UseQuery<TValue, TKey>(key, (_, _) => fetcher(), options, initialValue, tags);
    }
}
