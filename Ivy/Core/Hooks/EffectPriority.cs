namespace Ivy.Core.Hooks;

/// <summary>
/// Specifies the execution priority for effects in the effect queue processing pipeline.
/// </summary>
public enum EffectPriority
{
    /// <summary>Effects that execute immediately after state changes.</summary>
    StateChange,
    /// <summary>Effects that execute after the virtual DOM is updated.</summary>
    AfterRender,
    /// <summary>Effects that execute during component initialization.</summary>
    AfterInit
}