namespace Ivy.Core;

/// <summary>
/// Defines a contract for objects that support memoization by providing
/// their current state values for caching and optimization purposes.
/// </summary>
public interface IMemoized
{
    /// <summary>
    /// Retrieves an array of values that represent the current state
    /// of the implementing object. These values are used for memoization
    /// to determine if the object's state has changed and whether
    /// cached results can be reused.
    /// </summary>
    /// <returns>An array of objects representing the current state values for memoization.</returns>
    public object[] GetMemoValues();
}