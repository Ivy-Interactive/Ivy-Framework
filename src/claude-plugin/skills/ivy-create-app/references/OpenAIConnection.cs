using System.ClientModel;
using Ivy;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace MyProject.Connections.OpenAI;

public class OpenAIConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => """
        # OpenAI Connection

        The OpenAI connection provides access to the full OpenAI REST API, including chat completions, embeddings, image generation, audio transcription, assistants, file management, and more.

        ## Getting the service

        ```csharp
        var client = UseService<OpenAIClient>();
        ```

        ## Chat Completions

        ```csharp
        using OpenAI.Chat;

        var chatClient = client.GetChatClient("gpt-4o");
        var completion = await chatClient.CompleteChatAsync("Hello, how are you?");
        Console.WriteLine(completion.Value.Content[0].Text);
        ```

        ## Chat Completions with Streaming

        ```csharp
        using OpenAI.Chat;

        var chatClient = client.GetChatClient("gpt-4o");
        await foreach (var update in chatClient.CompleteChatStreamingAsync("Tell me a story."))
        {
            if (update.ContentUpdate.Count > 0)
                Console.Write(update.ContentUpdate[0].Text);
        }
        ```

        ## Generate Embeddings

        ```csharp
        using OpenAI.Embeddings;

        var embeddingClient = client.GetEmbeddingClient("text-embedding-3-small");
        var embedding = await embeddingClient.GenerateEmbeddingAsync("Some text to embed");
        ReadOnlyMemory<float> vector = embedding.Value.ToFloats();
        ```

        ## Generate Images

        ```csharp
        using OpenAI.Images;

        var imageClient = client.GetImageClient("dall-e-3");
        var image = await imageClient.GenerateImageAsync("A sunset over mountains", new ImageGenerationOptions
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
        });
        Console.WriteLine(image.Value.ImageUri);
        ```

        ## Transcribe Audio

        ```csharp
        using OpenAI.Audio;

        var audioClient = client.GetAudioClient("whisper-1");
        var transcription = await audioClient.TranscribeAudioAsync("audio.mp3");
        Console.WriteLine(transcription.Value.Text);
        ```

        ## Text to Speech

        ```csharp
        using OpenAI.Audio;

        var audioClient = client.GetAudioClient("tts-1");
        var speech = await audioClient.GenerateSpeechAsync("Hello world!", GeneratedSpeechVoice.Alloy);
        // speech.Value contains the audio bytes
        ```

        ## Moderations

        ```csharp
        using OpenAI.Moderations;

        var moderationClient = client.GetModerationClient("omni-moderation-latest");
        var result = await moderationClient.ClassifyTextAsync("Some text to moderate");
        Console.WriteLine($"Flagged: {result.Value.Flagged}");
        ```

        ## List Models

        ```csharp
        using OpenAI.Models;

        var modelClient = client.GetOpenAIModelClient();
        var models = await modelClient.GetModelsAsync();
        foreach (var model in models.Value)
            Console.WriteLine(model.Id);
        ```

        ## The OpenAIClient provides sub-clients for each API area:
        - `GetChatClient(model)` — Chat completions
        - `GetEmbeddingClient(model)` — Text embeddings
        - `GetImageClient(model)` — Image generation
        - `GetAudioClient(model)` — Audio transcription and TTS
        - `GetAssistantClient()` — Assistants API
        - `GetOpenAIFileClient()` — File uploads
        - `GetOpenAIModelClient()` — Model listing
        - `GetModerationClient(model)` — Content moderation
        - `GetVectorStoreClient()` — Vector stores
        """;

    public string GetNamespace() => typeof(OpenAIConnection).Namespace!;

    public string GetName() => "OpenAI";

    public string GetConnectionType() => "Nuget:OpenAI";

    public ConnectionEntity[] GetEntities() =>
    [
        new("ChatCompletion", "ChatCompletions"),
        new("Embedding", "Embeddings"),
        new("Image", "Images"),
        new("AudioTranscription", "AudioTranscriptions"),
        new("Assistant", "Assistants"),
        new("File", "Files"),
        new("Model", "Models"),
        new("Moderation", "Moderations"),
    ];

    public void RegisterServices(Server server)
    {
        server.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var apiKey = config["OpenAI:ApiKey"] ?? "";
            var endpoint = config["OpenAI:Endpoint"] ?? "https://api.openai.com/v1";
            return new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        });

        server.Services.AddSingleton<IChatClient>(sp =>
        {
            var client = sp.GetRequiredService<OpenAIClient>();
            return client
                .GetChatClient("gpt-4o-mini")
                .AsIChatClient();
        });
    }

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        try
        {
            var apiKey = config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return (false, "OpenAI:ApiKey is not configured. Please set your API key in user secrets.");

            var endpoint = config["OpenAI:Endpoint"] ?? "https://api.openai.com/v1";
            var client = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
            var modelClient = client.GetOpenAIModelClient();
            var models = await modelClient.GetModelsAsync();
            var count = models.Value.Count;
            return (true, $"Connected successfully. Found {count} model(s) available.");
        }
        catch (Exception ex)
        {
            return (false, $"Connection test failed: {ex.Message}");
        }
    }

    public Secret[] GetSecrets() =>
    [
        new Secret("OpenAI:ApiKey"),
        new Secret("OpenAI:Endpoint", "https://api.openai.com/v1"),
    ];
}
