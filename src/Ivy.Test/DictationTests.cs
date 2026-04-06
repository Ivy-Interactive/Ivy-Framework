using System.Net;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Ivy.Test.TestHelpers;

namespace Ivy.Test;

public class DictationTests
{
    [Fact]
    public void EnableDictation_SetsDictationProps()
    {
        var state = new MockState<string>("hello");
        var widget = state.ToTextInput();
        var result = widget.EnableDictation();

        Assert.True(result.Dictation);
        Assert.Null(result.DictationLanguage);
    }

    [Fact]
    public void EnableDictation_WithLanguage_SetsDictationLanguage()
    {
        var state = new MockState<string>("");
        var widget = state.ToTextInput();
        var result = widget.EnableDictation(language: "de-DE");

        Assert.True(result.Dictation);
        Assert.Equal("de-DE", result.DictationLanguage);
    }

    [Fact]
    public async Task AzureSpeechTranscriptionService_TranscribeAsync_Success()
    {
        var responseJson = """{"RecognitionStatus":"Success","DisplayText":"Hello world","Offset":0,"Duration":0}""";
        var handler = new MockHttpHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var service = new AzureSpeechTranscriptionService("westeurope", "test-key", httpClient);

        using var audioStream = new MemoryStream(new byte[] { 0, 1, 2, 3 });
        var result = await service.TranscribeAsync(audioStream, "audio/webm", "en-US");

        Assert.Equal("Hello world", result);
        Assert.Contains("westeurope.stt.speech.microsoft.com", handler.LastRequestUri?.ToString());
        Assert.Contains("language=en-US", handler.LastRequestUri?.ToString());
        Assert.Equal("test-key", handler.LastRequest?.Headers.GetValues("Ocp-Apim-Subscription-Key").First());
    }

    [Fact]
    public async Task AzureSpeechTranscriptionService_TranscribeAsync_NoMatch_ReturnsEmpty()
    {
        var responseJson = """{"RecognitionStatus":"NoMatch","Offset":0,"Duration":0}""";
        var handler = new MockHttpHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var service = new AzureSpeechTranscriptionService("westeurope", "test-key", httpClient);

        using var audioStream = new MemoryStream(new byte[] { 0, 1, 2, 3 });
        var result = await service.TranscribeAsync(audioStream, "audio/webm");

        Assert.Equal("", result);
    }

    [Fact]
    public async Task AzureSpeechTranscriptionService_TranscribeAsync_DefaultLanguage()
    {
        var responseJson = """{"RecognitionStatus":"Success","DisplayText":"Test"}""";
        var handler = new MockHttpHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var service = new AzureSpeechTranscriptionService("eastus", "key123", httpClient);

        using var audioStream = new MemoryStream(new byte[] { 0 });
        await service.TranscribeAsync(audioStream, "audio/wav");

        Assert.Contains("language=en-US", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public void AzureSpeechTranscriptionService_Constructor_ThrowsOnEmptyRegion()
    {
        Assert.Throws<ArgumentException>(() => new AzureSpeechTranscriptionService("", "key"));
    }

    [Fact]
    public void AzureSpeechTranscriptionService_Constructor_ThrowsOnEmptyKey()
    {
        Assert.Throws<ArgumentException>(() => new AzureSpeechTranscriptionService("region", ""));
    }

    private class MockState<T>(T value) : IState<T>
    {
        private readonly Subject<T> _subject = new();
        public T Value { get; set; } = value;

        [OverloadResolutionPriority(1)]
        public T Set(T value) { Value = value; return Value; }
        public T Set(Func<T, T> setter) { Value = setter(Value); return Value; }
        public T Reset() => Set(default(T)!);
        public Type GetStateType() => typeof(T);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(Value);
            return _subject.Subscribe(observer);
        }

        public void Dispose() => _subject.Dispose();
        public IDisposable SubscribeAny(Action action) => _subject.Subscribe(_ => action());
        public IDisposable SubscribeAny(Action<object?> action) => _subject.Subscribe(x => action(x));
        public IEffectTrigger ToTrigger() => EffectTrigger.OnStateChange(this);
        public object? GetValueAsObject() => Value;
    }
}
