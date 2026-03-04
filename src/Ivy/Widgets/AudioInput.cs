using Ivy.Core;
using Ivy.Services;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Allows users to record audio directly from the browser.
/// </summary>
public record AudioInput : WidgetBase<AudioInput>
{
    public AudioInput(UploadContext upload, string? label = null, string? recordingLabel = null, string mimeType = "audio/webm", int? chunkInterval = null, bool disabled = false)
    {
        UploadUrl = upload.UploadUrl;
        Label = label;
        RecordingLabel = recordingLabel;
        MimeType = mimeType;
        ChunkInterval = chunkInterval;
        Disabled = disabled;
    }

    internal AudioInput() { }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Label { get; set; }

    [Prop] public string? RecordingLabel { get; set; }

    [Prop] public string MimeType { get; set; } = "audio/webm";

    [Prop] public int? ChunkInterval { get; set; }

    [Prop] public string? UploadUrl { get; set; }
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

    public static AudioInput UploadUrl(this AudioInput widget, string? uploadUrl)
    {
        return widget with { UploadUrl = uploadUrl };
    }
}
