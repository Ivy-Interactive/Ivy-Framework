using Ivy.Core;
using Ivy.Services;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Audio recorder control allowing users to upload audio using microphone with configurable upload intervals.</summary>
public record AudioRecorder : WidgetBase<AudioRecorder>
{
    /// <param name="upload">Upload context for automatic audio file uploads (from UseUpload).</param>
    /// <param name="label">Label text displayed when no audio is recording.</param>
    /// <param name="recordingLabel">Label text displayed when audio is recording.</param>
    /// <param name="mimeType">Mime type of recorded audio data (e.g., "audio/webm").</param>
    /// <param name="chunkInterval">Chunk size in milliseconds for continuous uploads. If null, uploads when recording stops.</param>
    /// <param name="disabled">Whether widget should be disabled initially.</param>
    public AudioRecorder(UploadContext upload, string? label = null, string? recordingLabel = null, string mimeType = "audio/webm", int? chunkInterval = null, bool disabled = false)
    {
        UploadUrl = upload.UploadUrl;
        Label = label;
        RecordingLabel = recordingLabel;
        MimeType = mimeType;
        ChunkInterval = chunkInterval;
        Disabled = disabled;
    }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Label { get; set; }

    [Prop] public string? RecordingLabel { get; set; }

    [Prop] public string MimeType { get; set; }

    [Prop] public int? ChunkInterval { get; set; }

    [Prop] public string? UploadUrl { get; set; }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;
}

public static class AudioRecorderExtensions
{
    public static AudioRecorder Label(this AudioRecorder widget, string label)
    {
        return widget with { Label = label };
    }

    public static AudioRecorder RecordingLabel(this AudioRecorder widget, string label)
    {
        return widget with { RecordingLabel = label };
    }

    public static AudioRecorder Disabled(this AudioRecorder widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static AudioRecorder MimeType(this AudioRecorder widget, string mimeType)
    {
        return widget with { MimeType = mimeType };
    }

    public static AudioRecorder ChunkInterval(this AudioRecorder widget, int? chunkInterval)
    {
        return widget with { ChunkInterval = chunkInterval };
    }

    public static AudioRecorder UploadUrl(this AudioRecorder widget, string? uploadUrl)
    {
        return widget with { UploadUrl = uploadUrl };
    }

    public static AudioRecorder Size(this AudioRecorder widget, Sizes size)
    {
        return widget with { Size = size };
    }

    public static AudioRecorder Large(this AudioRecorder widget)
    {
        return widget.Size(Sizes.Large);
    }

    public static AudioRecorder Small(this AudioRecorder widget)
    {
        return widget.Size(Sizes.Small);
    }
}
