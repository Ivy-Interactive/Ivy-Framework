using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Audio recorder control allowing users to upload audio using microphone with configurable upload intervals.</summary>
public record AudioRecorder : WidgetBase<AudioRecorder>
{
    /// <summary>Initializes AudioRecorder with basic configuration.</summary>
    /// <param name="label">Label text displayed when no audio is recording.</param>
    /// <param name="recordingLabel">Label text displayed when audio is recording.</param>
    /// <param name="mimeType">Mime type of recorded audio data (e.g., "audio/webm").</param>
    /// <param name="chunkInterval">Chunk size in milliseconds for continuous uploads. If null, uploads when recording stops.</param>
    /// <param name="uploadUrl">Upload URL for automatic audio file uploads.</param>
    /// <param name="disabled">Whether widget should be disabled initially.</param>
    public AudioRecorder(string? label = null, string? recordingLabel = null, string mimeType = "audio/webm", int? chunkInterval = null, string? uploadUrl = null, bool disabled = false)
    {
        Label = label;
        RecordingLabel = recordingLabel;
        MimeType = mimeType;
        ChunkInterval = chunkInterval;
        UploadUrl = uploadUrl;
        Disabled = disabled;
    }

    /// <summary>Gets or sets whether the widget is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the label text displayed when no audio is recording.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Gets or sets the label text displayed when audio is recording.</summary>
    [Prop] public string? RecordingLabel { get; set; }

    /// <summary>Gets or sets the mime type used for recorded audio.</summary>
    [Prop] public string MimeType { get; set; }

    /// <summary>Gets or sets the chunk size, in milliseconds, for continuous chunked uploads.</summary>
    [Prop] public int? ChunkInterval { get; set; }

    /// <summary>Gets or sets the upload URL for automatic audio file uploads.</summary>
    [Prop] public string? UploadUrl { get; set; }
}

/// <summary>Extension methods for configuring audio recorders.</summary>
public static class AudioRecorderExtensions
{
    /// <summary>Sets the label text to display when no audio is recording.</summary>
    /// <param name="widget">The audio recorder to configure.</param>
    /// <param name="label">The label text.</param>
    public static AudioRecorder Label(this AudioRecorder widget, string label)
    {
        return widget with { Label = label };
    }

    /// <summary>Sets the label text to display when audio is recording.</summary>
    /// <param name="widget">The audio recorder to configure.</param>
    /// <param name="label">The label text.</param>
    public static AudioRecorder RecordingLabel(this AudioRecorder widget, string label)
    {
        return widget with { RecordingLabel = label };
    }

    /// <summary>Sets the disabled state of the audio recorder.</summary>
    /// <param name="widget">The audio recorder to configure.</param>
    /// <param name="disabled">Whether the recorder should be disabled.</param>
    public static AudioRecorder Disabled(this AudioRecorder widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    /// <summary>Sets the mime type used for recorded audio.</summary>
    /// <param name="widget">The audio recorder to configure.</param>
    /// <param name="mimeType">Mime type to use (e.g., "audio/webm").</param>
    public static AudioRecorder MimeType(this AudioRecorder widget, string mimeType)
    {
        return widget with { MimeType = mimeType };
    }

    /// <summary>Sets the chunk size, in milliseconds, for continuous chunked uploads.</summary>
    /// <param name="widget">The audio recorder to configure.</param>
    /// <param name="chunkInterval">Chunk size to use, in milliseconds. If null, audio will only be uploaded when recording stops.</param>
    public static AudioRecorder ChunkInterval(this AudioRecorder widget, int? chunkInterval)
    {
        return widget with { ChunkInterval = chunkInterval };
    }

    /// <summary>Sets the upload URL for automatic audio file uploads.</summary>
    /// <param name="widget">The audio recorder to configure.</param>
    /// <param name="uploadUrl">The upload URL where audio chunks should automatically be uploaded.</param>
    public static AudioRecorder UploadUrl(this AudioRecorder widget, string? uploadUrl)
    {
        return widget with { UploadUrl = uploadUrl };
    }
}