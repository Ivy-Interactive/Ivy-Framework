using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class AzureSpeechTranscriptionService : IAudioTranscriptionService
{
    private readonly string _region;
    private readonly string _subscriptionKey;
    private readonly HttpClient _httpClient;

    public AzureSpeechTranscriptionService(string region, string subscriptionKey, HttpClient? httpClient = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionKey);
        _region = region;
        _subscriptionKey = subscriptionKey;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string> TranscribeAsync(Stream audioStream, string mimeType, string? language = null, CancellationToken ct = default)
    {
        var lang = language ?? "en-US";
        var url = $"https://{_region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language={lang}";

        using var content = new StreamContent(audioStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(MapMimeType(mimeType));

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        request.Content = content;

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        var status = doc.RootElement.GetProperty("RecognitionStatus").GetString();
        if (status == "Success")
        {
            return doc.RootElement.GetProperty("DisplayText").GetString() ?? "";
        }

        return "";
    }

    private static string MapMimeType(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "audio/webm" or "audio/webm;codecs=opus" => "audio/webm",
            "audio/ogg" or "audio/ogg;codecs=opus" => "audio/ogg",
            "audio/wav" => "audio/wav",
            "audio/mp4" => "audio/mp4",
            "audio/aac" => "audio/aac",
            _ => mimeType
        };
    }
}

public static class SpeechServiceExtensions
{
    public static IServiceCollection AddAzureSpeechToText(this IServiceCollection services, string region, string subscriptionKey)
    {
        services.AddSingleton<IAudioTranscriptionService>(new AzureSpeechTranscriptionService(region, subscriptionKey));
        return services;
    }
}
