using Ivy.Core;
using Ivy.Services;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record AudioRecorder : WidgetBase<AudioRecorder>
{
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
}
