using System.Collections.Concurrent;
using Ivy.Core.Hooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ivy.Hooks;

internal enum QueryEntryState
{
    Empty,        // No value, no fetch in progress
    Fetching,     // Initial fetch in progress, no cached value
    Fresh,        // Value present and within TTL
    Stale,        // Value present but past TTL
    Revalidating  // Stale value present, background refresh in progress
}

internal record QuerySubscriber(
    Guid Id,
    Action<object?, QueryEntryState, Exception?> OnStateChanged);

internal class QueryEntry
{
    public required string Key { get; init; }
    public required Type ValueType { get; init; }
    public object? Value { get; set; }
    public Exception? Error { get; set; }
    public QueryEntryState State { get; set; } = QueryEntryState.Empty;
    public Func<CancellationToken, Task<object?>>? Fetcher { get; set; }
    public QueryOptions Options { get; set; } = new();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastFetchedAt { get; set; }
    public DateTimeOffset LastAccessedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public ConcurrentDictionary<Guid, QuerySubscriber> Subscribers { get; } = new();
    public CancellationTokenSource? FetchCts { get; set; }
    public SemaphoreSlim Lock { get; } = new(1, 1);
    public bool HasSubscribers => !Subscribers.IsEmpty;
    public bool IsOrphaned => !HasSubscribers;
}

internal class QuerySubscription(QueryManager manager, string queryKey, Guid subscriberId) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        manager.Unsubscribe(queryKey, subscriberId);
    }
}

internal class ViewQueryDisposable(CancellationTokenSource cts) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            cts.Cancel();
            cts.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // CTS was already disposed by a subsequent effect run
        }
    }
}

public static class QueryManagerServiceExtensions
{
    public static IServiceCollection AddQueryManager(
        this IServiceCollection services,
        Action<QueryManagerOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<QueryManager>();

        return services;
    }
}

