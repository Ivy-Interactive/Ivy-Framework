# Dictation

TextInput supports speech-to-text dictation via the `.EnableDictation()` extension method. When enabled, a microphone icon appears in the input field. Clicking it starts recording audio from the user's microphone. When recording stops, the audio is sent to the server for transcription and the result is appended to the input value.

## Setup

Register an `IAudioTranscriptionService` implementation in `Program.cs`. Ivy includes a built-in Azure Speech Services implementation:

```csharp
server.Services.AddAzureSpeechToText(
    region: "westeurope",
    subscriptionKey: "your-subscription-key"
);
```

## Usage

```csharp
var text = UseState("");
return text.ToTextInput()
    .Placeholder("Click mic to dictate...")
    .EnableDictation();
```

### With Language

```csharp
text.ToTextInput().EnableDictation(language: "de-DE")
```

### With Textarea

```csharp
text.ToTextareaInput()
    .Placeholder("Dictate into a textarea...")
    .EnableDictation();
```

## Custom Transcription Service

Implement `IAudioTranscriptionService` for any speech-to-text provider:

```csharp
public class MyTranscriptionService : IAudioTranscriptionService
{
    public async Task<string> TranscribeAsync(
        Stream audioStream,
        string mimeType,
        string? language = null,
        CancellationToken ct = default)
    {
        // Send audio to your provider and return the transcribed text
    }
}

// Register in Program.cs
server.Services.AddSingleton<IAudioTranscriptionService>(new MyTranscriptionService());
```

## API Reference

| Method | Description |
|--------|-------------|
| `.EnableDictation(language?)` | Enable speech-to-text on a TextInput. Optional BCP-47 language tag (default: `en-US`). |

## Notes

- The browser will prompt for microphone permission on first use.
- Audio is recorded as WebM (or the best format supported by the browser) and uploaded to the server for transcription.
- The transcribed text is appended to the current input value.
- Dictation works with both single-line TextInput and multiline Textarea variants.
