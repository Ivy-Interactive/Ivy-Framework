namespace Ivy.Core.Hooks;

/// <summary>
/// Specifies when an effect should be triggered during the component lifecycle.
/// </summary>
public enum EffectTriggerType
{
    /// <summary>Trigger when a specific state value changes.</summary>
    AfterChange,
    /// <summary>Trigger once after component initialization.</summary>
    AfterInit,
    /// <summary>Trigger after each render cycle.</summary>
    AfterRender
}