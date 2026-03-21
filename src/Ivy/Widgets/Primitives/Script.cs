using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Loads external JavaScript files or executes inline JavaScript code.
/// </summary>
public record Script : WidgetBase<Script>
{
    public Script(string? src = null)
    {
        Src = src;
    }

    internal Script() { }

    [Prop] public string? Src { get; set; }
    [Prop] public string? InlineCode { get; set; }
    [Prop] public bool Async { get; set; }
    [Prop] public bool Defer { get; set; }
    [Prop] public string? CrossOrigin { get; set; }
    [Prop] public string? Integrity { get; set; }
    [Prop] public string? ReferrerPolicy { get; set; }
}

public static class ScriptExtensions
{
    public static Script InlineCode(this Script script, string code) =>
        script with { InlineCode = code };

    public static Script Async(this Script script, bool async = true) =>
        script with { Async = async };

    public static Script Defer(this Script script, bool defer = true) =>
        script with { Defer = defer };

    public static Script CrossOrigin(this Script script, string crossOrigin) =>
        script with { CrossOrigin = crossOrigin };

    public static Script Integrity(this Script script, string integrity) =>
        script with { Integrity = integrity };

    public static Script ReferrerPolicy(this Script script, string referrerPolicy) =>
        script with { ReferrerPolicy = referrerPolicy };
}
