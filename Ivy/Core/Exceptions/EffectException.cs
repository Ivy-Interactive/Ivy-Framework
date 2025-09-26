using Ivy.Core.Hooks;

namespace Ivy.Core.Exceptions;

/// <summary>
/// Exception that wraps errors occurring during effect hook execution.
/// </summary>
/// <param name="effectHook">The effect hook that caused the exception.</param>
/// <param name="innerException">The underlying exception that occurred during effect execution.</param>
public class EffectException(EffectHook effectHook, Exception? innerException = null) : Exception(null, innerException)
{
    /// <summary>Gets the effect hook that caused this exception.</summary>
    public EffectHook EffectHook { get; } = effectHook;
}