using System.Collections.Concurrent;
using Ivy.Core.Hooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ivy.Hooks;

public class CacheManager : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<int, CacheEntry> _cache = new();
    private readonly ConcurrentDictionary<int, Task<object?>> _inflightRequests = new();

    private readonly ILogger<CacheManager> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly Timer _evictionTimer;
    private readonly CacheManagerOptions _options;

    private bool _disposed;

    public CacheManager(
        ILogger<CacheManager> logger,
        TimeProvider? timeProvider = null,
        IOptions<CacheManagerOptions>? options = null)
    {
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _options = options?.Value ?? new CacheManagerOptions();

        _evictionTimer = new Timer(
            callback: _ => EvictExpiredEntries(),
            state: null,
            dueTime: _options.EvictionInterval,
            period: _options.EvictionInterval);
    }

    /// <summary>
    /// Subscribe to a cache entry. Returns an IDisposable that unsubscribes when disposed.
    /// Fetch is automatically cancelled when all subscribers dispose.
    /// </summary>
    public IDisposable Subscribe<TValue, TKey>(
        IState<CacheResult<TValue>> resultState,
        Guid subscriberId,
        TKey fetcherKey,
        int cacheKey,
        Func<TKey, CancellationToken, Task<TValue>> fetcher,
        CacheOptions options)
    {
        var now = _timeProvider.GetUtcNow();

        // Wrap the typed fetcher to store in entry
        Func<CancellationToken, Task<object?>> wrappedFetcher = async ct =>
        {
            var result = await fetcher(fetcherKey, ct);
            return result;
        };

        // Get or create entry
        var entry = _cache.GetOrAdd(cacheKey, _ => new CacheEntry
        {
            Key = cacheKey,
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

        // Create subscriber callback
        var subscriber = new CacheSubscriber(subscriberId, (value, state, error) =>
        {
            var mutator = resultState.Value.Mutator;
            var isLoading = state is CacheEntryState.Fetching or CacheEntryState.Empty;
            var isValidating = state == CacheEntryState.Revalidating;

            resultState.Set(new CacheResult<TValue>(
                value is TValue typed ? typed : default,
                isLoading,
                isValidating,
                mutator,
                error));
        });

        // Register subscriber
        entry.Subscribers[subscriberId] = subscriber;

        // Handle subscription based on current state
        _ = HandleSubscriptionAsync(entry, options);

        return new CacheSubscription(this, cacheKey, subscriberId);
    }

    private async Task HandleSubscriptionAsync(CacheEntry entry, CacheOptions options)
    {
        await entry.Lock.WaitAsync();
        try
        {
            var now = _timeProvider.GetUtcNow();
            entry.LastAccessedAt = now;

            // Check staleness only for StaleWhileRevalidate strategy
            // CacheFirst ignores TTL - data stays "fresh" until explicitly invalidated
            if (options.Strategy == CacheStrategy.StaleWhileRevalidate &&
                entry.State == CacheEntryState.Fresh &&
                entry.ExpiresAt.HasValue &&
                now > entry.ExpiresAt.Value)
            {
                entry.State = CacheEntryState.Stale;
            }

            switch (entry.State)
            {
                case CacheEntryState.Empty:
                    await StartFetchAsync(entry);
                    break;

                case CacheEntryState.Fresh:
                    NotifySubscribers(entry);
                    break;

                case CacheEntryState.Stale when options.Strategy == CacheStrategy.StaleWhileRevalidate:
                    NotifySubscribers(entry); // Deliver stale immediately
                    await StartRevalidationAsync(entry);
                    break;

                case CacheEntryState.Stale: // CacheFirst - just return stale
                    NotifySubscribers(entry);
                    break;

                case CacheEntryState.Fetching:
                case CacheEntryState.Revalidating:
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

    private async Task StartFetchAsync(CacheEntry entry)
    {
        if (entry.Fetcher is null) return;

        entry.FetchCts?.Dispose();
        entry.FetchCts = new CancellationTokenSource();
        entry.State = CacheEntryState.Fetching;
        entry.Error = null;

        NotifySubscribers(entry);

        var cts = entry.FetchCts;
        _ = ExecuteFetchAsync(entry, cts.Token);
    }

    private async Task StartRevalidationAsync(CacheEntry entry)
    {
        if (entry.Fetcher is null) return;
        if (entry.State == CacheEntryState.Revalidating) return; // Already revalidating

        entry.FetchCts?.Dispose();
        entry.FetchCts = new CancellationTokenSource();
        entry.State = CacheEntryState.Revalidating;

        NotifySubscribers(entry);

        var cts = entry.FetchCts;
        _ = ExecuteFetchAsync(entry, cts.Token);
    }

    private async Task ExecuteFetchAsync(CacheEntry entry, CancellationToken cancellationToken)
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
                entry.State = CacheEntryState.Fresh;
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
            _logger.LogDebug("Fetch cancelled for cache key {Key}: no subscribers", entry.Key);

            await entry.Lock.WaitAsync(CancellationToken.None);
            try
            {
                if (entry.State is CacheEntryState.Fetching or CacheEntryState.Revalidating)
                {
                    entry.State = entry.Value is not null
                        ? CacheEntryState.Stale
                        : CacheEntryState.Empty;
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
            _logger.LogWarning(ex, "Fetch failed for cache key {Key}", entry.Key);

            await entry.Lock.WaitAsync(CancellationToken.None);
            try
            {
                entry.Error = ex;
                entry.State = entry.Value is not null
                    ? CacheEntryState.Stale
                    : CacheEntryState.Empty;

                NotifySubscribers(entry);
            }
            finally
            {
                entry.Lock.Release();
                _inflightRequests.TryRemove(entry.Key, out _);
            }
        }
    }

    private static void NotifySubscribers(CacheEntry entry)
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
    public async ValueTask MutateAsync<TValue>(
        int cacheKey,
        TValue? newValue = default,
        bool revalidate = true)
    {
        if (!_cache.TryGetValue(cacheKey, out var entry))
            return;

        await entry.Lock.WaitAsync();
        try
        {
            entry.Value = newValue;
            entry.Error = null;
            entry.LastAccessedAt = _timeProvider.GetUtcNow();

            if (revalidate && entry.Fetcher is not null)
            {
                entry.State = CacheEntryState.Revalidating;
                NotifySubscribers(entry);
                entry.Lock.Release();

                entry.FetchCts?.Dispose();
                entry.FetchCts = new CancellationTokenSource();
                _ = ExecuteFetchAsync(entry, entry.FetchCts.Token);
            }
            else
            {
                entry.State = CacheEntryState.Fresh;
                entry.LastFetchedAt = _timeProvider.GetUtcNow();
                entry.ExpiresAt = entry.Options.Expiration.HasValue
                    ? _timeProvider.GetUtcNow() + entry.Options.Expiration
                    : null;
                NotifySubscribers(entry);
            }
        }
        finally
        {
            if (entry.Lock.CurrentCount == 0)
                entry.Lock.Release();
        }
    }

    /// <summary>
    /// Force revalidation of a cache entry.
    /// </summary>
    public async ValueTask RevalidateAsync(int cacheKey)
    {
        if (!_cache.TryGetValue(cacheKey, out var entry))
            return;

        if (entry.Fetcher is null)
            return;

        await entry.Lock.WaitAsync();
        try
        {
            entry.FetchCts?.Dispose();
            entry.FetchCts = new CancellationTokenSource();

            var hadValue = entry.Value is not null;
            entry.State = hadValue ? CacheEntryState.Revalidating : CacheEntryState.Fetching;
            entry.Error = null;

            NotifySubscribers(entry);
        }
        finally
        {
            entry.Lock.Release();
        }

        _ = ExecuteFetchAsync(entry, entry.FetchCts!.Token);
    }

    /// <summary>
    /// Invalidate a specific cache entry. Clears the cached value and triggers a re-fetch if there are subscribers.
    /// </summary>
    public void Invalidate(int cacheKey)
    {
        if (!_cache.TryGetValue(cacheKey, out var entry))
            return;

        entry.FetchCts?.Cancel();
        entry.FetchCts?.Dispose();

        entry.Value = null;
        entry.Error = null;

        // If there are subscribers, start a fresh fetch
        if (entry.HasSubscribers && entry.Fetcher is not null)
        {
            entry.FetchCts = new CancellationTokenSource();
            entry.State = CacheEntryState.Fetching;
            NotifySubscribers(entry);
            _ = ExecuteFetchAsync(entry, entry.FetchCts.Token);
        }
        else
        {
            // No subscribers - just mark as empty, will be evicted later
            entry.State = CacheEntryState.Empty;
            NotifySubscribers(entry);
        }
    }

    /// <summary>
    /// Invalidate all cache entries with the specified tag.
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
    /// Clear all cache entries.
    /// </summary>
    public void Clear()
    {
        var keys = _cache.Keys.ToList();
        foreach (var key in keys)
        {
            Invalidate(key);
        }
    }

    internal void Unsubscribe(int cacheKey, Guid subscriberId)
    {
        if (!_cache.TryGetValue(cacheKey, out var entry))
            return;

        entry.Subscribers.TryRemove(subscriberId, out _);

        // If no subscribers remain, cancel any in-flight fetch
        if (entry is { IsOrphaned: true, FetchCts.IsCancellationRequested: false })
        {
            _logger.LogDebug("Cancelling fetch for cache key {Key}: no subscribers remain", cacheKey);
            entry.FetchCts.Cancel();
        }
    }

    private void EvictExpiredEntries()
    {
        if (_disposed) return;

        var now = _timeProvider.GetUtcNow();
        var entriesToRemove = new List<int>();

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
            if (entry.State == CacheEntryState.Empty && entry.IsOrphaned)
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
                _logger.LogDebug("Evicted cache entry: {Key}", key);
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