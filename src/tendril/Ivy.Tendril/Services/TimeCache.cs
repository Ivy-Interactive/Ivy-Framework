namespace Ivy.Tendril.Services;

/// <summary>
/// Generic time-based cache that stores a value with an expiration time.
/// Thread-safe for single-writer scenarios (typical for service instances).
///
/// Use GetOrCompute for synchronous computations and GetOrComputeAsync for async operations.
/// </summary>
/// <typeparam name="T">Type of cached value. Use nullable types for optional data.</typeparam>
public class TimeCache<T>
{
    private T? _value;
    private DateTime? _timestamp;
    private readonly TimeSpan _expiration;

    public TimeCache(TimeSpan expiration)
    {
        _expiration = expiration;
    }

    /// <summary>
    /// Gets the cached value if still valid, otherwise computes and caches a new value.
    /// </summary>
    /// <param name="compute">Function to compute the value if cache is expired.</param>
    /// <returns>The cached or newly computed value.</returns>
    public T GetOrCompute(Func<T> compute)
    {
        if (_value != null &&
            _timestamp != null &&
            DateTime.UtcNow - _timestamp.Value < _expiration)
        {
            return _value;
        }

        var result = compute();
        _value = result;
        _timestamp = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Gets the cached value if still valid, otherwise computes and caches a new value asynchronously.
    /// </summary>
    /// <param name="computeAsync">Async function to compute the value if cache is expired.</param>
    /// <returns>The cached or newly computed value.</returns>
    public async Task<T> GetOrComputeAsync(Func<Task<T>> computeAsync)
    {
        if (_value != null &&
            _timestamp != null &&
            DateTime.UtcNow - _timestamp.Value < _expiration)
        {
            return _value;
        }

        var result = await computeAsync();
        _value = result;
        _timestamp = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Invalidates the cache, forcing the next GetOrCompute to recompute.
    /// </summary>
    public void Invalidate()
    {
        _value = default;
        _timestamp = null;
    }

    /// <summary>
    /// Gets whether the cache currently holds a valid value.
    /// </summary>
    public bool IsValid =>
        _value != null &&
        _timestamp != null &&
        DateTime.UtcNow - _timestamp.Value < _expiration;
}
