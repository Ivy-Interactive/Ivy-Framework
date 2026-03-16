using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Allows users to capture photos from their device's webcam/camera.
/// </summary>
public record CameraInput : WidgetBase<CameraInput>
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
    [Prop] public string? UploadUrl { get; set; }
    [Prop] public string FacingMode { get; set; } = "user";
}

public static class CameraInputExtensions
{
    public static CameraInput Placeholder(this CameraInput widget, string placeholder)
        => widget with { Placeholder = placeholder };

    public static CameraInput Disabled(this CameraInput widget, bool disabled = true)
        => widget with { Disabled = disabled };

    public static CameraInput FacingMode(this CameraInput widget, string facingMode)
        => widget with { FacingMode = facingMode };
}
