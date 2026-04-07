namespace Ivy.Tendril.Services;

/// <summary>
///     Generic time-based cache that stores a value with an expiration time.
///     Thread-safe.
///     Use GetOrCompute for synchronous computations and GetOrComputeAsync for async operations.
/// </summary>
/// <typeparam name="T">Type of cached value. Use nullable types for optional data.</typeparam>
public class TimeCache<T>
{
    private readonly TimeSpan _expiration;
    private readonly object _lock = new();
    private DateTime? _timestamp;
    private T? _value;

    public TimeCache(TimeSpan expiration)
    {
        _expiration = expiration;
    }

    /// <summary>
    ///     Gets whether the cache currently holds a valid value.
    /// </summary>
    public bool IsValid
    {
        get
        {
            lock (_lock)
            {
                return _timestamp != null &&
                       DateTime.UtcNow - _timestamp.Value < _expiration;
            }
        }
    }

    /// <summary>
    ///     Gets the cached value if still valid, otherwise computes and caches a new value.
    /// </summary>
    /// <param name="compute">Function to compute the value if cache is expired.</param>
    /// <returns>The cached or newly computed value.</returns>
    public T GetOrCompute(Func<T> compute)
    {
        lock (_lock)
        {
            if (_timestamp != null &&
                DateTime.UtcNow - _timestamp.Value < _expiration)
                return _value!;
        }

        var result = compute();

        lock (_lock)
        {
            _value = result;
            _timestamp = DateTime.UtcNow;
        }

        return result;
    }

    /// <summary>
    ///     Gets the cached value if still valid, otherwise computes and caches a new value asynchronously.
    /// </summary>
    /// <param name="computeAsync">Async function to compute the value if cache is expired.</param>
    /// <returns>The cached or newly computed value.</returns>
    public async Task<T> GetOrComputeAsync(Func<Task<T>> computeAsync)
    {
        lock (_lock)
        {
            if (_timestamp != null &&
                DateTime.UtcNow - _timestamp.Value < _expiration)
                return _value!;
        }

        var result = await computeAsync();

        lock (_lock)
        {
            _value = result;
            _timestamp = DateTime.UtcNow;
        }

        return result;
    }

    /// <summary>
    ///     Invalidates the cache, forcing the next GetOrCompute to recompute.
    /// </summary>
    public void Invalidate()
    {
        lock (_lock)
        {
            _value = default;
            _timestamp = null;
        }
    }
}