public class QueryManager : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, QueryEntry> _cache = new();
    private readonly ConcurrentDictionary<string, Task<object?>> _inflightRequests = new();

    private readonly ILogger<QueryManager> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly Timer _evictionTimer;
    private readonly QueryManagerOptions _options;

    private bool _disposed;

    public QueryManager(
        ILogger<QueryManager> logger,
        TimeProvider? timeProvider = null,
        IOptions<QueryManagerOptions>? options = null)
    {
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _options = options?.Value ?? new QueryManagerOptions();

        _evictionTimer = new Timer(
            callback: _ => EvictExpiredEntries(),
            state: null,
            dueTime: _options.EvictionInterval,
            period: _options.EvictionInterval);
    }

    /// <summary>
    /// Subscribe to a query entry. Returns an IDisposable that unsubscribes when disposed.
    /// Fetch is automatically cancelled when all subscribers dispose.
    /// </summary>
    public IDisposable Subscribe<TValue, TKey>(
        IState<QueryResult<TValue>> resultState,
        Guid subscriberId,
        TKey fetcherKey,
        string queryKey,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        QueryOptions options,
        TValue? initialValue = default)
    {
        var now = _timeProvider.GetUtcNow();

        // Wrap the typed fetcher to store in entry
        Func<CancellationToken, Task<object?>> wrappedFetcher = async ct =>
        {
            var result = await fetcher(fetcherKey, ct);
            return result;
        };

        // Get or create entry
        var entry = _cache.GetOrAdd(queryKey, _ => new QueryEntry
        {
            Key = queryKey,
            ValueType = typeof(TValue),
            CreatedAt = now,
            LastAccessedAt = now,
            Tags = options.Tags,
            Options = options,
            Fetcher = wrappedFetcher
        });

        // Update fetcher if this is a new subscription (fetcher might have changed)
        entry.Fetcher = wrappedFetcher;
        entry.Options = options;

        // Populate entry with initialValue when skipping initial fetch
        if (!options.RevalidateOnInit && initialValue is not null && entry.Value is null)
        {
            entry.Value = initialValue;
            entry.State = QueryEntryState.Fresh;
        }

        // Create subscriber callback
        var subscriber = new QuerySubscriber(subscriberId, (value, state, error) =>
        {
            var mutator = resultState.Value.Mutator;
            // Only loading if fetching, or empty with no error (initial state)
            var isLoading = state is QueryEntryState.Fetching || (state is QueryEntryState.Empty && error is null);
            var isValidating = state == QueryEntryState.Revalidating;
            // Clear IsPrevious when we have fresh data
            var isPrevious = state != QueryEntryState.Fresh && resultState.Value.IsPrevious;

            // When IsPrevious is set and we're still loading, preserve the previous value
            var resultValue = value is TValue typed
                ? typed
                : isPrevious ? resultState.Value.Value : default;

            resultState.Set(new QueryResult<TValue>(
                resultValue,
                isLoading,
                isValidating,
                isPrevious,
                mutator,
                error));
        });

        // Register subscriber
        entry.Subscribers[subscriberId] = subscriber;

        // Handle subscription based on current state
        _ = HandleSubscriptionAsync(entry, options);

        return new QuerySubscription(this, queryKey, subscriberId);
    }

    private async Task HandleSubscriptionAsync(QueryEntry entry, QueryOptions options)
    {
        await entry.Lock.WaitAsync();
        try
        {
            var now = _timeProvider.GetUtcNow();
            entry.LastAccessedAt = now;

            // Check staleness
            if (entry.State == QueryEntryState.Fresh &&
                entry.ExpiresAt.HasValue &&
                now > entry.ExpiresAt.Value)
            {
                entry.State = QueryEntryState.Stale;
            }

            switch (entry.State)
            {
                case QueryEntryState.Empty:
                    if (options.RevalidateOnInit)
                    {
                        await StartFetchAsync(entry);
                    }
                    // else: skip initial fetch, subscriber uses their initialValue
                    break;

                case QueryEntryState.Fresh:
                    NotifySubscribers(entry);
                    break;

                case QueryEntryState.Stale:
                    NotifySubscribers(entry); // Deliver stale immediately
                    await StartRevalidationAsync(entry);
                    break;

                case QueryEntryState.Fetching:
                case QueryEntryState.Revalidating:
                    // Already in progress, subscriber will be notified on completion
                    NotifySubscribers(entry);
                    break;
            }
        }
        finally
        {
            entry.Lock.Release();
        }
    }

    private async Task StartFetchAsync(QueryEntry entry)
    {
        if (entry.Fetcher is null) return;

        entry.FetchCts?.Dispose();
        entry.FetchCts = new CancellationTokenSource();
        entry.State = QueryEntryState.Fetching;
        entry.Error = null;

        NotifySubscribers(entry);

        var cts = entry.FetchCts;
        _ = ExecuteFetchAsync(entry, cts.Token);
    }

    private async Task StartRevalidationAsync(QueryEntry entry)
    {
        if (entry.Fetcher is null) return;
        if (entry.State == QueryEntryState.Revalidating) return; // Already revalidating

        entry.FetchCts?.Dispose();
        entry.FetchCts = new CancellationTokenSource();
        entry.State = QueryEntryState.Revalidating;

        NotifySubscribers(entry);

        var cts = entry.FetchCts;
        _ = ExecuteFetchAsync(entry, cts.Token);
    }

    private async Task ExecuteFetchAsync(QueryEntry entry, CancellationToken cancellationToken)
    {
        if (entry.Fetcher is null) return;

        try
        {
            // Deduplicate: if already in flight, await existing
            var fetchTask = _inflightRequests.GetOrAdd(entry.Key, _ => entry.Fetcher(cancellationToken));

            var value = await fetchTask;

            await entry.Lock.WaitAsync(CancellationToken.None);
            try
            {
                entry.Value = value;
                entry.Error = null;
                entry.State = QueryEntryState.Fresh;
                entry.LastFetchedAt = _timeProvider.GetUtcNow();
                entry.ExpiresAt = entry.Options.Expiration.HasValue
                    ? _timeProvider.GetUtcNow() + entry.Options.Expiration
                    : null;

                NotifySubscribers(entry);
            }
            finally
            {
                entry.Lock.Release();
                _inflightRequests.TryRemove(entry.Key, out _);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Fetch cancelled for query key {Key}: no subscribers", entry.Key);

            await entry.Lock.WaitAsync(CancellationToken.None);
            try
            {
                if (entry.State is QueryEntryState.Fetching or QueryEntryState.Revalidating)
                {
                    entry.State = entry.Value is not null
                        ? QueryEntryState.Stale
                        : QueryEntryState.Empty;
                }
            }
            finally
            {
                entry.Lock.Release();
                _inflightRequests.TryRemove(entry.Key, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Fetch failed for query key {Key}", entry.Key);

            await entry.Lock.WaitAsync(CancellationToken.None);
            try
            {
                entry.Error = ex;
                entry.State = entry.Value is not null
                    ? QueryEntryState.Stale
                    : QueryEntryState.Empty;

                NotifySubscribers(entry);
            }
            finally
            {
                entry.Lock.Release();
                _inflightRequests.TryRemove(entry.Key, out _);
            }
        }
    }

    private static void NotifySubscribers(QueryEntry entry)
    {
        foreach (var (_, subscriber) in entry.Subscribers)
        {
            try
            {
                subscriber.OnStateChanged(entry.Value, entry.State, entry.Error);
            }
            catch
            {
                // Don't let one subscriber break others
            }
        }
    }

    /// <summary>
    /// Optimistic update with optional revalidation.
    /// </summary>
    public void Mutate<TValue>(
        string queryKey,
        TValue? newValue = default,
        bool revalidate = true)
    {
        if (!_cache.TryGetValue(queryKey, out var entry))
            return;

        entry.Lock.Wait();
        try
        {
            entry.Value = newValue;
            entry.Error = null;
            entry.LastAccessedAt = _timeProvider.GetUtcNow();

            if (revalidate && entry.Fetcher is not null)
            {
                entry.FetchCts?.Cancel();
                entry.FetchCts?.Dispose();
                entry.FetchCts = new CancellationTokenSource();

                entry.State = QueryEntryState.Revalidating;
                NotifySubscribers(entry);
            }
            else
            {
                entry.State = QueryEntryState.Fresh;
                entry.LastFetchedAt = _timeProvider.GetUtcNow();
                entry.ExpiresAt = entry.Options.Expiration.HasValue
                    ? _timeProvider.GetUtcNow() + entry.Options.Expiration
                    : null;
                NotifySubscribers(entry);
            }
        }
        finally
        {
            entry.Lock.Release();
        }

        // Start fetch outside lock to avoid holding it during async operation
        if (revalidate && entry.Fetcher is not null)
        {
            _ = ExecuteFetchAsync(entry, entry.FetchCts!.Token);
        }
    }

    /// <summary>
    /// Force revalidation of a query entry.
    /// </summary>
    public void Revalidate(string queryKey)
    {
        if (!_cache.TryGetValue(queryKey, out var entry))
            return;

        if (entry.Fetcher is null)
            return;

        CancellationTokenSource cts;

        entry.Lock.Wait();
        try
        {
            entry.FetchCts?.Cancel();
            entry.FetchCts?.Dispose();
            cts = new CancellationTokenSource();
            entry.FetchCts = cts;

            var hadValue = entry.Value is not null;
            entry.State = hadValue ? QueryEntryState.Revalidating : QueryEntryState.Fetching;
            entry.Error = null;

            NotifySubscribers(entry);
        }
        finally
        {
            entry.Lock.Release();
        }

        _ = ExecuteFetchAsync(entry, cts.Token);
    }

    /// <summary>
    /// Invalidate a specific query entry. Clears the cached value and triggers a re-fetch if there are subscribers.
    /// </summary>
    public void Invalidate(string queryKey)
    {
        if (!_cache.TryGetValue(queryKey, out var entry))
            return;

        CancellationTokenSource? cts = null;

        entry.Lock.Wait();
        try
        {
            entry.FetchCts?.Cancel();
            entry.FetchCts?.Dispose();

            entry.Value = null;
            entry.Error = null;

            // If there are subscribers, start a fresh fetch
            if (entry.HasSubscribers && entry.Fetcher is not null)
            {
                cts = new CancellationTokenSource();
                entry.FetchCts = cts;
                entry.State = QueryEntryState.Fetching;
                NotifySubscribers(entry);
            }
            else
            {
                // No subscribers - just mark as empty, will be evicted later
                entry.State = QueryEntryState.Empty;
                NotifySubscribers(entry);
            }
        }
        finally
        {
            entry.Lock.Release();
        }

        if (cts is not null)
        {
            _ = ExecuteFetchAsync(entry, cts.Token);
        }
    }

    /// <summary>
    /// Invalidate all query entries with the specified tag.
    /// </summary>
    public void InvalidateByTag(string tag)
    {
        var keysToInvalidate = _cache
            .Where(kvp => kvp.Value.Tags.Contains(tag))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToInvalidate)
        {
            Invalidate(key);
        }
    }

    /// <summary>
    /// Revalidate all query entries with the specified tag.
    /// </summary>
    public void RevalidateByTag(string tag)
    {
        var keysToInvalidate = _cache
            .Where(kvp => kvp.Value.Tags.Contains(tag))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToInvalidate)
        {
            Revalidate(key);
        }
    }

    /// <summary>
    /// Clear all query entries.
    /// </summary>
    public void Clear()
    {
        var keys = _cache.Keys.ToList();
        foreach (var key in keys)
        {
            Invalidate(key);
        }
    }

    internal void Unsubscribe(string queryKey, Guid subscriberId)
    {
        if (!_cache.TryGetValue(queryKey, out var entry))
            return;

        entry.Subscribers.TryRemove(subscriberId, out _);

        // If no subscribers remain, cancel any in-flight fetch
        if (entry is { IsOrphaned: true, FetchCts.IsCancellationRequested: false })
        {
            _logger.LogDebug("Cancelling fetch for query key {Key}: no subscribers remain", queryKey);
            entry.FetchCts.Cancel();
        }
    }

    private void EvictExpiredEntries()
    {
        if (_disposed) return;

        var now = _timeProvider.GetUtcNow();
        var entriesToRemove = new List<string>();

        foreach (var (key, entry) in _cache)
        {
            var shouldEvict = false;

            // 1. Hard expiration: past TTL with no subscribers
            if (entry.ExpiresAt.HasValue &&
                now > entry.ExpiresAt.Value &&
                entry.IsOrphaned)
            {
                shouldEvict = true;
            }

            // 2. Orphaned too long: no subscribers for OrphanedEntryTtl
            if (entry.IsOrphaned &&
                now > entry.LastAccessedAt + _options.OrphanedEntryTtl)
            {
                shouldEvict = true;
            }

            // 3. Empty entries with no subscribers
            if (entry.State == QueryEntryState.Empty && entry.IsOrphaned)
            {
                shouldEvict = true;
            }

            if (shouldEvict)
            {
                entriesToRemove.Add(key);
            }
        }

        foreach (var key in entriesToRemove)
        {
            if (_cache.TryRemove(key, out var entry))
            {
                entry.FetchCts?.Dispose();
                entry.Lock.Dispose();
                _logger.LogDebug("Evicted query entry: {Key}", key);
            }
        }

        // LRU eviction if over limit
        if (_options.MaxEntries.HasValue && _cache.Count > _options.MaxEntries.Value)
        {
            EvictLru(_cache.Count - _options.MaxEntries.Value);
        }
    }

    private void EvictLru(int count)
    {
        var candidates = _cache
            .Where(kvp => kvp.Value.IsOrphaned)
            .OrderBy(kvp => kvp.Value.LastAccessedAt)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in candidates)
        {
            if (_cache.TryRemove(key, out var entry))
            {
                entry.FetchCts?.Dispose();
                entry.Lock.Dispose();
            }
        }

        if (candidates.Count > 0)
        {
            _logger.LogDebug("LRU evicted {Count} entries", candidates.Count);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _evictionTimer.Dispose();

        foreach (var entry in _cache.Values)
        {
            entry.FetchCts?.Dispose();
            entry.Lock.Dispose();
        }

        _cache.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _evictionTimer.DisposeAsync();

        foreach (var entry in _cache.Values)
        {
            entry.FetchCts?.Dispose();
            entry.Lock.Dispose();
        }

        _cache.Clear();
    }
}
