// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAudioTranscriptionService
{
    Task<string> TranscribeAsync(Stream audioStream, string mimeType, string? language = null, CancellationToken ct = default);
}
