using Ivy.Core.Hooks;

namespace Ivy.Core.Exceptions;

public class EffectException(EffectHook effectHook, Exception? innerException = null) : Exception(null, innerException)
{
    public EffectHook EffectHook { get; } = effectHook;
}