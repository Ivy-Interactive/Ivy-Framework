using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Allows users to record audio directly from the browser.
/// </summary>
public record AudioInput : WidgetBase<AudioInput>, IAnyInput
{
    public AudioInput(UploadContext upload, string? label = null, string? recordingLabel = null, string mimeType = "audio/webm", int? chunkInterval = null, int? sampleRate = null, bool disabled = false)
    {
        UploadUrl = upload.UploadUrl;
        Label = label;
        RecordingLabel = recordingLabel;
        MimeType = mimeType;
        ChunkInterval = chunkInterval;
        SampleRate = sampleRate;
        Disabled = disabled;
    }

    internal AudioInput() { }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Label { get; set; }

    [Prop] public string? RecordingLabel { get; set; }

    [Prop] public string MimeType { get; set; } = "audio/webm";

    [Prop] public int? ChunkInterval { get; set; }

    [Prop] public int? SampleRate { get; set; }

    [Prop] public string? UploadUrl { get; set; }

    [Prop] public string? Placeholder { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public bool Nullable { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [];
}

public static class AudioInputExtensions
{
    public static AudioInput Label(this AudioInput widget, string label)
    {
        return widget with { Label = label };
    }

    public static AudioInput RecordingLabel(this AudioInput widget, string label)
    {
        return widget with { RecordingLabel = label };
    }

    public static AudioInput Disabled(this AudioInput widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static AudioInput MimeType(this AudioInput widget, string mimeType)
    {
        return widget with { MimeType = mimeType };
    }

    public static AudioInput ChunkInterval(this AudioInput widget, int? chunkInterval)
    {
        return widget with { ChunkInterval = chunkInterval };
    }

    public static AudioInput SampleRate(this AudioInput widget, int? sampleRate)
    {
        return widget with { SampleRate = sampleRate };
    }

    public static AudioInput UploadUrl(this AudioInput widget, string? uploadUrl)
    {
        return widget with { UploadUrl = uploadUrl };
    }

    [OverloadResolutionPriority(1)]
    public static AudioInput OnBlur(this AudioInput widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static AudioInput OnBlur(this AudioInput widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static AudioInput OnBlur(this AudioInput widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }

    [OverloadResolutionPriority(1)]
    public static AudioInput OnFocus(this AudioInput widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static AudioInput OnFocus(this AudioInput widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget with { OnFocus = new(onFocus.ToValueTask()) };
    }

    public static AudioInput OnFocus(this AudioInput widget, Action onFocus)
    {
        return widget with { OnFocus = new(_ => { onFocus(); return ValueTask.CompletedTask; }) };
    }

    public static AudioInput AutoFocus(this AudioInput widget, bool autoFocus = true)
    {
        return widget with { AutoFocus = autoFocus };
    }
}
