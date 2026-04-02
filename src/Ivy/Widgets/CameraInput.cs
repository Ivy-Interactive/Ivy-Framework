using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Allows users to capture photos from their device's webcam/camera.
/// </summary>
public record CameraInput : WidgetBase<CameraInput>, IAnyInput
{
    public CameraInput(UploadContext upload, string? placeholder = null, bool disabled = false)
    {
        UploadUrl = upload.UploadUrl;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    internal CameraInput() { }

    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public bool Nullable { get; set; }
    [Prop] public string? UploadUrl { get; set; }
    [Prop] public string FacingMode { get; set; } = "user";

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    public Type[] SupportedStateTypes() => [];
}

public static class CameraInputExtensions
{
    public static CameraInput Placeholder(this CameraInput widget, string placeholder)
        => widget with { Placeholder = placeholder };

    public static CameraInput Disabled(this CameraInput widget, bool disabled = true)
        => widget with { Disabled = disabled };

    public static CameraInput FacingMode(this CameraInput widget, string facingMode)
        => widget with { FacingMode = facingMode };

    public static CameraInput AutoFocus(this CameraInput widget, bool autoFocus = true)
        => widget with { AutoFocus = autoFocus };

    [OverloadResolutionPriority(1)]
    public static CameraInput OnBlur(this CameraInput widget, Func<Event<IAnyInput>, ValueTask> onBlur)
        => widget with { OnBlur = new(onBlur) };

    public static CameraInput OnBlur(this CameraInput widget, Action<Event<IAnyInput>> onBlur)
        => widget with { OnBlur = new(onBlur.ToValueTask()) };

    public static CameraInput OnBlur(this CameraInput widget, Action onBlur)
        => widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };

    [OverloadResolutionPriority(1)]
    public static CameraInput OnFocus(this CameraInput widget, Func<Event<IAnyInput>, ValueTask> onFocus)
        => widget with { OnFocus = new(onFocus) };

    public static CameraInput OnFocus(this CameraInput widget, Action<Event<IAnyInput>> onFocus)
        => widget with { OnFocus = new(onFocus.ToValueTask()) };

    public static CameraInput OnFocus(this CameraInput widget, Action onFocus)
        => widget with { OnFocus = new(_ => { onFocus(); return ValueTask.CompletedTask; }) };
}
